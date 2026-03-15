using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    private GameObject jugador;
    private bool puedeUsarPortal = true;

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

        // --- CORRECCIÓN DE ESTADO INICIAL ---
        // Si el nivel está pendiente de animación (1) o no completado (0), 
        // forzamos el mástil abajo de inmediato.
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
        // ------------------------------------

        if (estado == 1)
        {
            StartCoroutine(SecuenciaRetorno(key));
        }
        else
        {
            if (fadeGroup != null) fadeGroup.alpha = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (puedeUsarPortal && other.CompareTag("Player"))
        {
            StartCoroutine(IrAlNivel());
        }
    }

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

        // El fade empieza en 1 (negro) para ocultar el snap de posición
        if (fadeGroup != null) fadeGroup.alpha = 1;

        // Teletransporte y Rotación
        Vector3 posFinal = puntoObservacion.position;
        posFinal.y += offsetEnY;
        jugador.transform.position = posFinal;

        Vector3 dir = (soporteBandera.position - jugador.transform.position);
        dir.y = 0;
        if (dir != Vector3.zero) jugador.transform.rotation = Quaternion.LookRotation(dir);

        Physics.SyncTransforms();

        yield return new WaitForSeconds(0.4f);
        if (fadeGroup != null) yield return StartCoroutine(Fade(0, 0.8f));

        // Animación Mástil
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