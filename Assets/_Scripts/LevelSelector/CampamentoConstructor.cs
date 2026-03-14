using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CampamentoConstructor : MonoBehaviour
{
    [Header("Configuracion Economica")]
    public string idEstructura = "Carpa_Principal_01";
    public int precioSoles = 50;
    public float radioDeActivacion = 2.0f;

    [Header("Cinematica")]
    public Transform puntoCaminar;
    public float velocidadCaminar = 5f;
    public float velocidadRotacion = 1200f;
    public float esperaMirandoVacio = 2.0f;
    public float pausaEnfoqueFinal = 1.0f;
    public float zoomFOV = 40f;
    public float duracionZoomOut = 1.5f;

    [Header("Ajustes de Rebote (Carpa)")]
    [Range(0, 2)] public float fuerzaSaltoY = 0.4f;
    [Range(0, 1)] public float fuerzaEscalaY = 0.3f;
    [Range(5, 30)] public float velocidadRebote = 12f;
    [Range(1, 10)] public float amortiguacion = 4f;

    [Header("Referencias UI")]
    public Slider barraProgreso;
    public GameObject modeloCarpa;
    public GameObject visualAuxiliar;
    public Camera camaraJuego;

    [Header("VFX y Audio (Nuevos)")]
    [Tooltip("Arrastra aqui el Particle System del polvo")]
    public ParticleSystem particulasPolvoBase;
    [Tooltip("Arrastra aqui el AudioSource con el sonido de golpe")]
    public AudioSource audioConstruccion;

    [Header("Debug")]
    public bool resetearEnStart = true;

    private Transform jugador;
    private CharacterController controller;
    private bool yaConstruido = false;
    private bool enSecuencia = false;
    private float fovOriginal;
    private Canvas canvasMundo;

    void Start()
    {
        if (resetearEnStart) { PlayerPrefs.DeleteKey(idEstructura); PlayerPrefs.Save(); }
        yaConstruido = PlayerPrefs.GetInt(idEstructura, 0) == 1;

        if (camaraJuego != null) fovOriginal = camaraJuego.fieldOfView;

        if (barraProgreso != null)
        {
            canvasMundo = barraProgreso.GetComponentInParent<Canvas>();
            if (canvasMundo != null) canvasMundo.gameObject.SetActive(false);
        }

        ActualizarVisuales();
    }

    void Update()
    {
        if (yaConstruido || enSecuencia) return;
        if (jugador == null) { BuscarJugador(); return; }

        float distancia = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                           new Vector3(jugador.position.x, 0, jugador.position.z));

        if (distancia <= radioDeActivacion) IniciarProceso();
    }

    void BuscarJugador()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { jugador = p.transform; controller = p.GetComponent<CharacterController>(); }
    }

    void IniciarProceso()
    {
        int monedas = PlayerPrefs.GetInt("TotalCoins", 0);
        if (monedas >= precioSoles) StartCoroutine(CutsceneConstruccion());
    }

    IEnumerator CutsceneConstruccion()
    {
        enSecuencia = true;
        LevelSelectorPlayer.puedeMoversis = false;

        // 1. DESPLAZAMIENTO
        while (true)
        {
            Vector3 posJugador = new Vector3(jugador.position.x, 0, jugador.position.z);
            Vector3 posDestino = new Vector3(puntoCaminar.position.x, 0, puntoCaminar.position.z);
            if (Vector3.Distance(posJugador, posDestino) <= 0.4f) break;

            Vector3 direccion = (posDestino - posJugador).normalized;
            if (direccion != Vector3.zero)
                jugador.rotation = Quaternion.Slerp(jugador.rotation, Quaternion.LookRotation(direccion), 15f * Time.deltaTime);

            controller.Move(direccion * velocidadCaminar * Time.deltaTime);
            if (camaraJuego != null)
                camaraJuego.fieldOfView = Mathf.Lerp(camaraJuego.fieldOfView, zoomFOV, 5f * Time.deltaTime);
            yield return null;
        }

        // 2. GIRO HACIA EL EDIFICIO
        Vector3 dirHaciaCarpa = (new Vector3(transform.position.x, 0, transform.position.z) -
                                 new Vector3(jugador.position.x, 0, jugador.position.z)).normalized;
        if (dirHaciaCarpa != Vector3.zero)
        {
            Quaternion rotacionDestino = Quaternion.LookRotation(dirHaciaCarpa);
            while (Quaternion.Angle(jugador.rotation, rotacionDestino) > 0.1f)
            {
                jugador.rotation = Quaternion.RotateTowards(jugador.rotation, rotacionDestino, velocidadRotacion * Time.deltaTime);
                yield return null;
            }
            jugador.rotation = rotacionDestino;
        }

        // 3. BARRA DE CARGA
        if (barraProgreso != null && canvasMundo != null)
        {
            canvasMundo.gameObject.SetActive(true);
            barraProgreso.value = 0;
            float tiempoPasado = 0;
            while (tiempoPasado < esperaMirandoVacio)
            {
                tiempoPasado += Time.deltaTime;
                barraProgreso.value = tiempoPasado / esperaMirandoVacio;
                if (camaraJuego != null) canvasMundo.transform.rotation = camaraJuego.transform.rotation;
                yield return null;
            }
            canvasMundo.gameObject.SetActive(false);
        }

        // 4. GENERACIÓN + FEEDBACK (Audio y VFX)
        PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins") - precioSoles);
        PlayerPrefs.SetInt(idEstructura, 1);
        PlayerPrefs.Save();
        yaConstruido = true;
        ActualizarVisuales();

        // Disparo de Polvo
        if (particulasPolvoBase != null) particulasPolvoBase.Play();

        // Disparo de Sonido con ligera variacion de tono para realismo
        if (audioConstruccion != null)
        {
            audioConstruccion.pitch = Random.Range(0.9f, 1.1f);
            audioConstruccion.Play();
        }

        // Efecto de Rebote Físico
        if (modeloCarpa != null) StartCoroutine(EfectoReboteImpacto(modeloCarpa.transform));

        yield return new WaitForSeconds(pausaEnfoqueFinal);

        // 5. ZOOM OUT FINAL
        float tZoom = 0;
        while (tZoom < 1f)
        {
            tZoom += Time.deltaTime / duracionZoomOut;
            if (camaraJuego != null)
                camaraJuego.fieldOfView = Mathf.Lerp(zoomFOV, fovOriginal, tZoom);
            yield return null;
        }

        LevelSelectorPlayer.puedeMoversis = true;
        enSecuencia = false;
    }

    IEnumerator EfectoReboteImpacto(Transform objeto)
    {
        float tiempo = 0;
        float duracion = 1.2f;
        Vector3 posOriginal = objeto.localPosition;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float decay = Mathf.Exp(-tiempo * amortiguacion);

            float bounceY = Mathf.Cos(tiempo * velocidadRebote) * decay * fuerzaEscalaY;
            objeto.localScale = new Vector3(1f, 1f - bounceY, 1f);

            float saltoY = Mathf.Abs(Mathf.Sin(tiempo * velocidadRebote)) * decay * fuerzaSaltoY;
            objeto.localPosition = posOriginal + new Vector3(0, saltoY, 0);

            yield return null;
        }

        objeto.localScale = Vector3.one;
        objeto.localPosition = posOriginal;
    }

    void ActualizarVisuales()
    {
        if (modeloCarpa != null) modeloCarpa.SetActive(yaConstruido);
        if (visualAuxiliar != null) visualAuxiliar.SetActive(!yaConstruido);
    }
}