// PetUnlocker.cs
// Adjúntalo al GameObject del perrito en la escena del juego
using UnityEngine;

public class PetUnlocker : MonoBehaviour
{
    [Header("Debe coincidir exactamente con el itemName del ShopItemData")]
    public string itemName = "Ítem Exclusivo"; // ← pon el nombre exacto de tu ítem

    void Start()
    {
        // Si fue comprado, activarse — si no, permanecer oculto
        bool wasPurchased = PlayerPrefs.GetInt(itemName, 0) == 1;
        gameObject.SetActive(wasPurchased);
    }
}