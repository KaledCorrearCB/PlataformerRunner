using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    //Aqui hacemos que se vuelva un singletone
    public static LevelManager instance;
    public float waitBeforeRespawning;

    [HideInInspector]public bool respawning;
    private PlayerController player;
    public Vector3 respawnPoint;
    public float waitBeforeSceneLoad;
    private KillZone killZone;


    private CameraController cam;

    public void Awake()
    {
        instance = this;

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Traemos la informacion del objeto player
        player = FindFirstObjectByType<PlayerController>();  
        //traemos la posicion del respawn point
        respawnPoint = player.transform.position + Vector3.up;
        // Traemos la informaicon de la camara
        cam = FindFirstObjectByType<CameraController>();

        //traemos la informacion de la killzone
        killZone = FindFirstObjectByType<KillZone>();

        if (killZone != null)
        {
            killZone.SetPlayer(player.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Respawn()
    {
        if (!respawning)
        {
            respawning = true;

            StartCoroutine(RespawnCo());
        }
    }


    public IEnumerator RespawnCo()
    {
        player.gameObject.SetActive(false);
        UIController.instance.FadeToBlack();
        yield return new WaitForSeconds(waitBeforeRespawning);

        

        player.transform.position = respawnPoint;
        cam.SnapToTarget();
        player.gameObject.SetActive(true);
        UIController.instance.FadeFromBlack();
        respawning = false;
    }

    public void CompleteLevel(string sceneToLoad)
    {
        StartCoroutine(CompleteLevelCo(sceneToLoad));
    }

    private IEnumerator CompleteLevelCo(string sceneToLoad)
    {
        respawning = true;

        player.gameObject.SetActive(false);

        UIController.instance.FadeToBlack();

        // Esperar hasta que el alpha sea 1
        while (UIController.instance.fadeScreem.color.a < 0.95f)
        {
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
