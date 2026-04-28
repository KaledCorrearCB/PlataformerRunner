using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configuración de Moneda")]
    public int value = 1;          // Cuánto suma esta moneda
    public float rotationSpeed = 100f; // Velocidad de giro visual

    [Header("SFX")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float collectVolume = 0.7f;

    void Update()
    {
        // Movimiento visual: la moneda gira sobre su propio eje
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos si el objeto que entró en el Trigger tiene el Tag "Player"
        if (other.CompareTag("Player"))
        {
            // 2. Intentamos enviar el valor al SessionManager
            if (SessionManager.Instance != null)
            {
                SessionManager.Instance.AddCoin(value);
            }
            else
            {
                Debug.LogWarning("Ojo: No hay un SessionManager en la escena. Asegúrate de tener el objeto _Systems con el script.");
            }
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
            // 3. Destruimos la moneda para que no se pueda recoger dos veces
            Destroy(gameObject);
        }
    }
}