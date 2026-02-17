using UnityEngine;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TextMeshProUGUI totalCoinsText;

    void Start()
    {
        // Al iniciar el menú, leemos el bolsillo global guardado en el disco
        int total = GameData.GetGlobalPocket();
        if (totalCoinsText != null)
            totalCoinsText.text = "Total Monedas: " + total.ToString();
    }
}