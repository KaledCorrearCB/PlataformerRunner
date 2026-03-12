// ShopItemButton.cs
// Adjúntalo al prefab de cada botón de ítem
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [Header("Referencias UI del Prefab")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    private ShopItemData itemData;

    public void Setup(ShopItemData item, bool alreadyPurchased)
    {
        itemData = item;

        if (iconImage != null && item.icon != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (descriptionText != null)
            descriptionText.text = item.description;

        if (alreadyPurchased)
        {
            // Mostrar como ya comprado
            buyButtonText.text = "✓ Obtenido";
            buyButton.interactable = false;
        }
        else
        {
            buyButtonText.text = "Obtener";
            buyButton.interactable = true;
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    private void OnBuyClicked()
    {
        ShopManager.instance.PurchaseItem(itemData);
    }
}