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
    [HideInInspector] public UnlockableMechanic currentUnlockable;

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
    public float VerticalVelocity => verticalVelocity;

    private GrapplingRope currentRope;
    private bool isOnGrapple;
    private bool justLaunched;       // flag para proteger el impulso
    private float launchTimer;        // cuánto tiempo proteger

    // — Coyote Time —
    [Header("Configuración de Salto")]
    public float coyoteTime = 0.15f;       // segundos de gracia tras caer de un borde
    private float coyoteTimeCounter;        // contador regresivo
    private bool wasGrounded;               // estado del frame anterior

    // ─────────────────────────────────────────────
    //  Awake / Update
    // ─────────────────────────────────────────────

    void Awake()
    {
        this.enabled = true;
        stopMoving = false;

        instance = this;

        charCon = GetComponent<CharacterController>();
        playerInputComponent = GetComponent<UnityPlayerInput>();
        cam = FindFirstObjectByType<CameraController>();

        if (playerWater == null)
            playerWater = GetComponent<PlayerWater>();

        playerInputComponent.actions.FindActionMap("Player").Enable();

        // 🔍 LOGS DE DIAGNÓSTICO
        Debug.Log($"[PC Awake] enabled={this.enabled} | stopMoving={stopMoving}");
        Debug.Log($"[PC Awake] CharCon={charCon} | PlayerInput={playerInputComponent} | Cam={cam}");

    }

    void Update()
    {
      //  Debug.Log($"[PC Update] stopMoving={stopMoving} | enabled={this.enabled} | inputM={inputM}");

        if (stopMoving)
        {
            if (interactUI != null) interactUI.SetActive(false);
            if (anim != null) anim.SetFloat("Speed", 0f);
            ApplyGravity();     // la gravedad sigue actuando aunque esté parado
            return;
        }


        // Durante el swing: solo aplicar gravedad y movimiento vertical
        // GrapplingRope maneja la posición horizontal

        // Contador para proteger el impulso de liana
        if (justLaunched)
        {
            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0f) justLaunched = false;
        }

        if (isOnGrapple)
        {
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

        // ── Coyote Time ──────────────────────────────────────────
        // Si acaba de tocar el suelo → recarga el contador
        if (charCon.isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;   // se agota en el aire

        wasGrounded = charCon.isGrounded;
        // ─────────────────────────────────────────────────────────

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
        if (anim == null)
        {
            Debug.LogError("[ANIM] anim es NULL — referencia rota en build!");
            return;
        }

        float speed = inputM.magnitude;
        Debug.Log($"[ANIM] Speed={speed:F3} | isGrounded={charCon.isGrounded}");

        anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        anim.SetBool("IsGrounded", charCon.isGrounded);
    }

    // ─────────────────────────────────────────────
    //  UI de Interacción
    // ─────────────────────────────────────────────

    private void HandleInteractionUI()
    {

        // Si estamos en liana, GrapplingRope controla el movimiento
        if (isOnGrapple) return;

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

        // Soltar liana si está en swing
        if (isOnGrapple && currentRope != null)
        {
            currentRope.ReleaseSwing();
            return;
        }

        if (stopMoving) return;

        // ── Coyote Time ──────────────────────────────────────────
        // Permite saltar si el contador aún tiene tiempo (suelo real o gracia)
        if (coyoteTimeCounter > 0f)
        {
            verticalVelocity = jumpForce;
            coyoteTimeCounter = 0f;     // consumir el coyote time inmediatamente
            if (anim != null) anim.SetTrigger("Jump");
        }
        // ─────────────────────────────────────────────────────────
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
            currentLevelNode.LoadLevel();
            return;
        }

        // Prioridad 2: entregar kit
        if (currentCharacterInNeed != null)
        {
            currentCharacterInNeed.TryDeliverKit();
            return;
        }

        // ✅ Prioridad 3: desbloquear mecánica
        if (currentUnlockable != null)
        {
            currentUnlockable.TryUnlock();
            return;
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

    // ─────────────────────────────────────────────
    //  Integración con GrapplingRope
    // ─────────────────────────────────────────────

    public void OnGrappleStart(GrapplingRope rope)
    {
        currentRope = rope;
        isOnGrapple = true;
        justLaunched = false;

        // ✅ Limpiar velocidad residual del movimiento que traía
        movementVector = Vector3.zero;
        inputVector = Vector3.zero;
        verticalVelocity = 0f;

        // ✅ Deshabilitar el CC para que no interfiera con el movimiento directo
        charCon.enabled = false;

        if (anim != null) anim.SetBool("IsSwinging", true);
    }

    public void OnGrappleStop(Vector3 launchVelocity)
    {
        currentRope = null;
        isOnGrapple = false;

        // ✅ Rehabilitar el CC antes de devolver el control
        charCon.enabled = true;

        verticalVelocity = launchVelocity.y;
        justLaunched = true;
        launchTimer = 0.15f;

        if (anim != null)
        {
            anim.SetBool("IsSwinging", false);
            anim.SetTrigger("Jump");
        }
    }


    // ─────────────────────────────────────────────
    //  Integración con Trampoline
    // ─────────────────────────────────────────────

    public void Bounce(float force)
    {
        // Cancelar cualquier swing activo
        if (isOnGrapple && currentRope != null)
            currentRope.ReleaseSwing();

        verticalVelocity = force;
        justLaunched = true;     // reutiliza el flag del grapple para proteger el impulso
        launchTimer = 0.2f;

        if (anim != null)
        {
            anim.ResetTrigger("Jump");
            anim.SetTrigger("Jump");
        }
    }

}