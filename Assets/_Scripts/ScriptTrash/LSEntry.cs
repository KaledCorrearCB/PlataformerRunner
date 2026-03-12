using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LSEntry : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    public string levelName;
    [Tooltip("Si se marca, el nivel carga apenas el jugador pise el círculo")]
    public bool autoLoad = false;

    [Header("Visuales")]
    public GameObject mapPointActive;
    public GameObject mapPointInactive;

    private bool playerInside;

    void OnTriggerEnter(Collider other)
    {
        // 1. Verifica que el objeto tenga el Tag "Player"
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            if (mapPointActive != null) mapPointActive.SetActive(true);
            if (mapPointInactive != null) mapPointInactive.SetActive(false);

            // 2. Registra este nodo en el script del jugador
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.currentLevelNode = this;

                // 3. Si es carga automática, dispara la corrutina de una vez
                if (autoLoad)
                {
                    LoadLevel();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            if (mapPointActive != null) mapPointActive.SetActive(false);
            if (mapPointInactive != null) mapPointInactive.SetActive(true);

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.currentLevelNode == this)
            {
                player.currentLevelNode = null;
            }
        }
    }

    public void LoadLevel()
    {
        // Evita cargar si el jugador salió o si ya está cargando
        if (!playerInside) return;
        StartCoroutine(LoadLevelCo());
    }

    IEnumerator LoadLevelCo()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.stopMoving = true;

        // Verifica que el UIController exista para evitar errores en consola
        if (UIController.instance != null)
        {
            UIController.instance.FadeToBlack();
        }

        yield return new WaitForSeconds(1.5f); // Reducido un poco para mejor feeling

        if (!string.IsNullOrEmpty(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError("Falta el nombre del nivel en el Inspector de: " + gameObject.name);
        }
    }
}