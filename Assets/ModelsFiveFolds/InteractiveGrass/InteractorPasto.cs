using UnityEngine;

public class InteractorPasto : MonoBehaviour
{
    // Esta variable global es la que el Shader Graph leerá
    private static readonly int PosicionJugadorID = Shader.PropertyToID("_PosicionJugador");

    void Update()
    {
        // Envía la posición del objeto que tiene este script a la GPU
        Shader.SetGlobalVector(PosicionJugadorID, transform.position);
    }
}