using UnityEngine;

public class LevelSelectorCamera : MonoBehaviour
{
    public Transform target; // Arrastra aquí al MapPlayer
    public Vector3 offset = new Vector3(0, 45, -15); // Distancia cámara-personaje
    public float smoothTime = 0.2f; // Suavizado del movimiento

    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target != null)
        {
            // Calculamos la posición deseada
            Vector3 targetPosition = target.position + offset;

            // Movemos la cámara suavemente
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }
    }
}