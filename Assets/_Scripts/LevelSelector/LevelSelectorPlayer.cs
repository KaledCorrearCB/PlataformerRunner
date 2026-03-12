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
        // Estado inicial
        if (normalSkin) normalSkin.SetActive(true);
        if (motoSkin) motoSkin.SetActive(false);
    }

    private void Update()
    {
        MoverPersonaje();

        // Tecla 1 para cambiar de skin si ya tiene la moto
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasMoto)
        {
            ToggleMoto();
        }
    }

    private void MoverPersonaje()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;

        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si tocamos el item de la moto
        if (other.CompareTag("Moto"))
        {
            hasMoto = true;
            Debug.Log("Moto obtenida. Presiona 1 para usarla.");
            Destroy(other.gameObject); // Desaparece el item del mapa
        }
    }

    private void ToggleMoto()
    {
        isUsingMoto = !isUsingMoto;

        // Cambiamos visibilidad
        normalSkin.SetActive(!isUsingMoto);
        motoSkin.SetActive(isUsingMoto);

        // Opcional: Aumentar velocidad si usa la moto
        moveSpeed = isUsingMoto ? 15f : 8f;
    }
}