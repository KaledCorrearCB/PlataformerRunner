using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Obligatorio para el contador de distancia

[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    [Header("Movimiento y Progresión")]
    public float initialSpeed = 12f;
    public float maxSpeed = 30f;
    public float speedIncreaseRate = 0.05f; // Cuánto aumenta la velocidad por segundo
    private float currentForwardSpeed;

    [Header("Control de Carriles")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 15f;

    [Header("Física y Salto")]
    public float gravity = -35f;
    public float jumpForce = 12f;
    public float fastFallSpeed = -20f;
    public float wallJumpUpForce = 12f;
    private float verticalVelocity;

    [Header("Wall Run")]
    public Vector3 wallJumpImpulse = new Vector3(5, 10, 0);
    public LayerMask wallRunLayer;
    public float wallCheckDistance = 1.5f;
    private bool isWallRunning = false;
    private bool lastWallRight = false;

    [Header("Crouch (Deslizamiento)")]
    public float slideHeight = 1f;
    private float originalHeight;
    private Vector3 originalCenter;

    [Header("UI y Puntuación")]
    public TextMeshProUGUI distanceText;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;

    // Variables internas de control
    private CharacterController controller;
    private int currentLane = 0;
    private Vector3 currentForward = Vector3.forward;
    private Vector3 currentRight = Vector3.right;
    private Vector3 pivotPoint; // Para evitar el retroceso en los giros

    private Quaternion targetRotation;
    private bool isRotating = false;
    private bool isInTurnTrigger = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetRotation = transform.rotation;
        currentForwardSpeed = initialSpeed;
        lastPosition = transform.position;
        pivotPoint = transform.position;

        // Configuración inicial del Controller
        originalHeight = controller.height;
        originalCenter = controller.center;
        controller.stepOffset = 0.1f;
    }

    void Update()
    {
        // 1. LÓGICA DE DISTANCIA
        // Calculamos la distancia real recorrida ignorando la altura (Y)
        float frameDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                               new Vector3(lastPosition.x, 0, lastPosition.z));
        distanceTraveled += frameDistance;
        lastPosition = transform.position;

        // Actualizar UI
        if (distanceText != null)
            distanceText.text = Mathf.FloorToInt(distanceTraveled).ToString() + "m";

        // 2. ACELERACIÓN PROGRESIVA
        if (currentForwardSpeed < maxSpeed)
        {
            currentForwardSpeed += speedIncreaseRate * Time.deltaTime;
        }

        // 3. ENTRADA DE USUARIO (INPUTS)
        HandleInputs();

        // 4. MOVIMIENTO Y FÍSICA
        MovePlayer();

        // Condición de muerte por caída
        if (transform.position.y < -5f) Die();
    }

    void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.A)) MoveLane(false);
        if (Input.GetKeyDown(KeyCode.D)) MoveLane(true);

        if (Input.GetButtonDown("Jump"))
        {
            if (controller.isGrounded)
                verticalVelocity = jumpForce;
            else if (isWallRunning)
                ExecuteWallJump();
        }

        // Agacharse o bajar rápido (S)
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!controller.isGrounded && !isWallRunning) verticalVelocity = fastFallSpeed;
            StartSlide();
        }
        if (Input.GetKeyUp(KeyCode.S)) StopSlide();
    }

    void MovePlayer()
    {
        // Movimiento Lateral (Limpio y Firme)
        Vector3 offsetFromPivot = transform.position - pivotPoint;
        float currentLateralPos = Vector3.Dot(offsetFromPivot, currentRight);
        float targetLateralPos = currentLane * laneDistance;
        float lateralDelta = targetLateralPos - currentLateralPos;
        Vector3 lateralMoveVector = currentRight * (lateralDelta * laneChangeSpeed);

        // Movimiento Adelante (Dinámico)
        Vector3 forwardMoveVector = currentForward * currentForwardSpeed;

        // Física Vertical
        CheckWallRun();
        ApplyPhysics();
        Vector3 verticalMoveVector = Vector3.up * verticalVelocity;

        // Ejecución Final
        controller.Move((forwardMoveVector + lateralMoveVector + verticalMoveVector) * Time.deltaTime);

        // Rotación Visual Suave
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 700f * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    void MoveLane(bool goingRight)
    {
        if (isInTurnTrigger) { TurnCorner(goingRight ? 90 : -90); return; }
        if (!isRotating)
        {
            currentLane = Mathf.Clamp(currentLane + (goingRight ? 1 : -1), -1, 1);
        }
    }

    void TurnCorner(float angle)
    {
        if (isRotating) return;

        // El pivote se mueve a la posición actual para resetear el eje lateral
        pivotPoint = transform.position;

        targetRotation *= Quaternion.Euler(0, angle, 0);
        currentForward = targetRotation * Vector3.forward;
        currentRight = targetRotation * Vector3.right;

        isRotating = true;
        isInTurnTrigger = false;

        // Al girar nos centramos en el nuevo pasillo
        currentLane = 0;
    }

    void CheckWallRun()
    {
        if (!controller.isGrounded)
        {
            bool wallLeft = Physics.Raycast(transform.position, -currentRight, wallCheckDistance, wallRunLayer);
            bool wallRight = Physics.Raycast(transform.position, currentRight, wallCheckDistance, wallRunLayer);

            if (wallLeft || wallRight)
            {
                isWallRunning = true;
                lastWallRight = wallRight;
                if (verticalVelocity < 0) verticalVelocity = 0;
                return;
            }
        }
        isWallRunning = false;
    }

    void ExecuteWallJump()
    {
        verticalVelocity = wallJumpUpForce;

        // Impulso lateral: saltamos al carril contrario de la pared
        if (lastWallRight) currentLane = -1;
        else currentLane = 1;

        isWallRunning = false;
    }

    void ApplyPhysics()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1f;
        }
        else
        {
            float currGravity = isWallRunning ? 0 : gravity;
            verticalVelocity += currGravity * Time.deltaTime;
        }
    }

    void StartSlide() { controller.height = slideHeight; controller.center = new Vector3(0, slideHeight / 2f, 0); }
    void StopSlide() { controller.height = originalHeight; controller.center = originalCenter; }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Detección de obstáculos frontales (Muerte)
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            if (Vector3.Dot(hit.normal, currentForward) < -0.6f) Die();
        }
    }

    void Die()
    {
        // Reinicia la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Triggers para el giro de 90 grados
    private void OnTriggerEnter(Collider other) { if (other.CompareTag("TurnTrigger")) isInTurnTrigger = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("TurnTrigger")) isInTurnTrigger = false; }
}