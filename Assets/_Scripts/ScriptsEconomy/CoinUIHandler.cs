using UnityEngine;
using TMPro;
using System.Collections;

public class CoinUIHandler : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI sessionText;
    public CanvasGroup coinCanvasGroup;

    [Header("Configuración")]
    public float displayDuration = 3f;
    public float fadeSpeed = 3f;

    private Coroutine currentRoutine;

    void Start()
    {
        // Fuerza la transparencia a 0 al iniciar, sin desactivar el objeto
        if (coinCanvasGroup != null)
        {
            coinCanvasGroup.alpha = 0f;
        }
    }

    public void UpdateSessionUI()
    {
        // Actualiza el texto
        if (SessionManager.Instance != null && sessionText != null)
        {
            sessionText.text = SessionManager.Instance.coinsCollectedThisRun.ToString() + "c";
        }

        // Ejecuta la animación de desvanecimiento
        if (coinCanvasGroup != null)
        {
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(FadeInOutRoutine());
        }
    }

    private IEnumerator FadeInOutRoutine()
    {
        // 1. Aparecer (Fade In)
        while (coinCanvasGroup.alpha < 1f)
        {
            coinCanvasGroup.alpha += Time.deltaTime * (fadeSpeed * 2);
            yield return null;
        }
        coinCanvasGroup.alpha = 1f;

        // 2. Mantener en pantalla (3 segundos)
        yield return new WaitForSeconds(displayDuration);

        // 3. Desaparecer suave (Fade Out)
        while (coinCanvasGroup.alpha > 0f)
        {
            coinCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        coinCanvasGroup.alpha = 0f;
    }
}