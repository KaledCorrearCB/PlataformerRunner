// IKitInteractable.cs
// Interfaz que cualquier objeto puede implementar para reaccionar cuando
// el jugador recoge un kit. Ejemplos futuros: puertas que se abren con
// ropa, NPCs que reaccionan a comida, checkpoints que se activan con
// primeros auxilios, etc.

public interface IKitInteractable
{
    /// <summary>
    /// Se llama automáticamente cuando el jugador recoge un kit.
    /// </summary>
    /// <param name="type">El tipo de kit recogido.</param>
    /// <param name="amount">La cantidad recogida (por defecto 1).</param>
    void OnKitCollected(KitType type, int amount);
}