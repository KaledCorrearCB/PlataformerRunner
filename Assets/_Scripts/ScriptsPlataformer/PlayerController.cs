using Unity.VisualScripting;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;

    //variables para referencias las propiedades del objeto

    // el character controller
    CharacterController CharCon;
    // el input del InputSystem
    PlayerInput playerInput;

    private CameraController cam;
    //variables para las fisicas

    // Variable para detectar el valor dentro del vector 2

    private Vector2 inputM;
    private Vector3 inputVector;
    private Vector3 movementVector;
   
    private Vector3 moveAmount;


    // Variable de gravedad para equilibrar la velocidad en el eje y
    public float jumpForce = 7f;

    private float characterGravity = -20f;
    float verticalVelocity;
    //Sacamos las referencias y obtenemos los componentes

    //Rotacion de personaje

    public Transform model;
    public float rotationSpeed = 10f;

    // Controladores para animaciones

    [HideInInspector] public bool stopMoving;

    //LevelSelector

    public LSEntry currentLevelNode;


    public void Awake()
    {
      playerInput = GetComponent<PlayerInput>();
      CharCon = GetComponent<CharacterController>();
        cam = FindFirstObjectByType<CameraController>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public void GetInput()
    {
        inputM = playerInput.actions["Move"].ReadValue<Vector2>();


        // Direcciones de la cámara (proyectadas en el plano XZ)
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Movimiento relativo a la cámara
        inputVector = camRight * inputM.x + camForward * inputM.y;

        movementVector = (inputVector * moveSpeed) + Vector3.up * characterGravity;

        // gravedad
        if (CharCon.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += characterGravity * Time.deltaTime;

        movementVector = inputVector * moveSpeed;
        movementVector.y = verticalVelocity;

    }

    //Funciones para rotacion y control de modelo

    void RotateModel()
    {
        Vector3 flatMove = new Vector3(inputVector.x, 0, inputVector.z);

        if (flatMove.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatMove);
            model.rotation = Quaternion.Slerp(
                model.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Funcion que va a ser para mover al personaje 
    public void OnMove()
    {
        CharCon.Move(movementVector * Time.deltaTime);
    }

    public void Jump()
    {
        if (CharCon.isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }
    public void OnJump()
    {
        Jump();

    }

    public void OnSelect()
    {
        if (currentLevelNode != null)
        {
            currentLevelNode.LoadLevel();
        }
        Debug.Log("SELECT WORKING");
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        OnMove();
        RotateModel();
    }


}
