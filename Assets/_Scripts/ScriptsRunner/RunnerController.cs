using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    [Header("Configuración General")]
    public float forwardSpeed = 10f;
    public float laneChangeSpeed = 20f; // Ajustado para el suavizado (Lerp)
    public float laneDistance = 2.5f;

    [Header("Configuración Física")]
    public float gravity = -20f;
    public float jumpForce = 8f;

    // Estado Privado
    private CharacterController controller;
    private int currentLane = 0; // 0 = Centro, -1 = Izq, 1 = Der
    private float verticalVelocity;

    // Variables para el movimiento lateral "Snappy" y Smooth
    private float currentLateralDistance = 0f;
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
        // 1. INPUT
        if (Input.GetKeyDown(KeyCode.A)) MoveLane(false);
        if (Input.GetKeyDown(KeyCode.D)) MoveLane(true);

        // 2. CÁLCULO DE POSICIÓN OBJETIVO
        float targetLateralDistance = currentLane * laneDistance;

        // --- MOVIMIENTO SUAVIZADO (LERP) ---
        float smoothedLateralDistance = Mathf.Lerp(currentLateralDistance, targetLateralDistance, laneChangeSpeed * Time.deltaTime);
        float moveDelta = smoothedLateralDistance - currentLateralDistance;
        currentLateralDistance = smoothedLateralDistance;

        // 3. MOVIMIENTO FINAL
        Vector3 moveVector = (forwardDirection * forwardSpeed * Time.deltaTime);
        moveVector += rightDirection * moveDelta;

        // 4. GRAVEDAD
        if (controller.isGrounded) verticalVelocity = -2f;
        else verticalVelocity += gravity * Time.deltaTime;
        moveVector.y = verticalVelocity * Time.deltaTime;

        controller.Move(moveVector);

        // 5. ROTACIÓN VISUAL
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

    void MoveLane(bool goingRight)
    {
        if (isInTurnTrigger)
        {
            TurnCorner(goingRight ? 90 : -90);
            return;
        }

        if (!isRotating)
        {
            currentLane += (goingRight ? 1 : -1);
            currentLane = Mathf.Clamp(currentLane, -1, 1);
        }
    }

    void TurnCorner(float angle)
    {
        if (isRotating) return;

        targetRotation *= Quaternion.Euler(0, angle, 0);
        forwardDirection = targetRotation * Vector3.forward;
        rightDirection = targetRotation * Vector3.right;

        currentLane = 0;
        currentLateralDistance = 0f;

        isRotating = true;
        isInTurnTrigger = false;
    }

    // --- DETECCIÓN DE TRIGGERS (GIRO Y GENERACIÓN) ---
    private void OnTriggerEnter(Collider other)
    {
        // Lógica de Giro
        if (other.CompareTag("TurnTrigger"))
        {
            isInTurnTrigger = true;
        }

        // Lógica de Generación Infinita (Punto 4 solicitado)
        // Cuando el jugador entra en un trigger de pieza nueva, el Spawner crea la siguiente.
        if (other.CompareTag("SpawnTrigger"))
        {
            TileSpawner spawner = Object.FindFirstObjectByType<TileSpawner>();
            if (spawner != null)
            {
                spawner.SpawnTile();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TurnTrigger"))
        {
            isInTurnTrigger = false;
        }
    }
}