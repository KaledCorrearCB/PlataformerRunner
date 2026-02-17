using UnityEngine;
using TMPro;
using System.Collections;

public class CoinUIHandler : MonoBehaviour
{
    public float displayDuration = 3f;

    [Header("Referencias")]
    public TextMeshProUGUI coinText; // Ahora la UI maneja su propio texto

    private CanvasGroup canvasGroup;
    private Coroutine hideCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        // Actualizar el número al cargar cualquier escena
        UpdateText();
    }

    public void ShowCoins()
    {
        UpdateText(); // Actualizamos el número antes de mostrarlo

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        canvasGroup.alpha = 1f;
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    // Nueva función centralizada
    private void UpdateText()
    {
        if (coinText != null)
        {
            coinText.text = GameData.GetTotalCoins().ToString() + "c";
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 2f;
            yield return null;
        }
    }
}