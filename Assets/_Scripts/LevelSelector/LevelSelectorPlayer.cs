using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class LevelSelectorPlayer : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    public float gravity = -20f;

    [Header("Skins e Inventario")]
    public GameObject normalSkin;
    public GameObject motoSkin;
    private bool hasMoto = false;
    private bool isUsingMoto = false;

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Estado inicial de las skins
        if (normalSkin) normalSkin.SetActive(true);
        if (motoSkin) motoSkin.SetActive(false);
    }

    private void Update()
    {
        MoverPersonaje();

        // Tecla 1 para cambiar de skin si ya tiene la moto en el inventario
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasMoto)
        {
            ToggleMoto();
        }
    }

    private void MoverPersonaje()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Movimiento relativo al mundo (isométrico)
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Gravedad para no flotar en los cambios de relieve del mapa
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Rotación y Traslación
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // Aplicar la velocidad vertical (caída)
        controller.Move(velocity * Time.deltaTime);
    }

    private void ToggleMoto()
    {
        isUsingMoto = !isUsingMoto;

        // Cambiamos visibilidad de los modelos hijos
        if (normalSkin) normalSkin.SetActive(!isUsingMoto);
        if (motoSkin) motoSkin.SetActive(isUsingMoto);

        // Aumentar velocidad si usa la moto (ajusta el 15f a tu gusto)
        moveSpeed = isUsingMoto ? 15f : 8f;
    }

    // --- DETECCION DE TRIGGERS (Items) ---
    private void OnTriggerEnter(Collider other)
    {
        // Detectar si tocamos el item de la moto para guardarlo en el inventario
        if (other.CompareTag("Moto"))
        {
            hasMoto = true;
            Debug.Log("Moto obtenida. Presiona 1 para usarla.");
            Destroy(other.gameObject);
        }
    }

    // --- INTERACCION JELLY (Choques con arbustos/arboles) ---
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Verificamos si el objeto que chocamos tiene el Tag que creamos para los arbustos
        if (hit.gameObject.CompareTag("ObstaculoWobble"))
        {
            // Buscamos el script ProceduralWobble en el arbusto
            ProceduralWobble wobbler = hit.gameObject.GetComponent<ProceduralWobble>();

            if (wobbler != null)
            {
                // Activamos el efecto de gelatina
                wobbler.TriggerWobble();
            }
        }
    }
}