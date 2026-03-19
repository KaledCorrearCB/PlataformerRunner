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
        isUsingMoto = false;

        if (normalSkin) normalSkin.SetActive(true);
        if (motoSkin) motoSkin.SetActive(false);

        Invoke("ActualizarReferenciaAnimator", 0.15f);
    }

    public void OnMove(InputValue value) => inputMovimiento = value.Get<Vector2>();

    private void Update()
    {
        if (puedeMoversis)
        {
            // --- DETECCIÓN DE MOTO ULTRA-FIABLE ---
            bool presionoActivarMoto = false;

            // 1. Revisar Teclado (Tecla 1)
            if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
                presionoActivarMoto = true;

            // 2. Revisar Mando (Cuadrado / West)
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                Debug.Log("Botón Cuadrado detectado en mando");
                presionoActivarMoto = true;
            }

            if (presionoActivarMoto)
            {
                if (hasMoto)
                {
                    ToggleMoto();
                }
                else
                {
                    Debug.Log("Presionaste el botón pero NO tienes la moto recogida aún.");
                }
            }

            MoverPersonaje();
        }
        else
        {
            AplicarGravedadSolo();
        }
    }

    private void MoverPersonaje()
    {
        Vector3 moveDirection = new Vector3(inputMovimiento.x, 0f, inputMovimiento.y).normalized;
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;

        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            ActualizarAnimacion(moveDirection.magnitude);
        }
        else ActualizarAnimacion(0f);

        controller.Move(velocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("ObstaculoWobble"))
        {
            var wobbler = hit.gameObject.GetComponent<ProceduralWobble>();
            if (wobbler != null) wobbler.TriggerWobble();
        }
    }

    public void ActualizarAnimacion(float valor)
    {
        if (animator == null || !animator.gameObject.activeInHierarchy) ActualizarReferenciaAnimator();
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
        ActualizarReferenciaAnimator();
        Debug.Log("Moto " + (isUsingMoto ? "Activada" : "Desactivada"));
    }

    private void ActualizarReferenciaAnimator()
    {
        if (isUsingMoto && motoSkin != null) animator = motoSkin.GetComponentInChildren<Animator>();
        else if (normalSkin != null) animator = normalSkin.GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Asegúrate de que el item de la moto tenga el Tag "Moto"
        if (other.CompareTag("Moto"))
        {
            hasMoto = true;
            Debug.Log("¡Moto recogida! Ahora puedes usar Cuadrado.");
            Destroy(other.gameObject);
        }
    }
}