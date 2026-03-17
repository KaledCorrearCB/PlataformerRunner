using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class LevelSelectorPlayer : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    public float gravity = -20f;

    public static bool puedeMoversis = true;

    [Header("Skins e Inventario")]
    public GameObject normalSkin;
    public GameObject motoSkin;
    private bool hasMoto = false;
    private bool isUsingMoto = false;

    [Header("Animacion")]
    public Animator animator;
    public string parametroVelocidad = "Speed";

    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 inputMovimiento;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        puedeMoversis = true;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (normalSkin) normalSkin.SetActive(true);
        if (motoSkin) motoSkin.SetActive(false);
    }

    public void OnMove(InputValue value)
    {
        inputMovimiento = value.Get<Vector2>();
    }

    // Acción para el mando (Configurada en el Input Action Asset)
    public void OnAction(InputValue value)
    {
        if (value.isPressed && hasMoto && puedeMoversis)
        {
            ToggleMoto();
        }
    }

    private void Update()
    {
        if (puedeMoversis)
        {
            // DETECCIÓN ADICIONAL PARA TECLADO (Tecla 1)
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame && hasMoto)
            {
                ToggleMoto();
            }

            MoverPersonaje();
        }
        else
        {
            AplicarGravedadSolo();
            ActualizarAnimacion(0f);
        }
    }

    private void MoverPersonaje()
    {
        Vector3 moveDirection = new Vector3(inputMovimiento.x, 0f, inputMovimiento.y).normalized;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            ActualizarAnimacion(moveDirection.magnitude);
        }
        else
        {
            ActualizarAnimacion(0f);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void ActualizarAnimacion(float valor)
    {
        if (animator != null) animator.SetFloat(parametroVelocidad, valor);
    }

    private void AplicarGravedadSolo()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ToggleMoto()
    {
        isUsingMoto = !isUsingMoto;
        if (normalSkin) normalSkin.SetActive(!isUsingMoto);
        if (motoSkin) motoSkin.SetActive(isUsingMoto);
        moveSpeed = isUsingMoto ? 15f : 8f;

        // Refrescamos el animator para que tome el de la skin activa
        animator = GetComponentInChildren<Animator>();
        Debug.Log(isUsingMoto ? "Moto equipada" : "Caminando");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Moto"))
        {
            hasMoto = true;
            Debug.Log("Moto obtenida. Tecla 1 o Cuadrado para usarla.");
            Destroy(other.gameObject);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("ObstaculoWobble"))
        {
            ProceduralWobble wobbler = hit.gameObject.GetComponent<ProceduralWobble>();
            if (wobbler != null)
            {
                wobbler.TriggerWobble();
            }
        }
    }
}