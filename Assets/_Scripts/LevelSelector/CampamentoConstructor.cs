using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CampamentoConstructor : MonoBehaviour
{
    [Header("Configuracion Economica")]
    public string idEstructura = "Carpa_01";
    public int precioSoles = 50;
    public float radioDeActivacion = 2.5f;

    [Header("Interaccion UI Visual")]
    public GameObject panelTeclaE;
    public Sprite spriteTeclado;
    public Sprite spriteMando;
    public KeyCode teclaAccion = KeyCode.E;

    [Header("Animacion Tecla (Smooth)")]
    public float amplitudFlotado = 0.15f;
    public float velocidadFlotado = 3f;
    private Vector3 posInicialTecla;

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

    [Header("Referencias UI/Objetos")]
    public Slider barraProgreso;
    public GameObject modeloCarpa;
    public GameObject visualAuxiliar;
    public Camera camaraJuego;

    [Header("VFX y Audio")]
    public ParticleSystem particulasPolvoBase;
    public AudioSource audioConstruccion;

    [Header("Debug")]
    public bool resetearEnStart = true;

    private Transform jugador;
    private CharacterController controller;
    private LevelSelectorPlayer scriptJugador; // Referencia para animaciones
    private bool yaConstruido = false;
    private bool enSecuencia = false;
    private float fovOriginal;
    private Canvas canvasMundo;
    private SpriteRenderer sr;
    private Image img;
    private static bool usandoMando = false;

    void Start()
    {
        InputSystem.onActionChange += OnActionChange;
        if (resetearEnStart)
        {
            PlayerPrefs.DeleteKey(idEstructura);
            PlayerPrefs.Save();
        }
        yaConstruido = PlayerPrefs.GetInt(idEstructura, 0) == 1;
        if (camaraJuego == null) camaraJuego = Camera.main;
        if (camaraJuego != null) fovOriginal = camaraJuego.fieldOfView;
        if (barraProgreso != null)
        {
            canvasMundo = barraProgreso.GetComponentInParent<Canvas>();
            if (canvasMundo != null) canvasMundo.gameObject.SetActive(false);
        }
        if (panelTeclaE != null)
        {
            posInicialTecla = panelTeclaE.transform.localPosition;
            panelTeclaE.SetActive(false);
            sr = panelTeclaE.GetComponent<SpriteRenderer>() ?? panelTeclaE.GetComponentInChildren<SpriteRenderer>();
            img = panelTeclaE.GetComponent<Image>() ?? panelTeclaE.GetComponentInChildren<Image>();
        }
        ActualizarVisuales();
        ActualizarIconoInput();
    }

    private void OnDestroy() => InputSystem.onActionChange -= OnActionChange;

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted)
        {
            var action = (InputAction)obj;
            if (action.activeControl != null)
            {
                usandoMando = action.activeControl.device is Gamepad;
                ActualizarIconoInput();
            }
        }
    }

    void Update()
    {
        if (yaConstruido || enSecuencia)
        {
            if (panelTeclaE != null && panelTeclaE.activeSelf) panelTeclaE.SetActive(false);
            return;
        }
        if (jugador == null) { BuscarJugador(); return; }

        float distancia = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                           new Vector3(jugador.position.x, 0, jugador.position.z));

        if (distancia <= radioDeActivacion)
        {
            GestionarIndicadorE(true);
            bool presionoInteractuar = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                                     (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);

            if (presionoInteractuar) IniciarProceso();
        }
        else GestionarIndicadorE(false);
    }

    void BuscarJugador()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            jugador = p.transform;
            controller = p.GetComponent<CharacterController>();
            scriptJugador = p.GetComponent<LevelSelectorPlayer>();
        }
    }

    void IniciarProceso()
    {
        if (PlayerPrefs.GetInt("TotalCoins", 0) >= precioSoles) StartCoroutine(CutsceneConstruccion());
    }

    IEnumerator CutsceneConstruccion()
    {
        enSecuencia = true;
        if (panelTeclaE != null) panelTeclaE.SetActive(false);
        LevelSelectorPlayer.puedeMoversis = false;

        // --- FASE 1: CAMINAR HACIA EL PUNTO ---
        while (true)
        {
            Vector3 posJugador = new Vector3(jugador.position.x, 0, jugador.position.z);
            Vector3 posDestino = new Vector3(puntoCaminar.position.x, 0, puntoCaminar.position.z);
            float dist = Vector3.Distance(posJugador, posDestino);

            if (dist <= 0.2f) break;

            Vector3 direccion = (posDestino - posJugador).normalized;
            if (direccion != Vector3.zero)
                jugador.rotation = Quaternion.Slerp(jugador.rotation, Quaternion.LookRotation(direccion), 15f * Time.deltaTime);

            controller.Move(direccion * velocidadCaminar * Time.deltaTime);

            // ACTIVAR ANIMACIÓN DE CAMINATA
            if (scriptJugador != null) scriptJugador.ActualizarAnimacion(1f);

            if (camaraJuego != null) camaraJuego.fieldOfView = Mathf.Lerp(camaraJuego.fieldOfView, zoomFOV, 5f * Time.deltaTime);
            yield return null;
        }

        // DETENER ANIMACIÓN AL LLEGAR
        if (scriptJugador != null) scriptJugador.ActualizarAnimacion(0f);

        // --- FASE 2: MIRAR A LA CARPA ---
        Vector3 dirHaciaCarpa = (new Vector3(transform.position.x, 0, transform.position.z) -
                                 new Vector3(jugador.position.x, 0, jugador.position.z)).normalized;
        if (dirHaciaCarpa != Vector3.zero)
        {
            Quaternion rotDestino = Quaternion.LookRotation(dirHaciaCarpa);
            while (Quaternion.Angle(jugador.rotation, rotDestino) > 1f)
            {
                jugador.rotation = Quaternion.RotateTowards(jugador.rotation, rotDestino, velocidadRotacion * Time.deltaTime);
                yield return null;
            }
        }

        // --- FASE 3: CONSTRUCCIÓN ---
        if (barraProgreso != null && canvasMundo != null)
        {
            canvasMundo.gameObject.SetActive(true);
            float t = 0;
            while (t < esperaMirandoVacio)
            {
                t += Time.deltaTime;
                barraProgreso.value = t / esperaMirandoVacio;
                yield return null;
            }
            canvasMundo.gameObject.SetActive(false);
        }

        PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins") - precioSoles);
        PlayerPrefs.SetInt(idEstructura, 1);
        PlayerPrefs.Save();
        yaConstruido = true;
        ActualizarVisuales();

        if (particulasPolvoBase != null) particulasPolvoBase.Play();
        if (audioConstruccion != null) { audioConstruccion.pitch = Random.Range(0.9f, 1.1f); audioConstruccion.Play(); }
        if (modeloCarpa != null) StartCoroutine(EfectoReboteImpacto(modeloCarpa.transform));

        yield return new WaitForSeconds(pausaEnfoqueFinal);

        // --- FASE 4: ZOOM OUT Y LIBERAR ---
        float tZoom = 0;
        while (tZoom < 1f)
        {
            tZoom += Time.deltaTime / duracionZoomOut;
            if (camaraJuego != null) camaraJuego.fieldOfView = Mathf.Lerp(zoomFOV, fovOriginal, tZoom);
            yield return null;
        }

        LevelSelectorPlayer.puedeMoversis = true;
        enSecuencia = false;
    }

    IEnumerator EfectoReboteImpacto(Transform objeto)
    {
        float t = 0;
        Vector3 posOriginal = objeto.localPosition;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            float decay = Mathf.Exp(-t * amortiguacion);
            float bounceY = Mathf.Cos(t * velocidadRebote) * decay * fuerzaEscalaY;
            objeto.localScale = new Vector3(1f, 1f - bounceY, 1f);
            float saltoY = Mathf.Abs(Mathf.Sin(t * velocidadRebote)) * decay * fuerzaSaltoY;
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

    void GestionarIndicadorE(bool estado)
    {
        if (panelTeclaE != null)
        {
            if (panelTeclaE.activeSelf != estado) panelTeclaE.SetActive(estado);
            if (estado && camaraJuego != null)
            {
                panelTeclaE.transform.LookAt(panelTeclaE.transform.position + camaraJuego.transform.forward);
                float y = posInicialTecla.y + Mathf.Sin(Time.time * velocidadFlotado) * amplitudFlotado;
                panelTeclaE.transform.localPosition = new Vector3(posInicialTecla.x, y, posInicialTecla.z);
            }
        }
    }

    void ActualizarIconoInput()
    {
        Sprite s = usandoMando ? spriteMando : spriteTeclado;
        if (s == null) return;
        if (sr != null) sr.sprite = s;
        if (img != null) img.sprite = s;
    }
}