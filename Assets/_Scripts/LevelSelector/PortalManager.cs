using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

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

    // --- VARIABLES DE INTERACCIÓN ---
    private PlayerInput inputActions; // Clase generada por el Input System
    private bool jugadorEstaCerca = false;
    private GameObject jugador;
    private bool puedeUsarPortal = true;

    void Awake()
    {
        // Instanciamos la clase de acciones generada
        inputActions = new PlayerInput();
    }

    void OnEnable()
    {
        // Activamos el mapa "Player" (el que configuramos ayer)
        inputActions.Player.Enable();

        // Suscribimos la acción "Interact" para que ejecute la entrada al nivel
        inputActions.Player.Interact.performed += OnInteractPerformed;
    }

    void OnDisable()
    {
        // Limpieza de suscripciones y desactivación
        inputActions.Player.Interact.performed -= OnInteractPerformed;
        inputActions.Player.Disable();
    }

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");
        string key = "NivelCompletado_" + portalID;

        // Reset de progreso para pruebas
        if (resetearEnStart)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        // Cargar estado de la bandera (0: Bloqueado, 1: Recién completado, 2: Ya completado)
        int estado = PlayerPrefs.GetInt(key, 0);

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

        // Si venimos de completar el nivel, activamos la secuencia de la bandera
        if (estado == 1)
        {
            StartCoroutine(SecuenciaRetorno(key));
        }
        else
        {
            if (fadeGroup != null) fadeGroup.alpha = 0;
        }
    }

    // --- DETECCIÓN POR TRIGGER ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && puedeUsarPortal)
        {
            jugadorEstaCerca = true;
            Debug.Log($"[Portal {portalID}] Cerca. Pulsa el botón de interacción.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEstaCerca = false;
        }
    }

    // --- LÓGICA DE INTERACCIÓN ---

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (jugadorEstaCerca && puedeUsarPortal)
        {
            StartCoroutine(IrAlNivel());
        }
    }

    IEnumerator IrAlNivel()
    {
        puedeUsarPortal = false;

        // Solo desactivamos INPUT, no el script completo
        // Así el CharacterController sigue aplicando gravedad durante el fade
        if (scriptMovimientoJugador != null)
        {
            LevelSelectorPlayer lsp = scriptMovimientoJugador as LevelSelectorPlayer;
            if (lsp != null) LevelSelectorPlayer.puedeMoversis = false;
        }

        if (fadeGroup != null) yield return StartCoroutine(Fade(1, 0.5f));

        SceneManager.LoadScene(nombreEscena);

    }

    IEnumerator SecuenciaRetorno(string key)
    {
        puedeUsarPortal = false;

        // Bloqueo total de controles
        if (scriptMovimientoJugador != null) scriptMovimientoJugador.enabled = false;
        CharacterController cc = jugador.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (fadeGroup != null) fadeGroup.alpha = 1;

        // Posicionar al jugador en el punto de observación
        Vector3 posFinal = puntoObservacion.position;
        posFinal.y += offsetEnY;
        jugador.transform.position = posFinal;

        // Orientar al jugador hacia la bandera
        Vector3 dir = (soporteBandera.position - jugador.transform.position);
        dir.y = 0;
        if (dir != Vector3.zero) jugador.transform.rotation = Quaternion.LookRotation(dir);

        Physics.SyncTransforms();

        yield return new WaitForSeconds(0.4f);
        if (fadeGroup != null) yield return StartCoroutine(Fade(0, 0.8f));

        // Animación de escala de la bandera (el soporte sube)
        float elapsed = 0;
        while (elapsed < 1.5f)
        {
            soporteBandera.localScale = Vector3.Lerp(new Vector3(1, 0, 1), Vector3.one, elapsed / 1.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        soporteBandera.localScale = Vector3.one;
        bandera.SetActive(true);

        // Guardar progreso definitivo
        PlayerPrefs.SetInt(key, 2);
        PlayerPrefs.Save();

        // Reactivar controles
        if (cc != null) cc.enabled = true;
        if (scriptMovimientoJugador != null) scriptMovimientoJugador.enabled = true;

        yield return new WaitForSeconds(1.0f);
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