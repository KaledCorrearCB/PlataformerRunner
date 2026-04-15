using UnityEngine;
using System.Collections;

public class TrampolineAnimation : MonoBehaviour
{
    // Arrastra el "Grupo_Movil" a este espacio en el inspector
    public Transform grupoMovil;

    // Configuración de la animación
    private Vector3 escalaOriginal = new Vector3(1, 1, 1);
    private Vector3 escalaComprimida = new Vector3(1, 0.5f, 1);
    private float velocidad = 0.1f;

    public void DispararAnimacion()
    {
        StartCoroutine(EfectoResorte());
    }

    IEnumerator EfectoResorte()
    {
        // Comprimir
        grupoMovil.localScale = escalaComprimida;
        yield return new WaitForSeconds(velocidad);

        // Regresar a original
        grupoMovil.localScale = escalaOriginal;
    }
}