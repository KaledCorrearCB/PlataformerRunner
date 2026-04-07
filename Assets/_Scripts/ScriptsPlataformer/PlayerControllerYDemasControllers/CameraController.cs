using UnityEngine;

public class CameraController : MonoBehaviour
{
    //variable para asignar al jugador como objetivo a seguir.
    public Transform target;         

    //Posicion de la camara y velocidad
    public Vector3 offset = new Vector3(0f, 6f, -10f);
    public float followSpeed = 5f;
    public float lookSpeed = 10f;
    

    // Las funciones en lateUpdate funcionan al finalizar un fotograma luego de que todo lo ocurrido en los "update" se termine
    //Esto es perfecto para cualquier situacion donde se "siga" un objeto en especifico, como una camara al jugador.
    void LateUpdate()
    {
        //Para evitar que el juego estalle, si no hay jugador aun generado, simplemente no se devuelve nada.
        if (!target) return;

        // Posición deseada de la cámara
        Vector3 desiredPosition = target.position + offset;

        // Movimiento suave
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Rotar la cámara para mirar al jugador
        Vector3 lookDir = target.position - desiredPosition;
        Quaternion targetRotation = Quaternion.LookRotation(lookDir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            lookSpeed * Time.deltaTime
        );

        if (transform.position.y <offset.y)
        {
            transform.position = new Vector3(transform.position.x,offset.y, transform.position.z);
        }
    }

    public void SnapToTarget()
    {
        transform.position = target.position + offset;
    }    
}
