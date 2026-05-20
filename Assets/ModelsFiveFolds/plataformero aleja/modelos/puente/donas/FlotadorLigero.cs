using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlotadorLigero : MonoBehaviour
{
    [Header("F�sicas Ligeras")]
    [Tooltip("Gravedad personalizada. Unity usa 9.8, un n�mero bajo lo hace 'flotar' m�s tiempo en el aire.")]
    public float gravedadSuave = 3f;
    public float fuerzaDeRebote = 7f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Configuramos el Rigidbody para que sea como un globo/pelota de playa
        rb.useGravity = false; // Apagamos la gravedad normal de yunque
        rb.mass = 0.2f;        // Muy ligero
        rb.linearDamping = 0.5f;        // Resistencia al aire para que el rebote sea flotadito
        rb.angularDamping = 0.5f;
    }

    void FixedUpdate()
    {
        // Le aplicamos nuestra propia gravedad suave hacia abajo constantemente
        rb.AddForce(Vector3.down * gravedadSuave, ForceMode.Acceleration);
    }

    // Usamos OnTriggerEnter para detectar f�cilmente al CharacterController
    private void OnTriggerEnter(Collider other)
    {
        // Tu jugador debe tener la etiqueta "Player" asignada en el editor
        if (other.CompareTag("Player"))
        {
            // Calculamos hacia d�nde empujar la pelota (alej�ndose del jugador)
            Vector3 direccionEmpuje = transform.position - other.transform.position;

            // Forzamos un poco de elevaci�n para que siempre salte al chocar
            direccionEmpuje.y = 1f;

            // Reseteamos su velocidad actual para que el nuevo rebote sea limpio
            rb.linearVelocity = Vector3.zero;

            // Aplicamos el empuj�n
            rb.AddForce(direccionEmpuje.normalized * fuerzaDeRebote, ForceMode.Impulse);
        }
    }
}