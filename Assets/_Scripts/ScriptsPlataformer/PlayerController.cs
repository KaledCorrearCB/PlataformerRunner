using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Singleton
    public static PlayerController instance;

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
    private float characterGravity = -20f;

    [Header("Referencias de Escena")]
    public Transform model;
    public GameObject interactUI; // UI de interacción (opcional por ahora)

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

    public void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        CharCon = GetComponent<CharacterController>();
        cam = FindFirstObjectByType<CameraController>();

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
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
        // Leemos el input del Input System
        inputM = playerInput.actions["Move"].ReadValue<Vector2>();

        // Direcciones relativas a la cámara
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
        else
        {
            // Fallback si no hay cámara
            inputVector = new Vector3(inputM.x, 0, inputM.y);
        }

        // Gravedad
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
        // Esta función queda vacía por ahora hasta que creemos el nuevo sistema de niveles
        Debug.Log("Botón de selección presionado");
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
        // Desactivado temporalmente ya que no hay nodos de nivel
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }
    }
}