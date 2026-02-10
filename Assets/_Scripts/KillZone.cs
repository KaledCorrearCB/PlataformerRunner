using Unity.VisualScripting;
using UnityEngine;

public class KillZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if ( other.tag == "Player")
        {
            //other.gameObject.GetComponent<CharacterController>().Move(Vector3.up - other.transform.position);
            LevelManager.instance.Respawn();
        }
    }

}
