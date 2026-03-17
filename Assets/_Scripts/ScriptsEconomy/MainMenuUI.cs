using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;
    public GameObject panelPrincipal;
    public GameObject panelRecords;
    public GameObject panelShop;

    void Start()
    {
        ActualizarMonedas();
        VolverAlMenu(); // Asegura que todo esté en su sitio al arrancar
    }

    public void ActualizarMonedas()
    {
        int total = GameData.GetGlobalPocket();
        if (totalCoinsText != null) totalCoinsText.text = "Total Monedas: " + total.ToString();
    }

    public void AbrirRecords() { SetPanels(false, true, false); }
    public void AbrirTienda() { SetPanels(false, false, true); }
    public void VolverAlMenu() { SetPanels(true, false, false); }

    private void SetPanels(bool p, bool r, bool s)
    {
        if (panelPrincipal) panelPrincipal.SetActive(p);
        if (panelRecords) panelRecords.SetActive(r);
        if (panelShop) panelShop.SetActive(s);
    }

    public void Jugar(string nombreNivel) { SceneManager.LoadScene(nombreNivel); }

    // BOTÓN CÍRCULO: Solo funciona si NO estamos en el menú principal
    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (panelRecords.activeSelf || panelShop.activeSelf)
            {
                VolverAlMenu();
            }
        }
    }
}