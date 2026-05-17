using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnlockableSign : MonoBehaviour
{
    [Header("UI del letrero")]
    public GameObject signCanvas;        // el Canvas World Space
    public Image iconImage;              // imagen del icono
    public TextMeshProUGUI requiredText; // texto del requisito

    private UnlockableMechanic _mechanic;

    void Awake()
    {
        _mechanic = GetComponentInParent<UnlockableMechanic>();

        if (_mechanic == null)
        {
            Debug.LogWarning("[UnlockableSign] No se encontró UnlockableMechanic en el padre.");
            return;
        }

        // Configurar UI con los datos de la mecánica
        SetupUI();

        // Canvas empieza oculto
        if (signCanvas != null)
            signCanvas.SetActive(false);
    }

    private void SetupUI()
    {
        if (requiredText != null)
        {
            requiredText.text = _mechanic.filterByKitType
                ? $"{_mechanic.requiredHelpedCount} personas\n({_mechanic.requiredKitType})"
                : $"{_mechanic.requiredHelpedCount} personas";
        }

        // Si tienes un icono asignado en UnlockableMechanic lo muestra
        if (iconImage != null && _mechanic.signIcon != null)
            iconImage.sprite = _mechanic.signIcon;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_mechanic == null || !_mechanic.IsInteractable()) return;

        PlayerController.instance.currentUnlockable = _mechanic;

        if (signCanvas != null)
            signCanvas.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PlayerController.instance.currentUnlockable == _mechanic)
            PlayerController.instance.currentUnlockable = null;

        if (signCanvas != null)
            signCanvas.SetActive(false);
    }
}