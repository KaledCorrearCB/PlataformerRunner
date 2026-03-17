using UnityEngine;
using UnityEngine.InputSystem;

// Alias para evitar conflictos con la clase generada de Unity
using UnityPlayerInput = UnityEngine.InputSystem.PlayerInput;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    // ─────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────

    [Header("Sistemas del Jugador")]
    public PlayerWater playerWater;
    public Animator anim;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
    private float characterGravity = -20f;

    [Header("Referencias de Escena")]
    public Transform model;
    public GameObject interactUI;

    // ─────────────────────────────────────────────
    //  Estado público (usado por otros scripts)
    // ─────────────────────────────────────────────

    [HideInInspector] public bool stopMoving;
    [HideInInspector] public LSEntry currentLevelNode;
    [HideInInspector] public FlowerPot currentFlowerPot;
    [HideInInspector] public WaterSource currentWaterSource;
    [HideInInspector] public CharacterInNeed currentCharacterInNeed;

    // ─────────────────────────────────────────────
    //  Componentes y variables internas
    // ─────────────────────────────────────────────

    private CharacterController charCon;
    private UnityPlayerInput playerInputComponent;
    private CameraController cam;

    private Vector2 inputM;
    private Vector3 inputVector;
    private Vector3 movementVector;
    private float verticalVelocity;
    private bool isSprinting;

    // ─────────────────────────────────────────────
    //  Awake / Update
    // ─────────────────────────────────────────────

    void Awake()
    {
        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        charCon = GetComponent<CharacterController>();
        playerInputComponent = GetComponent<UnityPlayerInput>();
        cam = FindFirstObjectByType<CameraController>();

        if (playerWater == null)
            playerWater = GetComponent<PlayerWater>();
    }

    void Update()
    {
        if (stopMoving)
        {
            if (interactUI != null) interactUI.SetActive(false);
            if (anim != null) anim.SetFloat("Speed", 0f);
            ApplyGravity();     // la gravedad sigue actuando aunque esté parado
            return;
        }

        HandleInputs();
        ApplyMovement();
        RotateModel();
        HandleInteractionUI();
        UpdateAnimations();
    }

    // ─────────────────────────────────────────────
    //  Movimiento
    // ─────────────────────────────────────────────

    private void HandleInputs()
    {
        inputM = playerInputComponent.actions["Move"].ReadValue<Vector2>();
        isSprinting = playerInputComponent.actions["Sprint"].ReadValue<float>() > 0.1f;

        // Dirección relativa a la cámara (igual que el código viejo)
        if (cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            inputVector = camRight * inputM.x + camForward * inputM.y;
        }
        else
        {
            // Fallback sin cámara
            inputVector = new Vector3(inputM.x, 0f, inputM.y);
        }

        ApplyGravity();

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        movementVector = inputVector * speed;
        movementVector.y = verticalVelocity;
    }

    private void ApplyGravity()
    {
        if (charCon.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += characterGravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        charCon.Move(movementVector * Time.deltaTime);
    }

    private void RotateModel()
    {
        Vector3 flatMove = new Vector3(inputVector.x, 0f, inputVector.z);
        if (flatMove.sqrMagnitude > 0.001f && model != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatMove);
            model.rotation = Quaternion.Slerp(model.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ─────────────────────────────────────────────
    //  Animaciones
    // ─────────────────────────────────────────────

    private void UpdateAnimations()
    {
        if (anim == null) return;

        // Damping de 0.1 s para transiciones suaves (igual que el código viejo)
        anim.SetFloat("Speed", inputM.magnitude, 0.1f, Time.deltaTime);
        anim.SetBool("IsGrounded", charCon.isGrounded);
    }

    // ─────────────────────────────────────────────
    //  UI de Interacción
    // ─────────────────────────────────────────────

    private void HandleInteractionUI()
    {
        if (interactUI == null) return;

        bool show = currentLevelNode != null
                 || currentCharacterInNeed != null
                 || currentFlowerPot != null
                 || currentWaterSource != null;

        interactUI.SetActive(show);
    }

    // ─────────────────────────────────────────────
    //  Callbacks del Input System
    //  (todos reciben InputAction.CallbackContext
    //   porque el Input Actions Asset usa
    //   "Send Messages" o "Invoke Unity Events")
    // ─────────────────────────────────────────────

    /// <summary>
    /// Salto — Button South (Gamepad) / Space (Teclado)
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!charCon.isGrounded || stopMoving) return;

        verticalVelocity = jumpForce;
        if (anim != null) anim.SetTrigger("Jump");
    }

    /// <summary>
    /// Interacción rápida — Button North (Gamepad) / E (Teclado)
    /// Prioridad: Nivel → Personaje en necesidad
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed || stopMoving) return;

        // Prioridad 1: cargar nivel
        if (currentLevelNode != null)
        {
            Debug.Log("Cargando nivel: " + currentLevelNode.levelName);
            currentLevelNode.LoadLevel();
            return;
        }

        // Prioridad 2: entregar kit a personaje en necesidad
        if (currentCharacterInNeed != null)
        {
            currentCharacterInNeed.TryDeliverKit();
        }
    }

    /// <summary>
    /// Interacción mantenida — Button North (Gamepad) / E (Teclado) con Hold
    /// Maneja regar plantas y absorber agua.
    /// context.started  → empieza a mantener
    /// context.canceled → suelta el botón
    /// </summary>
    public void OnHoldInteract(InputAction.CallbackContext context)
    {
        if (stopMoving) return;

        if (context.started)
        {
            Debug.Log($"Hold started — FlowerPot: {currentFlowerPot}, WaterSource: {currentWaterSource}");

            if (currentFlowerPot != null)
            {
                currentFlowerPot.StartWatering();
            }
            else if (currentWaterSource != null)
            {
                currentWaterSource.StartAbsorbing(this);
                if (playerWater != null)
                    playerWater.StartAbsorbingWater(currentWaterSource.gameObject);
            }
        }

        if (context.canceled)
        {
            if (currentFlowerPot != null)
            {
                currentFlowerPot.StopWatering();
            }
            else if (currentWaterSource != null)
            {
                currentWaterSource.StopAbsorbing(this);
                if (playerWater != null)
                    playerWater.StopAbsorbingWater();
            }
        }
    }

    /// <summary>
    /// Detección / Ladrido — Button West (Gamepad) / V (Teclado)
    /// Delega en CharacterDetector para no duplicar lógica.
    /// </summary>
    public void OnAction(InputAction.CallbackContext context)
    {
        if (!context.performed || stopMoving) return;

        // CharacterDetector vive en el mismo GameObject; lo buscamos una vez.
        CharacterDetector detector = GetComponent<CharacterDetector>();
        if (detector != null)
        {
            detector.TryDetect();
        }
        else
        {
            Debug.LogWarning("[PlayerController] No se encontró CharacterDetector en el jugador.");
        }
    }
}