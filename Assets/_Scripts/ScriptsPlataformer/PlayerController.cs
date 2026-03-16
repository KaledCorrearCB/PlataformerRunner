using UnityEngine;
using UnityEngine.InputSystem;

// Alias para evitar conflictos con la clase generada de Unity
using UnityPlayerInput = UnityEngine.InputSystem.PlayerInput;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("Sistemas del Jugador")]
    public PlayerWater playerWater; // Restaurado
    public Animator anim;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
    private float characterGravity = -20f;

    [Header("Referencias de Modelo")]
    public Transform model;

    // --- VARIABLES DE ESTADO (Requeridas por tus otros scripts) ---
    [HideInInspector] public bool stopMoving;
    [HideInInspector] public LSEntry currentLevelNode;
    [HideInInspector] public FlowerPot currentFlowerPot;
    [HideInInspector] public WaterSource currentWaterSource;
    [HideInInspector] public CharacterInNeed currentCharacterInNeed;

    // Componentes internos
    private CharacterController CharCon;
    private UnityPlayerInput playerInputComponent;
    private Vector2 inputM;
    private Vector3 movementVector;
    private float verticalVelocity;
    private bool isSprinting;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        CharCon = GetComponent<CharacterController>();
        playerInputComponent = GetComponent<UnityPlayerInput>();

        if (playerWater == null) playerWater = GetComponent<PlayerWater>();
    }

    void Update()
    {
        if (stopMoving)
        {
            if (anim != null) anim.SetFloat("Speed", 0f);
            ApplyGravity();
            return;
        }

        HandleInputs();
        ApplyMovement();
        RotateModel();
        UpdateAnimations();
    }

    private void HandleInputs()
    {
        // Lectura de los nuevos botones que configuramos
        inputM = playerInputComponent.actions["Move"].ReadValue<Vector2>();
        isSprinting = playerInputComponent.actions["Sprint"].ReadValue<float>() > 0.1f;

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (CharCon.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += characterGravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // Movimiento relativo al mundo (puedes ajustarlo a la cámara luego)
        Vector3 moveDir = new Vector3(inputM.x, 0, inputM.y);
        movementVector = moveDir * speed;
        movementVector.y = verticalVelocity;

        if (CharCon != null) CharCon.Move(movementVector * Time.deltaTime);
    }

    private void RotateModel()
    {
        Vector3 direction = new Vector3(movementVector.x, 0, movementVector.z);
        if (direction.sqrMagnitude > 0.01f && model != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            model.rotation = Quaternion.Slerp(model.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetFloat("Speed", inputM.magnitude);
            anim.SetBool("IsGrounded", CharCon.isGrounded);
        }
    }

    // --- VINCULACIÓN DE EVENTOS PARA EL MANDO ---

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && CharCon.isGrounded && !stopMoving)
        {
            verticalVelocity = jumpForce;
            if (anim != null) anim.SetTrigger("Jump");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // Esta es tu antigua tecla "E" o "Triángulo"
        if (context.performed && !stopMoving)
        {
            if (currentLevelNode != null) currentLevelNode.LoadLevel();
            if (currentCharacterInNeed != null) currentCharacterInNeed.TryDeliverKit();
        }
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        // Esta es tu antigua tecla "V" o "Cuadrado"
        if (context.performed && !stopMoving)
        {
            // Aquí puedes llamar a funciones de recolección de PlayerWater si las tenías
            Debug.Log("Acción ejecutada: Recoger/Usar");
        }
    }
}