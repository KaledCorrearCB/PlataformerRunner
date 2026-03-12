using UnityEngine;
using TMPro;

public class ActualizadorMonedasHUD : MonoBehaviour
{
    public TextMeshProUGUI textoMonedas;

    void Update()
    {
        // Leemos el valor que usan tus compañeros en el PDF (TotalCoins)
        int total = PlayerPrefs.GetInt("TotalCoins", 0);
        textoMonedas.text = "Monedas: " + total.ToString();
    }
}