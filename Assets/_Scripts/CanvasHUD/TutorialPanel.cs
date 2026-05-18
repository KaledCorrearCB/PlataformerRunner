using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Vive en el prefab del Canvas, siempre en escena pero desactivado.
/// TutorialTrigger lo activa y le pasa el contenido.
/// El botón "Entendido" lo cierra y devuelve el control al jugador.
/// </summary>
public class TutorialPanel : MonoBehaviour
{
    [Header("Referencias UI del Panel")]
    [Tooltip("Image de Unity UI donde se mostrará la imagen de referencia.")]
    public Image referenceImage;

    [Tooltip("TextMeshPro para el texto explicativo.")]
    public TMP_Text descriptionText;

    [Tooltip("El botón 'Entendido'. Asigna su OnClick a OnUnderstoodClicked.")]
    public Button understoodButton;

    [Tooltip("El panel raíz (el GameObject que se activa/desactiva).")]
    public GameObject panelRoot;

    // ?????????????????????????????????????????????
    //  Animación de entrada (opcional pero bonita)
    // ?????????????????????????????????????????????
    [Header("Animación (opcional)")]
    [Tooltip("Si tienes un Animator en el panel, se llamará al trigger 'Show'.")]
    public Animator panelAnimator;
    private static readonly int ShowTrigger = Animator.StringToHash("Show");
    private static readonly int HideTrigger = Animator.StringToHash("Hide");

    private void Awake()
    {
        // El panel arranca oculto
        if (panelRoot != null)
            panelRoot.SetActive(false);

        // Conecta el botón por código como respaldo
        if (understoodButton != null)
            understoodButton.onClick.AddListener(OnUnderstoodClicked);
    }

    /// <summary>
    /// Llamado por TutorialTrigger. Muestra el panel con el contenido configurado.
    /// </summary>
    public void Show(Sprite image, string text)
    {
        // Asigna el contenido
        if (referenceImage != null)
        {
            referenceImage.sprite = image;
            referenceImage.gameObject.SetActive(image != null); // oculta si no hay imagen
        }

        if (descriptionText != null)
            descriptionText.text = text;

        // Activa el panel
        if (panelRoot != null)
            panelRoot.SetActive(true);

        // Dispara animación si existe
        if (panelAnimator != null)
            panelAnimator.SetTrigger(ShowTrigger);
    }

    /// <summary>
    /// Conecta este método al OnClick del botón "Entendido" desde el Inspector.
    /// </summary>
    public void OnUnderstoodClicked()
    {
        // Devuelve el control al jugador
        if (PlayerController.instance != null)
            PlayerController.instance.stopMoving = false;

        // Dispara animación de salida si existe, si no, cierra directo
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger(HideTrigger);
            // El Animator debe llamar HidePanel() como Animation Event al terminar
        }
        else
        {
            HidePanel();
        }
    }

    /// <summary>
    /// Desactiva el panel. Si usas animación, llámalo como Animation Event
    /// en el último frame de la animación de salida.
    /// </summary>
    public void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}