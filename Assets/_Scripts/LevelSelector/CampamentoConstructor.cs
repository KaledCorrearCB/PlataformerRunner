using UnityEngine;
using TMPro;

public class CampamentoConstructor : MonoBehaviour
{
    [Header("Configuracion")]
    public string idEstructura = "Carpa_Principal_01";
    public int precioSoles = 50;
    public float distanciaParaConstruir = 8.0f;

    [Header("Referencias")]
    public Transform jugador;
    public GameObject modeloCarpa;
    public GameObject visualAuxiliar;
    public TextMeshProUGUI textoCosto;

    private bool yaConstruido = false;

    void Start()
    {
        // Carga el estado usando la clave de monedas del sistema global
        yaConstruido = PlayerPrefs.GetInt(idEstructura, 0) == 1;

        if (textoCosto != null) textoCosto.text = precioSoles.ToString();

        ActualizarEstado();
    }

    void Update()
    {
        if (yaConstruido || jugador == null) return;

        // Calculamos distancia ignorando la altura (Y) para evitar el error de los 5 metros
        Vector3 posBaldosa = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 posJugador = new Vector3(jugador.position.x, 0, jugador.position.z);
        float distanciaPlana = Vector3.Distance(posBaldosa, posJugador);

        if (distanciaPlana <= distanciaParaConstruir)
        {
            // Verificamos la tecla E
            if (Input.GetKeyDown(KeyCode.E))
            {
                IntentarComprar();
            }
        }
    }

    void IntentarComprar()
    {
        // Clave exacta usada por el sistema de tus compañeros en GameData
        int monedasTotales = PlayerPrefs.GetInt("TotalCoins", 0);

        if (monedasTotales >= precioSoles)
        {
            // Restar y guardar en el registro global
            int nuevasMonedas = monedasTotales - precioSoles;
            PlayerPrefs.SetInt("TotalCoins", nuevasMonedas);
            PlayerPrefs.SetInt(idEstructura, 1);
            PlayerPrefs.Save();

            yaConstruido = true;
            ActualizarEstado();
            Debug.Log("Compra exitosa. Estructura generada.");
        }
        else
        {
            Debug.Log("Monedas insuficientes. Tienes: " + monedasTotales);
        }
    }

    void ActualizarEstado()
    {
        if (modeloCarpa != null) modeloCarpa.SetActive(yaConstruido);
        if (visualAuxiliar != null) visualAuxiliar.SetActive(!yaConstruido);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // Dibuja el radio de accion en el suelo
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), distanciaParaConstruir);
    }
}