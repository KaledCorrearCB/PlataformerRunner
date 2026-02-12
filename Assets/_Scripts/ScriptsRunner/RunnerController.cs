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
        // 1. INPUT (Detectar teclas)
        if (Input.GetKeyDown(KeyCode.A)) MoveLane(false);
        if (Input.GetKeyDown(KeyCode.D)) MoveLane(true);

        // 2. CALCULAR OBJETIVO (A dónde debería estar)
        float targetLateralDistance = currentLane * laneDistance;

        // 3. MOVER LATERALMENTE (Calcular el delta/diferencia de este frame)
        // Esto es lo que arregla el patinaje: Mueve "hacia" el objetivo, no "con" el objetivo.
        float nextLateralDistance = Mathf.MoveTowards(currentLateralDistance, targetLateralDistance, laneChangeSpeed * Time.deltaTime);
        float moveDelta = nextLateralDistance - currentLateralDistance; // Cuánto me moví solo en este frame
        currentLateralDistance = nextLateralDistance;

        // 4. PREPARAR VECTORES DE MOVIMIENTO
        Vector3 moveVector = Vector3.zero;

        // A. Movimiento Hacia Adelante constante
        moveVector += forwardDirection * forwardSpeed * Time.deltaTime;

        // B. Movimiento Lateral (Solo lo que calculamos en el paso 3)
        moveVector += rightDirection * moveDelta;

        // C. Gravedad
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Mantener pegado al suelo
        }
        verticalVelocity += gravity * Time.deltaTime;
        moveVector.y = verticalVelocity * Time.deltaTime;

        // 5. APLICAR MOVIMIENTO FINAL
        controller.Move(moveVector);

        // 6. GESTIONAR LA ROTACIÓN DEL PERSONAJE
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