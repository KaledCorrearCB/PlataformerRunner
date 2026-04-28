using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Usamos el nombre exacto de tu archivo: LevelSelector
    public void OpenLevelSelector()
    {
        SceneManager.LoadScene("LevelSelector");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenRecordMenu()
    {
        SceneManager.LoadScene("RecordMenu");
    }

    public void CloseGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}