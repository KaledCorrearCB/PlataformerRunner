using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform _cam;

    void Start()
    {
        _cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (_cam == null) return;

        // Rota para mirar siempre hacia la cámara
        transform.rotation = Quaternion.LookRotation(transform.position - _cam.position);
    }
}