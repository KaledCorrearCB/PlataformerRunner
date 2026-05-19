using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject pauseMenuPanel;   // el Panel con los dos botones
    public string mainMenuSceneName = "MainMenu"; // nombre exacto de tu escena

    private bool isPaused = false;

    // ─── Llamado por el botón de pausa en pantalla ───
    public void TogglePause()
    {
        isPaused = !isPaused;

        pauseMenuPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        // Bloquear movimiento del jugador mientras está pausado
        if (PlayerController.instance != null)
            PlayerController.instance.stopMoving = isPaused;
    }

    // ─── Botón "Seguir jugando" ───
    public void ContinueGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;

        if (PlayerController.instance != null)
            PlayerController.instance.stopMoving = false;
    }

    // ─── Botón "Volver al menú" ───
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // IMPORTANTE: resetear antes de cambiar escena
        SceneManager.LoadScene(mainMenuSceneName);
    }
}