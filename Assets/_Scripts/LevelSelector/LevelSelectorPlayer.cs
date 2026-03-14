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

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        puedeMoversis = true; // Aseguramos que inicie pudiendo moverse

        // Estado inicial de las skins
        if (normalSkin) normalSkin.SetActive(true);
        if (motoSkin) motoSkin.SetActive(false);
    }

    private void Update()
    {
        // SOLO PROCESAMOS INPUT SI ESTÁ DESBLOQUEADO
        if (puedeMoversis)
        {
            MoverPersonaje();

            // Tecla 1 para cambiar de skin si ya tiene la moto en el inventario
            if (Input.GetKeyDown(KeyCode.Alpha1) && hasMoto)
            {
                ToggleMoto();
            }
        }
        else
        {
            // SI ESTÁ BLOQUEADO: Aplicamos gravedad de todas formas para evitar que flote
            AplicarGravedadSolo();
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
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void AplicarGravedadSolo()
    {
        // Esto mantiene al personaje pegado al suelo durante la cutscene si hay desniveles
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