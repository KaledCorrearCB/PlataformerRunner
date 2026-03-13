// ShopItemData.cs
// Crea uno por cada producto: Click derecho en Project > Create > Shop > Item
using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Info del Producto")]
    public string itemName = "Ítem Exclusivo";
    public string description = "Descripción del contenido exclusivo";
    public Sprite icon;

    [Header("Contenido Exclusivo")]
    public GameObject exclusiveContentPrefab; // Lo que se activa al comprar
    public string unlockMessage = "¡Desbloqueaste el contenido exclusivo!";

    [Header("Mensaje de Impacto")]
    [TextArea(2, 4)]
    public string impactMessage = "Con las donaciones totales logradas por ti y el resto de la comunidad se salvaron más de 120 familias. 💚";
    public float impactMessageDelay = 60f; // Segundos antes de mostrar el mensaje
}