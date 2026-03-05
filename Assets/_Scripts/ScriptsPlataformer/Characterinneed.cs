// CharacterInNeed.cs  ← REEMPLAZA tu versión anterior
// Ahora se integra al sistema de PlayerController igual que FlowerPot y WaterSource.
// Ya NO usa Input.GetKeyDown — la tecla E la maneja PlayerController.OnSelect().
//
// SETUP EN UNITY:
//   1. Selecciona el Empty "Personaje 1" (o 2 o 3).
//   2. Add Component → CharacterInNeed.
//   3. Elige el KitType que ese personaje necesita.
//   4. Asegúrate de que el hijo Cilindro tiene un Collider con Is Trigger ✓.
//   5. Repite para Personaje 2 y 3.

using UnityEngine;

public class CharacterInNeed : MonoBehaviour
{
    [Header("Configuración del Personaje")]
    public KitType requiredKit;
    public string characterName = "Persona";

    private bool _alreadyHelped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_alreadyHelped) return;
        if (!other.CompareTag("Player")) return;

        // Le avisamos al PlayerController que hay un personaje cerca,
        // igual que hace currentFlowerPot o currentWaterSource
        PlayerController.instance.currentCharacterInNeed = this;

        Debug.Log($"[{characterName}] Jugador cerca. Necesita: {requiredKit}. Presiona E.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Limpiamos la referencia al salir
        if (PlayerController.instance.currentCharacterInNeed == this)
            PlayerController.instance.currentCharacterInNeed = null;
    }

    /// <summary>
    /// Llamado desde PlayerController.OnSelect() cuando el jugador presiona E.
    /// </summary>
    public void TryDeliverKit()
    {
        if (_alreadyHelped) return;

        if (KitInventory.Instance == null)
        {
            Debug.LogWarning("[CharacterInNeed] No hay KitInventory en la escena.");
            return;
        }

        bool success = KitInventory.Instance.SpendKit(requiredKit);

        if (success)
        {
            _alreadyHelped = true;

            // Limpiar referencia en el PlayerController
            if (PlayerController.instance.currentCharacterInNeed == this)
                PlayerController.instance.currentCharacterInNeed = null;

            // Registrar en la base de datos
            HelpedCharactersData.RegisterHelped(requiredKit);

            Debug.Log($"[{characterName}] ¡Ayudado con {requiredKit}! " +
                      $"Total ayudados: {HelpedCharactersData.GetTotalHelped()}");

            // Opcional — descomenta la que prefieras:
            // gameObject.SetActive(false);                            // Desaparece todo
            // transform.GetChild(0).gameObject.SetActive(false);     // Solo desaparece el cilindro
        }
        else
        {
            Debug.Log($"[{characterName}] El jugador no tiene {requiredKit} disponible.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}