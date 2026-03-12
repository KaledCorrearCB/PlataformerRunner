// ShopUI.cs
// Adjúntalo al mismo panel de la tienda
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform itemContainer;        // El layout group donde van los ítems
    public GameObject itemButtonPrefab;    // Prefab de cada botón de ítem

    public void RefreshUI(List<ShopItemData> items, HashSet<string> purchased)
    {
        // Limpiar ítems anteriores
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        // Crear uno por cada ítem
        foreach (var item in items)
        {
            var buttonGO = Instantiate(itemButtonPrefab, itemContainer);
            var buttonUI = buttonGO.GetComponent<ShopItemButton>();

            bool alreadyBought = purchased.Contains(item.itemName);
            buttonUI.Setup(item, alreadyBought);
        }
    }
}