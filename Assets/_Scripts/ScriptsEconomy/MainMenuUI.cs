using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el mando
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;

    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelRecords;
    public GameObject panelShop;

    void Start()
    {
        // Tu lógica de monedas que ya tenías
        int total = GameData.GetGlobalPocket();
        if (totalCoinsText != null)
            totalCoinsText.text = "Total Monedas: " + total.ToString();
    }

    // ESTA ES LA FUNCIÓN PARA EL MANDO (Botón Círculo / Back)
    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Si el panel de Records está abierto, lo cierra y vuelve al principal
            if (panelRecords != null && panelRecords.activeSelf)
            {
                panelRecords.SetActive(false);
                panelPrincipal.SetActive(true);
            }
            // Si el de la tienda está abierto, lo mismo
            else if (panelShop != null && panelShop.activeSelf)
            {
                panelShop.SetActive(false);
                panelPrincipal.SetActive(true);
            }
        }
    }
}