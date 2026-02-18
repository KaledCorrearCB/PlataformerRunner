using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        // Esto hace que el texto siempre rote hacia la cámara principal
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}