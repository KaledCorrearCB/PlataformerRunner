using UnityEngine;
using TMPro;

public class KitUIHandler : MonoBehaviour
{
    [Header("Textos de cada kit")]
    public TMP_Text firstAidText;
    public TMP_Text foodText;
    public TMP_Text clothingText;

    void Start()
    {
        UpdateKitUI();
    }

    public void UpdateKitUI()
    {
        if (KitInventory.Instance == null) return;

        firstAidText.text = "Kits de Primeros Auxilios: " + KitInventory.Instance.GetCount(KitType.FirstAid);
        foodText.text = "Kits de Comida: " + KitInventory.Instance.GetCount(KitType.Food);
        clothingText.text = "Kits de Ropa: " + KitInventory.Instance.GetCount(KitType.Clothing);
    }
}