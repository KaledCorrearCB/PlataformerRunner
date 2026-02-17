using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class RunnerController : MonoBehaviour
{
    [Header("Movimiento y Progresión")]
    public float initialSpeed = 12f;
    public float maxSpeed = 30f;
    public float speedIncreaseRate = 0.05f;
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

    [Header("Habilidad Ambulancia (Escudo)")]
    public MeshRenderer capsuleRenderer;
    public GameObject ambulanceModel;
    public float abilityDuration = 7f;
    public float ambulanceSpeedBoost = 5f;
    private bool isAmbulanceMode = false;
    private float abilityTimer = 0f;

    // Variables internas de control
    private CharacterController controller;
    private int currentLane = 0;
    private Vector3 currentForward = Vector3.forward;
    private Vector3 currentRight = Vector3.right;
    private Vector3 pivotPoint;

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

        originalHeight = controller.height;
        originalCenter = controller.center;
        controller.stepOffset = 0.1f;

        if (ambulanceModel != null) ambulanceModel.SetActive(false);
    }

    void Update()
    {
        // 1. Distancia
        float frameDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                               new Vector3(lastPosition.x, 0, lastPosition.z));
        distanceTraveled += frameDistance;
        lastPosition = transform.position;

        if (distanceText != null)
            distanceText.text = Mathf.FloorToInt(distanceTraveled).ToString() + "m";

        // 2. Velocidad
        if (currentForwardSpeed < maxSpeed)
            currentForwardSpeed += speedIncreaseRate * Time.deltaTime;

        // 3. Lógica
        HandleInputs();
        HandleAbilityTimer();
        MovePlayer();

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

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!isAmbulanceMode)
            {
                if (!controller.isGrounded && !isWallRunning) verticalVelocity = fastFallSpeed;
                StartSlide();
            }
            else
            {
                if (!controller.isGrounded && !isWallRunning) verticalVelocity = fastFallSpeed;
            }
        }

        if (Input.GetKeyUp(KeyCode.S)) StopSlide();
    }

    void MovePlayer()
    {
        Vector3 offsetFromPivot = transform.position - pivotPoint;
        float currentLateralPos = Vector3.Dot(offsetFromPivot, currentRight);
        float targetLateralPos = currentLane * laneDistance;
        float lateralDelta = targetLateralPos - currentLateralPos;
        Vector3 lateralMoveVector = currentRight * (lateralDelta * laneChangeSpeed);

        float actualSpeed = isAmbulanceMode ? currentForwardSpeed + ambulanceSpeedBoost : currentForwardSpeed;
        Vector3 forwardMoveVector = currentForward * actualSpeed;

        CheckWallRun();
        ApplyPhysics();
        Vector3 verticalMoveVector = Vector3.up * verticalVelocity;

        controller.Move((forwardMoveVector + lateralMoveVector + verticalMoveVector) * Time.deltaTime);

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

    #region Lógica de Habilidad
    void HandleAbilityTimer()
    {
        if (isAmbulanceMode)
        {
            abilityTimer -= Time.deltaTime;
            if (abilityTimer <= 0) ToggleAmbulanceMode(false);
        }
    }

    public void ToggleAmbulanceMode(bool activate)
    {
        isAmbulanceMode = activate;
        if (capsuleRenderer != null) capsuleRenderer.enabled = !activate;
        if (ambulanceModel != null) ambulanceModel.SetActive(activate);

        if (activate) abilityTimer = abilityDuration;
        else StopSlide();
    }
    #endregion

    void MoveLane(bool goingRight)
    {
        if (isInTurnTrigger) { TurnCorner(goingRight ? 90 : -90); return; }
        if (!isRotating)
            currentLane = Mathf.Clamp(currentLane + (goingRight ? 1 : -1), -1, 1);
    }

    void TurnCorner(float angle)
    {
        if (isRotating) return;
        pivotPoint = transform.position;
        targetRotation *= Quaternion.Euler(0, angle, 0);
        currentForward = targetRotation * Vector3.forward;
        currentRight = targetRotation * Vector3.right;
        isRotating = true;
        isInTurnTrigger = false;
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
            if (Vector3.Dot(hit.normal, currentForward) < -0.6f)
            {
                if (isAmbulanceMode)
                {
                    ToggleAmbulanceMode(false);
                    Destroy(hit.gameObject);
                }
                else Die();
            }
        }
    }

    void Die() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TurnTrigger")) isInTurnTrigger = true;

        if (other.CompareTag("PowerUp"))
        {
            ToggleAmbulanceMode(true);
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) { if (other.CompareTag("TurnTrigger")) isInTurnTrigger = false; }
}