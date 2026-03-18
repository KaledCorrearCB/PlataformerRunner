using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI totalCoinsText;

    [Header("Panels")]
    public GameObject panelPrincipal;
    public GameObject panelRecords;
    public GameObject panelShop;

    void Start()
    {
        // Forzamos que el mouse sea visible al entrar al menú
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ActualizarMonedas();

        // Configuramos el estado inicial: Menú principal encendido, los demás apagados
        VolverAlMenu();
    }

    public void ActualizarMonedas()
    {
        int total = GameData.GetGlobalPocket();
        if (totalCoinsText != null)
            totalCoinsText.text = "Total Monedas: " + total.ToString();
    }

    // Métodos públicos para los botones del Inspector
    public void AbrirRecords() { SetPanels(false, true, false); }
    public void AbrirTienda() { SetPanels(false, false, true); }
    public void VolverAlMenu() { SetPanels(true, false, false); }

    private void SetPanels(bool p, bool r, bool s)
    {
        // Seguridad: Solo desactivamos si el objeto existe
        if (panelPrincipal != null) panelPrincipal.SetActive(p);
        if (panelRecords != null) panelRecords.SetActive(r);
        if (panelShop != null) panelShop.SetActive(s);

        // Debug para saber qué está pasando en la consola si algo desaparece
        Debug.Log($"Paneles actualizados: Principal({p}), Records({r}), Shop({s})");
    }

    public void Jugar(string nombreNivel)
    {
        SceneManager.LoadScene(nombreNivel);
    }

    // BOTÓN CÍRCULO / ESC: Regresar
    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Solo volvemos al menú si NO estamos ya en el principal
            if ((panelRecords != null && panelRecords.activeSelf) ||
                (panelShop != null && panelShop.activeSelf))
            {
                VolverAlMenu();
            }
        }
    }
}