using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

public class PortalManager : MonoBehaviour
{
    [Header("Configuración de Desarrollo")]
    public bool resetearEnStart = false;
    public int portalID = 1;
    public string nombreEscena;

    [Header("Ajuste de Altura")]
    public float offsetEnY = 1.08f;

    [Header("Referencias de Posición")]
    public Transform puntoObservacion;
    public Transform soporteBandera;

    [Header("Referencias Visuales")]
    public GameObject bandera;
    public CanvasGroup fadeGroup;
    public MonoBehaviour scriptMovimientoJugador;

    // --- NUEVAS VARIABLES DE INTERACCIÓN ---
    private PlayerInput inputActions;
    private bool jugadorEstaCerca = false;
    private GameObject jugador;
    private bool puedeUsarPortal = true;

    void Awake()
    {
        // Inicializamos el mapa de controles generado (PlayerInput.cs)
        inputActions = new PlayerInput();
    }

    void OnEnable()
    {
        // Activamos el mapa de acciones "Gameplay"
        inputActions.Gameplay.Enable();

        // Suscribimos el botón de "Interact" (Espacio/Mando)
        // Cuando se presione, ejecutará la función IntentarEntrar
        inputActions.Gameplay.Interact.performed += ctx => IntentarEntrarAlNivel();
    }

    void OnDisable()
    {
        inputActions.Gameplay.Disable();
    }

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");
        string key = "NivelCompletado_" + portalID;

        if (resetearEnStart)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        int estado = PlayerPrefs.GetInt(key, 0);

        // Forzado de estado visual inicial (Anti-glitch)
        if (estado == 0 || estado == 1)
        {
            soporteBandera.localScale = new Vector3(1, 0, 1);
            bandera.SetActive(false);
        }
        else if (estado == 2)
        {
            soporteBandera.localScale = Vector3.one;
            bandera.SetActive(true);
        }

        if (estado == 1)
        {
            StartCoroutine(SecuenciaRetorno(key));
        }
        else
        {
            if (fadeGroup != null) fadeGroup.alpha = 0;
        }
    }

    // --- LÓGICA DE DETECCIÓN ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && puedeUsarPortal)
        {
            jugadorEstaCerca = true;
            Debug.Log("Jugador en zona del portal " + portalID + ". Presiona ESPACIO.");
            // NOTA: Aquí activaremos el Panel de Info (Estrellas/Foto) en el siguiente paso.
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEstaCerca = false;
            Debug.Log("Jugador salió de la zona.");
            // NOTA: Aquí ocultaremos el Panel de Info en el siguiente paso.
        }
    }

    // Función que llama el Input System
    private void IntentarEntrarAlNivel()
    {
        if (jugadorEstaCerca && puedeUsarPortal)
        {
            StartCoroutine(IrAlNivel());
        }
    }

    // --- CORRUTINAS DE TRANSICIÓN ---

    IEnumerator IrAlNivel()
    {
        puedeUsarPortal = false;
        if (fadeGroup != null) yield return StartCoroutine(Fade(1, 0.5f));
        SceneManager.LoadScene(nombreEscena);
    }

    IEnumerator SecuenciaRetorno(string key)
    {
        puedeUsarPortal = false;

        if (scriptMovimientoJugador != null) scriptMovimientoJugador.enabled = false;
        CharacterController cc = jugador.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (fadeGroup != null) fadeGroup.alpha = 1;

        Vector3 posFinal = puntoObservacion.position;
        posFinal.y += offsetEnY;
        jugador.transform.position = posFinal;

        Vector3 dir = (soporteBandera.position - jugador.transform.position);
        dir.y = 0;
        if (dir != Vector3.zero) jugador.transform.rotation = Quaternion.LookRotation(dir);

        Physics.SyncTransforms();

        yield return new WaitForSeconds(0.4f);
        if (fadeGroup != null) yield return StartCoroutine(Fade(0, 0.8f));

        float elapsed = 0;
        while (elapsed < 1.5f)
        {
            soporteBandera.localScale = Vector3.Lerp(new Vector3(1, 0, 1), Vector3.one, elapsed / 1.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        soporteBandera.localScale = Vector3.one;
        bandera.SetActive(true);

        PlayerPrefs.SetInt(key, 2);
        PlayerPrefs.Save();

        if (cc != null) cc.enabled = true;
        if (scriptMovimientoJugador != null) scriptMovimientoJugador.enabled = true;

        yield return new WaitForSeconds(2.0f);
        puedeUsarPortal = true;
    }

    IEnumerator Fade(float target, float time)
    {
        float start = fadeGroup.alpha;
        float elapsed = 0;
        while (elapsed < time)
        {
            fadeGroup.alpha = Mathf.Lerp(start, target, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = target;
    }
}