using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

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

    [Header("Configuración Tobogán")]
    public float slideSpeedMultiplier = 1.5f;
    public GameObject waterSplashVFX;
    private bool isSlidingOnWater = false;

    private CharacterController controller;

    private int currentLane = 0;

    private Vector3 currentForward = Vector3.forward;
    private Vector3 currentRight = Vector3.right;

    private Vector3 pivotPoint;

    private Quaternion targetRotation;

    private bool isRotating = false;

    private bool isInTurnTrigger = false;

    private bool canTurn = false;

    private bool isInTurnZone = false;

    private float turnInputTimer = 0f;
    private float turnInputBufferTime = 0.3f; // tiempo para recordar input
    private int bufferedTurn = 0;

    private Transform turnCenterPoint;

    private bool isSnappingToCenter = false;



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
        if (waterSplashVFX != null) waterSplashVFX.SetActive(false);
    }

    void Update()
    {
        float frameDistance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(lastPosition.x, 0, lastPosition.z)
        );

        distanceTraveled += frameDistance;
        lastPosition = transform.position;

        if (distanceText != null)
            distanceText.text = Mathf.FloorToInt(distanceTraveled) + "m";

        if (currentForwardSpeed < maxSpeed)
            currentForwardSpeed += speedIncreaseRate * Time.deltaTime;

        if (SessionManager.Instance != null)
            SessionManager.Instance.SetDistance(distanceTraveled);

        HandleAbilityTimer();
        MovePlayer();

        if (transform.position.y < -5f)
            Die();

    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (input.x > 0.5f)
            {
                if (isInTurnZone && !isRotating)
                {
                    Debug.Log("GIRO DERECHA");
                    TurnCorner(90f);
                }
                else
                {
                    MoveLane(true);
                }
            }
            else if (input.x < -0.5f)
            {
                if (isInTurnZone && !isRotating)
                {
                    Debug.Log("GIRO IZQUIERDA");
                    TurnCorner(-90f);
                }
                else
                {
                    MoveLane(false);
                }
            }

            if (input.y < -0.5f)
            {
                if (!controller.isGrounded && !isWallRunning)
                    verticalVelocity = fastFallSpeed;

                StartSlide();
            }
        }

        if (context.canceled)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (input.y > -0.5f)
                StopSlide();
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (controller.isGrounded)
                verticalVelocity = jumpForce;
            else if (isWallRunning)
                ExecuteWallJump();
        }
    }

    public void OnActionInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!controller.isGrounded && !isWallRunning)
                verticalVelocity = fastFallSpeed;

            StartSlide();
        }
        else if (context.canceled)
        {
            StopSlide();
        }
    }

    void MovePlayer()
    {
        Vector3 offsetFromPivot = transform.position - pivotPoint;

        float currentLateralPos = Vector3.Dot(offsetFromPivot, currentRight);

        float targetLateralPos = currentLane * laneDistance;

        float lateralDelta = targetLateralPos - currentLateralPos;

        Vector3 lateralMoveVector = Vector3.zero;

        if (!isSnappingToCenter)
        {
            lateralMoveVector = currentRight * (lateralDelta * laneChangeSpeed);
        }

        float actualSpeed = currentForwardSpeed;

        if (isAmbulanceMode)
            actualSpeed += ambulanceSpeedBoost;

        if (isSlidingOnWater)
            actualSpeed *= slideSpeedMultiplier;

        Vector3 forwardMoveVector = currentForward * actualSpeed;

        CheckWallRun();
        ApplyPhysics();

        Vector3 verticalMoveVector = Vector3.up * verticalVelocity;

        controller.Move((forwardMoveVector + lateralMoveVector + verticalMoveVector) * Time.deltaTime);

        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                700f * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;

                currentForward = transform.forward;
                currentRight = transform.right;

                isRotating = false;
            }
        }

        if (isSnappingToCenter && turnCenterPoint != null)
        {
            Vector3 targetPos = new Vector3(
                turnCenterPoint.position.x,
                transform.position.y,
                turnCenterPoint.position.z
            );

            Vector3 moveToCenter = targetPos - transform.position;

            controller.Move(moveToCenter);

            if (moveToCenter.magnitude < 0.05f)
            {
                transform.position = targetPos;

                pivotPoint = transform.position;
                currentLane = 0;

                currentForward = transform.forward;
                currentRight = transform.right;

                isSnappingToCenter = false;
            }
        }
    }

    void MoveLane(bool goingRight)
    {

       
            if (!isRotating)
            {
                currentLane = Mathf.Clamp(currentLane + (goingRight ? 1 : -1), -1, 1);
            }
        
    }

    void TurnCorner(float angle)
    {
        if (isRotating) return;

        currentLane = 0;

        if (turnCenterPoint != null)
        {
            isSnappingToCenter = true;
        }

        targetRotation *= Quaternion.Euler(0, angle, 0);


        isRotating = true;
        isInTurnZone = false;
    }

    void HandleAbilityTimer()
    {
        if (isAmbulanceMode)
        {
            abilityTimer -= Time.deltaTime;

            if (abilityTimer <= 0)
                ToggleAmbulanceMode(false);
        }
    }

    public void ToggleAmbulanceMode(bool activate)
    {
        isAmbulanceMode = activate;

        if (capsuleRenderer != null)
            capsuleRenderer.enabled = !activate;

        if (ambulanceModel != null)
            ambulanceModel.SetActive(activate);

        if (activate)
            abilityTimer = abilityDuration;
        else
            StopSlide();
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

                if (verticalVelocity < 0)
                    verticalVelocity = 0;

                return;
            }
        }

        isWallRunning = false;
    }

    void ExecuteWallJump()
    {
        verticalVelocity = wallJumpUpForce;

        if (lastWallRight)
            currentLane = -1;
        else
            currentLane = 1;

        isWallRunning = false;
    }

    void ApplyPhysics()
    {
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -1f;
        else
        {
            float currGravity = isWallRunning ? 0 : gravity;
            verticalVelocity += currGravity * Time.deltaTime;
        }
    }

    void StartSlide()
    {
        controller.height = slideHeight;
        controller.center = new Vector3(0, slideHeight / 2f, 0);
    }

    void StopSlide()
    {
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

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
                else
                    Die();
            }
        }
    }

    void Die()
    {
        if (SessionManager.Instance != null)
            SessionManager.Instance.FinalizeRun();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TurnTrigger"))
        {
            isInTurnZone = true;

            Transform center = other.transform.Find("CenterPoint");
            if (center != null)
            {
                turnCenterPoint = center;
            }

            Debug.Log("ENTRÓ AL TRIGGER");
        }

        if (other.CompareTag("PowerUp"))
        {
            float dist = Vector3.Distance(transform.position, other.transform.position);

            if (dist < 3f)
            {
                ToggleAmbulanceMode(true);
                Destroy(other.gameObject);
            }
        }

        if (other.CompareTag("WaterSlide"))
        {
            isSlidingOnWater = true;

            if (waterSplashVFX != null)
                waterSplashVFX.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TurnTrigger"))
        {
            isInTurnZone = false;
        }

        if (other.CompareTag("WaterSlide"))
        {
            isSlidingOnWater = false;

            if (waterSplashVFX != null)
                waterSplashVFX.SetActive(false);
        }
    }

}