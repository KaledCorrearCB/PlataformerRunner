using UnityEngine;

public class ControladorLiana : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform[] nodos; // Aquí arrastraremos las esferas

    void Update()
    {
        // Actualiza la posición de cada punto de la línea en cada frame
        for (int i = 0; i < nodos.Length; i++)
        {
            lineRenderer.SetPosition(i, nodos[i].position);
        }
    }
}