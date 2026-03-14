using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class LevelSelectorPlayer : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    public float gravity = -20f;

    // VARIABLE PARA BLOQUEO EXTERNO (Usada por CampamentoConstructor)
    public static bool puedeMoversis = true;

    [Header("Skins e Inventario")]
    public GameObject normalSkin;
    public GameObject motoSkin;
    private bool hasMoto = false;
    private bool isUsingMoto = false;

    [Header("Animacion")]
    [Tooltip("Arrastra aqui el componente Animator del modelo del personaje")]
    public Animator animator;
    [Tooltip("Nombre exacto del parametro (Float) en el Animator (ej: Speed)")]
    public string parametroVelocidad = "Speed";

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        puedeMoversis = true;

        // Si no asignaste el Animator manualmente, intentamos buscarlo en los hijos
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Estado inicial de las skins
        if (normalSkin) normalSkin.SetActive(true);
        if (motoSkin) motoSkin.SetActive(false);
    }

    private void Update()
    {
        if (puedeMoversis)
        {
            MoverPersonaje();

            if (Input.GetKeyDown(KeyCode.Alpha1) && hasMoto)
            {
                ToggleMoto();
            }
        }
        else
        {
            AplicarGravedadSolo();
            // Si el movimiento esta bloqueado, forzamos la animacion a 0 (Idle)
            ActualizarAnimacion(0f);
        }
    }

    private void MoverPersonaje()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

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

            // Enviamos la magnitud del movimiento al Animator
            ActualizarAnimacion(moveDirection.magnitude);
        }
        else
        {
            // No hay input, enviamos 0 para volver a Idle
            ActualizarAnimacion(0f);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void ActualizarAnimacion(float valor)
    {
        if (animator != null)
        {
            // Usamos un Float para que el Animator decida entre caminar/correr segun la velocidad
            animator.SetFloat(parametroVelocidad, valor);
        }
    }

    private void AplicarGravedadSolo()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ToggleMoto()
    {
        isUsingMoto = !isUsingMoto;

        if (normalSkin) normalSkin.SetActive(!isUsingMoto);
        if (motoSkin) motoSkin.SetActive(isUsingMoto);

        moveSpeed = isUsingMoto ? 15f : 8f;

        // Al cambiar de skin, refrescamos la referencia del Animator si es necesario
        // (Por si la moto tiene su propio Animator independiente)
        animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Moto"))
        {
            hasMoto = true;
            Debug.Log("Moto obtenida. Presiona 1 para usarla.");
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