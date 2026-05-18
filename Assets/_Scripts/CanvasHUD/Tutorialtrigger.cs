using UnityEngine;

/// <summary>
/// Colócalo en un GameObject vacío con un Collider en modo "Is Trigger".
/// Cuando el jugador entra, muestra el panel de tutorial y bloquea el movimiento.
/// Se destruye a sí mismo después de activarse (una sola vez por trigger).
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    [Header("Referencia al Panel")]
    [Tooltip("Arrastra aquí el prefab TutorialPanel que está en la escena (ya instanciado en el Canvas).")]
    public TutorialPanel tutorialPanel;

    [Header("Contenido del Tutorial")]
    [Tooltip("Imagen que se muestra arriba del panel (ej: ícono del control).")]
    public Sprite tutorialImage;

    [Tooltip("Texto explicativo que verá el jugador.")]
    [TextArea(3, 6)]
    public string tutorialText = "Escribe aquí la explicación de este control.";

    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Solo reacciona al jugador y solo una vez
        if (alreadyTriggered) return;
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = true;

        // Busca el panel en escena si no fue asignado manualmente
        if (tutorialPanel == null)
            tutorialPanel = FindFirstObjectByType<TutorialPanel>();

        if (tutorialPanel == null)
        {
            Debug.LogError("[TutorialTrigger] No se encontró ningún TutorialPanel en la escena.");
            return;
        }

        // Bloquea al jugador y muestra el panel
        PlayerController.instance.stopMoving = true;
        tutorialPanel.Show(tutorialImage, tutorialText);

        // Desactiva el collider para que no vuelva a dispararse
        GetComponent<Collider>().enabled = false;
    }

    // Dibuja el trigger en el editor para que sea fácil de posicionar
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.8f, 0.25f);
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 1f, 0.8f, 0.9f);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = old;
        }
    }
}