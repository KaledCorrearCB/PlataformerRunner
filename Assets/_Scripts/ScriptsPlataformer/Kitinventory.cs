// KitInventory.cs
// Lleva la cuenta de cuántos kits tiene el jugador EN MANO en este momento.
// Es diferente a KitSessionManager: ese cuenta los recogidos en total,
// este cuenta los disponibles para entregar.
//
// SETUP: Agrega este script al GameObject del Player.

using UnityEngine;

public class KitInventory : MonoBehaviour
{
    public static KitInventory Instance;

    private System.Collections.Generic.Dictionary<KitType, int> _kitsInHand
        = new System.Collections.Generic.Dictionary<KitType, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (KitType type in System.Enum.GetValues(typeof(KitType)))
            _kitsInHand[type] = 0;
    }

    /// <summary>Agrega un kit al inventario del jugador al recogerlo del suelo.</summary>
    public void AddKit(KitType type, int amount = 1)
    {
        _kitsInHand[type] += amount;
        Debug.Log($"[Inventario] {type} en mano: {_kitsInHand[type]}");
        Object.FindFirstObjectByType<KitUIHandler>()?.UpdateKitUI();
    }

    /// <summary>Intenta gastar un kit. Devuelve true si había al menos uno.</summary>
    public bool SpendKit(KitType type)
    {
        if (_kitsInHand.TryGetValue(type, out int count) && count > 0)
        {
            _kitsInHand[type]--;
            Debug.Log($"[Inventario] {type} gastado. Quedan: {_kitsInHand[type]}");
            Object.FindFirstObjectByType<KitUIHandler>()?.UpdateKitUI();
            return true;
        }
        Debug.Log($"[Inventario] No hay {type} disponibles.");
        return false;
    }

    /// <summary>Consulta cuántos kits de un tipo hay en mano.</summary>
    public int GetCount(KitType type)
    {
        return _kitsInHand.TryGetValue(type, out int count) ? count : 0;
    }
}