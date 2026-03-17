using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("Configuración de Victoria")]
    [Tooltip("IMPORTANTE: Este ID debe coincidir con el ID del Portal en el Level Selector")]
    public int portalID = 1;

    [SerializeField] private string levelSelectorScene = "LevelSelector";

    private bool metaActivada = false; // Evita que se dispare varias veces

    private void OnTriggerEnter(Collider other)
    {
        // Solo el jugador y solo una vez
        if (other.CompareTag("Player") && !metaActivada)
        {
            metaActivada = true;
            RegistrarVictoria();
        }
    }

    private void RegistrarVictoria()
    {
        // 1. Guardamos el estado '1' (Volviendo con victoria) para el portal específico
        string llaveVictoria = "NivelCompletado_" + portalID;
        PlayerPrefs.SetInt(llaveVictoria, 1);
        PlayerPrefs.Save();

        Debug.Log("<color=green>Victoria registrada:</color> " + llaveVictoria);

        // 2. Transición de Escena
        if (LevelManager.instance != null)
        {
            // Usamos tu LevelManager si existe
            LevelManager.instance.CompleteLevel(levelSelectorScene);
        }
        else
        {
            // Fallback directo si no hay manager
            SceneManager.LoadScene(levelSelectorScene);
        }
    }
}