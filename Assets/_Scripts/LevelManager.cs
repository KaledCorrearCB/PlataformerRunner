using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public float waitBeforeRespawning;
    [HideInInspector]public bool respawning;
    private PlayerController player;
    public Vector3 respawnPoint;

    private CameraController cam;

    public void Awake()
    {
        instance = this;

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();  
        respawnPoint = player.transform.position + Vector3.up;
        cam = FindFirstObjectByType<CameraController>();
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
}
