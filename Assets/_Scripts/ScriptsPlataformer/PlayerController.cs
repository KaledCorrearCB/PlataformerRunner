// PlayerController.cs  ← REEMPLAZA tu versión actual
// Cambios respecto al original (marcados con // *** NUEVO ***):
//   1. Se agrega la variable pública currentCharacterInNeed
//   2. OnSelect() ahora también llama a TryDeliverKit() si hay un personaje cerca
//   3. HandleInteractionUI() muestra el interactUI cuando hay un personaje cerca

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour
{
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
    public GameObject interactUI;

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
    [HideInInspector] public CharacterInNeed currentCharacterInNeed; // *** NUEVO ***

    public void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        CharCon = GetComponent<CharacterController>();
        cam = FindFirstObjectByType<CameraController>();

        if (instance != null)
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

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        inputVector = camRight * inputM.x + camForward * inputM.y;

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

    public void OnSelect()
    {
        Debug.Log("Select pressed");

        // Prioridad 1: cargar nivel (igual que antes)
        if (currentLevelNode != null && !stopMoving)
        {
            Debug.Log("Cargando nivel: " + currentLevelNode.levelName);
            currentLevelNode.LoadLevel();
            return;
        }

        // *** NUEVO — Prioridad 2: entregar kit a personaje ***
        if (currentCharacterInNeed != null)
        {
            currentCharacterInNeed.TryDeliverKit();
            return;
        }
    }

    public void OnSelectHold(InputAction.CallbackContext context)
    {
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

    public void OnJump()
    {
        if (CharCon.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    private void HandleInteractionUI()
    {
        if (interactUI != null)
        {
            // *** NUEVO — currentCharacterInNeed agregado a la prioridad visual ***
            if (currentLevelNode != null)
            {
                interactUI.SetActive(true);
            }
            else if (currentCharacterInNeed != null)  // *** NUEVO ***
            {
                interactUI.SetActive(true);
            }
            else if (currentFlowerPot != null)
            {
                interactUI.SetActive(true);
            }
            else if (currentWaterSource != null)
            {
                interactUI.SetActive(true);
            }
            else
            {
                interactUI.SetActive(false);
            }
        }
    }
}