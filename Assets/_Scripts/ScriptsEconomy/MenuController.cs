using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        // Carga la escena del Runner (que es la número 1 en el Build Profile)
        SceneManager.LoadScene(1);
    }
}