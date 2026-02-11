using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (LevelManager.instance.respawnPoint != transform.position)
            {
                LevelManager.instance.respawnPoint = transform.position;

                CheckPoint[] allCheckPo =
                    Object.FindObjectsByType<CheckPoint>(FindObjectsSortMode.None);

                foreach (CheckPoint CheckPo in allCheckPo)
                {
                    CheckPo.enabled = false;
                }

                enabled = true;
            }
        }
    }
}
