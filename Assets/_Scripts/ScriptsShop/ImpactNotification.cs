// ImpactNotification.cs
// Adjúntalo al panel de notificación en tu Canvas
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImpactNotification : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject notificationPanel;   // El panel/pestaña completa
    public TextMeshProUGUI messageText;    // Texto del mensaje
    public Button closeButton;             // Botón para cerrar

    [Header("Animación")]
    public float slideInDuration = 0.5f;   // Segundos que tarda en aparecer
    public float autoHideAfter = 0f;       // 0 = no se oculta solo, >0 = segundos

    [Header("Posiciones (en píxeles Y)")]
    public float hiddenPositionY = -300f;  // Fuera de pantalla (abajo)
    public float visiblePositionY = 0f;    // Posición visible

    private RectTransform rectTransform;
    private Coroutine currentAnimation;

    void Awake()
    {
        rectTransform = notificationPanel.GetComponent<RectTransform>();

        if (closeButton != null)
            closeButton.onClick.AddListener(HideMessage);

        // Empezar oculto
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        notificationPanel.SetActive(true);

        // Cancelar animación anterior si existía
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(SlideIn());

        if (autoHideAfter > 0f)
            StartCoroutine(AutoHide());
    }

    public void HideMessage()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(SlideOut());
    }

    private IEnumerator SlideIn()
    {
        float elapsed = 0f;
        Vector2 startPos = new Vector2(rectTransform.anchoredPosition.x, hiddenPositionY);
        Vector2 endPos = new Vector2(rectTransform.anchoredPosition.x, visiblePositionY);

        rectTransform.anchoredPosition = startPos;

        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideInDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
    }

    private IEnumerator SlideOut()
    {
        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(rectTransform.anchoredPosition.x, hiddenPositionY);

        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideInDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        notificationPanel.SetActive(false);
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(autoHideAfter);
        HideMessage();
    }
}