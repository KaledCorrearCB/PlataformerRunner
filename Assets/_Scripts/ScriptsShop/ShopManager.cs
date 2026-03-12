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
        if (purchasedItems.Contains(item.itemName)) return;

        purchasedItems.Add(item.itemName);
        PlayerPrefs.SetInt(item.itemName, 1);
        PlayerPrefs.Save();

        // --- EL FIX: Buscar el objeto en la escena ---
        // Buscamos a "Athos" (o el nombre que tenga el ítem) entre los objetos desactivados
        GameObject[] todosLosObjetos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in todosLosObjetos)
        {
            // Si el nombre del objeto en la escena coincide con el itemName del producto
            if (go.name == item.itemName)
            {
                go.SetActive(true);
                Debug.Log($"¡{go.name} activado en la escena!");
            }
        }

        if (shopUI != null) shopUI.RefreshUI(availableItems, purchasedItems);
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