using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float forwardSpeed = 12f;
    public float laneDistance = 3f;
    public float laneChangeSpeed = 15f;

    [Header("Física y Salto")]
    public float gravity = -35f;
    public float jumpForce = 12f;
    public float fastFallSpeed = -20f;
    public float wallJumpUpForce = 12f;
    private float verticalVelocity;

    [Header("Wall Run")]
    public LayerMask wallRunLayer;
    public float wallCheckDistance = 1.5f;
    private bool isWallRunning = false;
    private bool lastWallRight = false;

    [Header("Crouch")]
    public float slideHeight = 1f;
    private float originalHeight;
    private Vector3 originalCenter;

    private CharacterController controller;
    private int currentLane = 0;

    // El secreto: Estas variables se "limpian" en cada giro
    private Vector3 currentForward = Vector3.forward;
    private Vector3 currentRight = Vector3.right;
    private Vector3 pivotPoint; // Punto de referencia para el carril

    private Quaternion targetRotation;
    private bool isRotating = false;
    private bool isInTurnTrigger = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetRotation = transform.rotation;
        originalHeight = controller.height;
        originalCenter = controller.center;
        pivotPoint = transform.position; // El inicio es nuestro primer pivote
        controller.stepOffset = 0.1f;
    }

    void Update()
    {
        // 1. INPUTS
        if (Input.GetKeyDown(KeyCode.A)) MoveLane(false);
        if (Input.GetKeyDown(KeyCode.D)) MoveLane(true);

        if (Input.GetButtonDown("Jump"))
        {
            if (controller.isGrounded) verticalVelocity = jumpForce;
            else if (isWallRunning) ExecuteWallJump();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!controller.isGrounded && !isWallRunning) verticalVelocity = fastFallSpeed;
            StartSlide();
        }
        if (Input.GetKeyUp(KeyCode.S)) StopSlide();

        // 2. PARED
        CheckWallRun();

        // 3. MOVIMIENTO LATERAL (CARRIL LIMPIO)
        // Calculamos cuánto nos hemos alejado del PIVOTE en el eje derecho actual
        Vector3 offsetFromPivot = transform.position - pivotPoint;
        float currentLateralPos = Vector3.Dot(offsetFromPivot, currentRight);
        float targetLateralPos = currentLane * laneDistance;

        float lateralDelta = targetLateralPos - currentLateralPos;
        Vector3 lateralMove = currentRight * (lateralDelta * laneChangeSpeed);

        // 4. MOVIMIENTO ADELANTE Y FÍSICA
        Vector3 forwardMove = currentForward * forwardSpeed;
        ApplyPhysics();
        Vector3 verticalMove = Vector3.up * verticalVelocity;

        // 5. EJECUCIÓN
        controller.Move((forwardMove + lateralMove + verticalMove) * Time.deltaTime);

        // 6. ROTACIÓN
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 700f * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }

        if (transform.position.y < -5f) Die();
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

        // Antes de girar, actualizamos el pivote a nuestra posición actual
        // Esto "borra" la memoria del carril anterior y evita que se devuelva
        pivotPoint = transform.position;

        targetRotation *= Quaternion.Euler(0, angle, 0);
        currentForward = targetRotation * Vector3.forward;
        currentRight = targetRotation * Vector3.right;

        isRotating = true;
        isInTurnTrigger = false;

        // Al girar, el carril en el que estabas se convierte en tu nuevo "centro"
        // para que la transición sea fluida.
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

        // El impulso lateral: nos cambia de carril inmediatamente
        if (lastWallRight) currentLane = -1;
        else currentLane = 1;

        isWallRunning = false;
    }

    void ApplyPhysics()
    {
        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -1f;
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
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            if (Vector3.Dot(hit.normal, currentForward) < -0.6f) Die();
        }
    }

    void Die() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    private void OnTriggerEnter(Collider other) { if (other.CompareTag("TurnTrigger")) isInTurnTrigger = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("TurnTrigger")) isInTurnTrigger = false; }
}