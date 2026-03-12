// ShopManager.cs
// Adjúntalo a un GameObject vacío "ShopManager" en la escena
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("Ítems disponibles en la tienda")]
    public List<ShopItemData> availableItems;

    [Header("Referencias UI")]
    public GameObject shopPanel;           // Panel principal de la tienda
    public ShopUI shopUI;                  // Componente que dibuja la UI

    [Header("Notificación de Impacto")]
    public ImpactNotification impactNotification;


    public GameObject mainCanvas;

    // Guarda qué ítems ya compró el jugador (por nombre)
    private HashSet<string> purchasedItems = new HashSet<string>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (mainCanvas != null)
            mainCanvas.SetActive(false); 

        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (shopUI != null)
            shopUI.RefreshUI(availableItems, purchasedItems);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (mainCanvas != null)
            mainCanvas.SetActive(true); 
    
}

    public void PurchaseItem(ShopItemData item)
    {
        if (purchasedItems.Contains(item.itemName))
        {
            Debug.Log($"Ya compraste: {item.itemName}");
            return;
        }

        // Registrar compra
        purchasedItems.Add(item.itemName);
        Debug.Log($"✅ Comprado: {item.itemName}");

        // Activar contenido exclusivo
        if (item.exclusiveContentPrefab != null)
            Instantiate(item.exclusiveContentPrefab);

        // Refrescar UI para mostrar "Ya comprado"
        if (shopUI != null)
            shopUI.RefreshUI(availableItems, purchasedItems);

        // Iniciar cuenta regresiva para el mensaje de impacto
        StartCoroutine(ShowImpactMessageAfterDelay(item));
    }

    public bool IsPurchased(string itemName)
    {
        return purchasedItems.Contains(itemName);
    }

    private IEnumerator ShowImpactMessageAfterDelay(ShopItemData item)
    {
        Debug.Log($"⏳ Mensaje de impacto llegará en {item.impactMessageDelay} segundos...");
        yield return new WaitForSeconds(item.impactMessageDelay);

        if (impactNotification != null)
            impactNotification.ShowMessage(item.impactMessage);
    }
}