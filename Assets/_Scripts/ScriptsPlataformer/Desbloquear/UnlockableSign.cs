using UnityEngine;

public class UnlockableSign : MonoBehaviour
{
    // Se autodetecta en el padre, no hace falta asignar nada
    private UnlockableMechanic _mechanic;

    void Awake()
    {
        _mechanic = GetComponentInParent<UnlockableMechanic>();

        if (_mechanic == null)
            Debug.LogWarning("[UnlockableSign] No se encontró UnlockableMechanic en el padre.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_mechanic == null || !_mechanic.IsInteractable()) return;

        PlayerController.instance.currentUnlockable = _mechanic;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerController.instance.currentUnlockable == _mechanic)
            PlayerController.instance.currentUnlockable = null;
    }
}