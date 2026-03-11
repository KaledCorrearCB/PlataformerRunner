// KitSessionManager.cs
// Espejo exacto de SessionManager pero para los tres tipos de kits.
// Guarda cuántos kits de cada tipo se recogieron en la sesión actual,
// y al finalizar los acumula en GameData (PlayerPrefs).
//
// USO: Coloca este script en el mismo GameObject que SessionManager (_Systems).

using System.Collections.Generic;
using UnityEngine;

public class KitSessionManager : MonoBehaviour
{
    public static KitSessionManager Instance;

    // Contador por tipo para la sesión actual
    private Dictionary<KitType, int> _sessionKits = new Dictionary<KitType, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Inicializa el contador en 0 para cada tipo
        foreach (KitType type in System.Enum.GetValues(typeof(KitType)))
        {
            _sessionKits[type] = 0;
        }
    }

    /// <summary>
    /// Registra la recolección de uno o más kits del tipo indicado.
    /// También notifica a todos los IKitInteractable de la escena.
    /// </summary>
    public void AddKit(KitType type, int amount = 1)
    {
        _sessionKits[type] += amount;

        Debug.Log($"[Kits] {type} recogido. Total en sesión: {_sessionKits[type]}");
        Object.FindFirstObjectByType<KitUIHandler>()?.UpdateKitUI();

        // Notifica a todos los objetos de la escena que implementen IKitInteractable
        // Así cualquier objeto futuro puede reaccionar sin modificar este script.
        var interactables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mono in interactables)
        {
            if (mono is IKitInteractable interactable)
            {
                interactable.OnKitCollected(type, amount);
            }
        }
    }

    /// <summary>
    /// Devuelve cuántos kits de un tipo se recogieron en esta sesión.
    /// </summary>
    public int GetSessionCount(KitType type)
    {
        return _sessionKits.TryGetValue(type, out int count) ? count : 0;
    }

    /// <summary>
    /// Llama esto junto con SessionManager.FinalizeRun() al morir o terminar nivel.
    /// Acumula los kits de sesión al total global guardado en disco.
    /// </summary>
    public void FinalizeRun()
    {
        foreach (KitType type in System.Enum.GetValues(typeof(KitType)))
        {
            if (_sessionKits[type] > 0)
            {
                GameData.AddKitsToGlobalPocket(type, _sessionKits[type]);
                Debug.Log($"[Kits] Guardado en disco — {type}: {_sessionKits[type]}");
            }
        }
    }
}