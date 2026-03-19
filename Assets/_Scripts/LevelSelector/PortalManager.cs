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

    private bool jugadorEstaCerca = false;
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

        if (estado == 1) StartCoroutine(SecuenciaRetorno(key));
        else if (fadeGroup != null) fadeGroup.alpha = 0;
    }

    void Update()
    {
        if (jugadorEstaCerca && puedeUsarPortal && LevelSelectorPlayer.puedeMoversis)
        {
            bool interaccionPresionada = false;

            // Detección de Teclado
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                interaccionPresionada = true;

            // Detección de Mando (Triángulo o X/A)
            if (Gamepad.current != null)
            {
                // buttonNorth es el Triángulo en PS
                // buttonSouth es la X en PS
                if (Gamepad.current.buttonNorth.wasPressedThisFrame ||
                    Gamepad.current.buttonSouth.wasPressedThisFrame)
                {
                    interaccionPresionada = true;
                }
            }

            if (interaccionPresionada)
            {
                Debug.Log("Interacción detectada. Entrando a: " + nombreEscena);
                StartCoroutine(IrAlNivel());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) jugadorEstaCerca = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) jugadorEstaCerca = false;
    }

    IEnumerator IrAlNivel()
    {
        puedeUsarPortal = false;
        LevelSelectorPlayer.puedeMoversis = false;

        if (fadeGroup != null) yield return StartCoroutine(Fade(1, 0.5f));
        SceneManager.LoadScene(nombreEscena);
    }

    IEnumerator SecuenciaRetorno(string key)
    {
        puedeUsarPortal = false;
        LevelSelectorPlayer.puedeMoversis = false;

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

        yield return new WaitForSeconds(1.0f);
        LevelSelectorPlayer.puedeMoversis = true;
        puedeUsarPortal = true;
    }

    IEnumerator Fade(float target, float time)
    {
        if (fadeGroup == null) yield break;
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