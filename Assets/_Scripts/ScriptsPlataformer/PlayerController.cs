using UnityEngine;
using UnityEngine.InputSystem;

// Esto soluciona el error CS1061 al diferenciar el componente de tu clase generada
using UnityPlayerInput = UnityEngine.InputSystem.PlayerInput;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    [Header("Sistemas del Jugador")]
    public PlayerWater playerWater;
    public Animator anim;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
    private float characterGravity = -20f;

    [Header("Referencias de Escena")]
    public Transform model;
    public GameObject interactUI;

    // Componentes internos
    private CharacterController CharCon;
    private UnityPlayerInput playerInputComponent; // Usamos el alias para evitar conflictos
    private CameraController cam;

    // Variables de estado y física
    private Vector2 inputM;
    private Vector3 inputVector;
    private Vector3 movementVector;
    private float verticalVelocity;

    [HideInInspector] public bool stopMoving;
    [HideInInspector] public LSEntry currentLevelNode;
    [HideInInspector] public FlowerPot currentFlowerPot;
    [HideInInspector] public WaterSource currentWaterSource;
    [HideInInspector] public CharacterInNeed currentCharacterInNeed;

    public void Awake()
    {
        // Obtenemos el componente usando el alias definido arriba
        playerInputComponent = GetComponent<UnityPlayerInput>();
        CharCon = GetComponent<CharacterController>();
        cam = FindFirstObjectByType<CameraController>();

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        if (playerWater == null)
            playerWater = GetComponent<PlayerWater>();
    }

    void Update()
    {
        if (stopMoving)
        {
            if (interactUI != null) interactUI.SetActive(false);
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        GetInput();
        ApplyMovement();
        RotateModel();
        HandleInteractionUI();

        if (anim != null)
        {
            anim.SetFloat("Speed", inputM.magnitude, 0.1f, Time.deltaTime);
            anim.SetBool("IsGrounded", CharCon.isGrounded);
        }
    }

    public void GetInput()
    {
        // Acceso seguro a las acciones del Asset configurado en el Inspector
        if (playerInputComponent != null && playerInputComponent.actions != null)
        {
            inputM = playerInputComponent.actions["Move"].ReadValue<Vector2>();
        }

        if (cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            inputVector = camRight * inputM.x + camForward * inputM.y;
        }

        if (CharCon.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += characterGravity * Time.deltaTime;

        movementVector = inputVector * moveSpeed;
        movementVector.y = verticalVelocity;
    }

    void RotateModel()
    {
        Vector3 flatMove = new Vector3(inputVector.x, 0, inputVector.z);
        if (flatMove.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatMove);
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void ApplyMovement()
    {
        if (CharCon != null)
        {
            CharCon.Move(movementVector * Time.deltaTime);
        }
    }

    // Vincula estos métodos en los Unity Events del componente Player Input del inspector
    public void OnSelect()
    {
        if (stopMoving) return;

        if (currentLevelNode != null)
        {
            currentLevelNode.LoadLevel();
            return;
        }

        if (currentCharacterInNeed != null)
        {
            currentCharacterInNeed.TryDeliverKit();
            return;
        }
    }

    public void OnJump()
    {
        if (CharCon.isGrounded && !stopMoving)
        {
            verticalVelocity = jumpForce;
            if (anim != null) anim.SetTrigger("Jump");
        }
    }

    private void HandleInteractionUI()
    {
        if (interactUI != null)
        {
            bool showUI = (currentLevelNode != null || currentCharacterInNeed != null ||
                           currentFlowerPot != null || currentWaterSource != null);
            interactUI.SetActive(showUI);
        }
    }
}