using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    [Header("Configuración General")]
    public float forwardSpeed = 10f;
    public float laneChangeSpeed = 30f; // Velocidad alta para efecto "Snappy"
    public float laneDistance = 2.5f;

    [Header("Configuración Física")]
    public float gravity = -20f;
    public float jumpForce = 8f; // Por si quieres saltar después

    // Estado Privado
    private CharacterController controller;
    private int currentLane = 0; // 0 = Centro, -1 = Izq, 1 = Der
    private float verticalVelocity;

    // Variables para el movimiento lateral "Snappy"
    private float currentLateralDistance = 0f; // Donde estoy realmente (en valores de carril)
    private Vector3 forwardDirection = Vector3.forward;
    private Vector3 rightDirection = Vector3.right;

    // Variables de Giro
    private bool isInTurnTrigger = false;
    private Quaternion targetRotation;
    private bool isRotating = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // 1. INPUT (Se mantiene igual)
        if (Input.GetKeyDown(KeyCode.A)) MoveLane(false);
        if (Input.GetKeyDown(KeyCode.D)) MoveLane(true);

        // 2. CÁLCULO DE POSICIÓN OBJETIVO
        float targetLateralDistance = currentLane * laneDistance;

        // --- CAMBIO PARA SUAVIZAR ---
        // Usamos Lerp para que el movimiento sea un porcentaje de la distancia restante.
        // El valor 0.1f controla qué tan rápido llega; puedes ajustarlo.
        float smoothedLateralDistance = Mathf.Lerp(currentLateralDistance, targetLateralDistance, laneChangeSpeed * Time.deltaTime);
        float moveDelta = smoothedLateralDistance - currentLateralDistance;
        currentLateralDistance = smoothedLateralDistance;
        // ----------------------------

        // 3. MOVIMIENTO FINAL
        Vector3 moveVector = (forwardDirection * forwardSpeed * Time.deltaTime);
        moveVector += rightDirection * moveDelta;

        // 4. GRAVEDAD
        if (controller.isGrounded) verticalVelocity = -2f;
        else verticalVelocity += gravity * Time.deltaTime;
        moveVector.y = verticalVelocity * Time.deltaTime;

        controller.Move(moveVector);

        // 5. ROTACIÓN (Se mantiene igual)
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500f * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    // Función para procesar el cambio de carril o giro
    void MoveLane(bool goingRight)
    {
        // Si estamos en la zona de giro, ¡GIRAMOS!
        if (isInTurnTrigger)
        {
            TurnCorner(goingRight ? 90 : -90);
            return;
        }

        // Si NO estamos en zona de giro, cambiamos de carril
        if (!isRotating)
        {
            currentLane += (goingRight ? 1 : -1);
            currentLane = Mathf.Clamp(currentLane, -1, 1);
        }
    }

    // Lógica de Giro de 90 Grados
    void TurnCorner(float angle)
    {
        if (isRotating) return;

        // 1. Calcular nueva rotación
        targetRotation *= Quaternion.Euler(0, angle, 0);

        // 2. Actualizar vectores de dirección (Vital para que 'Adelante' sea el nuevo 'Adelante')
        forwardDirection = targetRotation * Vector3.forward;
        rightDirection = targetRotation * Vector3.right;

        // 3. Resetear carriles (Al girar, aterrizas en el centro del nuevo camino)
        currentLane = 0;
        currentLateralDistance = 0f;

        isRotating = true;
        isInTurnTrigger = false; // Consumimos el trigger
    }

    // Detección del Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TurnTrigger")) isInTurnTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TurnTrigger")) isInTurnTrigger = false;
    }
}