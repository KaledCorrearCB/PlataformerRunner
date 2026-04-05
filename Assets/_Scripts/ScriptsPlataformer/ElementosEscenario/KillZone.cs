using UnityEngine;

public class KillZone : MonoBehaviour
{
    private Transform player;

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        transform.position = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.instance.Respawn();
        }
    }
}