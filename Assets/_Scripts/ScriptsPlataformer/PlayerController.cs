using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour
{
    //hacerlo singletone
    public static PlayerController instance;

    [Header("Sistemas del Jugador")]
    public PlayerWater playerWater;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
    private float characterGravity = -20f;

    [Header("Referencias de Escena")]
    public Transform model;
    public GameObject interactUI; // Arrastra aquí tu texto de "Presiona E"

    // Componentes internos
    private CharacterController CharCon;
    private PlayerInput playerInput;
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
    public void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        CharCon = GetComponent<CharacterController>();
        cam = FindFirstObjectByType<CameraController>();
        
        if(instance != null)
        {
            return;
        }

        else
        {
            instance = this;
        }
        if (playerWater == null)
            playerWater = GetComponent<PlayerWater>();

    }

    void Update()
    {
        // Si el nivel está cargando, bloqueamos todo el control
        if (stopMoving)
        {
            if (interactUI != null) interactUI.SetActive(false);
            return;
        }

        GetInput();
        OnMove();
        RotateModel();
        HandleInteractionUI();
    }

    public void GetInput()
    {
        inputM = playerInput.actions["Move"].ReadValue<Vector2>();

        // Direcciones de la cámara
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Cálculo de movimiento
        inputVector = camRight * inputM.x + camForward * inputM.y;

        // Gravedad constante
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

    public void OnMove()
    {
        CharCon.Move(movementVector * Time.deltaTime);
    }

    // Se ejecuta automáticamente por el InputSystem al presionar el botón de "Select" (E)
    public void OnSelect()
    {
        Debug.Log("Select pressed");
        if (currentLevelNode != null && !stopMoving)
        {
            Debug.Log("Cargando nivel: " + currentLevelNode.levelName);
            currentLevelNode.LoadLevel();
            return;
        }

        
    }

    public void OnSelectHold(InputAction.CallbackContext context)
    {
        if (context.started) // Cuando empieza a presionar E
        {
            // Prioridad: 1. Regar maceta, 2. Absorber agua
            if (currentFlowerPot != null)
            {
                currentFlowerPot.StartWatering();
            }
            else if (currentWaterSource != null)
            {
                // Activar efectos visuales de la fuente (si los tiene)
                currentWaterSource.StartAbsorbing(this);

                // ¡ACTIVAR LA ABSORCIÓN DE AGUA DEL JUGADOR!
                if (playerWater != null)
                    playerWater.StartAbsorbingWater(currentWaterSource.gameObject);
            }
        }

        if (context.canceled) // Cuando suelta E
        {
            if (currentFlowerPot != null)
            {
                currentFlowerPot.StopWatering();
            }
            else if (currentWaterSource != null)
            {
                currentWaterSource.StopAbsorbing(this);

                // ¡DETENER LA ABSORCIÓN DE AGUA!
                if (playerWater != null)
                    playerWater.StopAbsorbingWater();
            }
        }
    }

    public void OnJump()
    {
        if (CharCon.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    // Controla si el cartel de "Presiona E" se ve o no
    private void HandleInteractionUI()
    {
        if (interactUI != null)
        {
            // Prioridad visual: portal > maceta > fuente de agua
            if (currentLevelNode != null)
            {
                interactUI.SetActive(true);
                // Si tienes un texto, cámbialo a "Entrar al portal"
            }
            else if (currentFlowerPot != null)
            {
                interactUI.SetActive(true);
                // Cambiar texto a "Regar planta (Mantener E)"
            }
            else if (currentWaterSource != null)
            {
                interactUI.SetActive(true);
                // Cambiar texto a "Absorber agua (Mantener E)"
            }
            else
            {
                interactUI.SetActive(false);
            }
        }
    }

}