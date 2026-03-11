// Kit.cs
// Objeto recogible genérico para los tres tipos de kits.
// Funciona igual que Coin.cs: detecta al Player por Tag y se destruye al ser recogido.
//
// SETUP EN UNITY:
//   1. Crea un GameObject para cada tipo de kit (o un Prefab base y tres variantes).
//   2. Agrega un Collider con "Is Trigger" activado.
//   3. Agrega este script y elige el KitType desde el Inspector.
//   4. (Opcional) Asigna un AudioClip y/o ParticleSystem para feedback visual/sonoro.

using UnityEngine;

public class Kit : MonoBehaviour
{
    [Header("Tipo de Kit")]
    public KitType kitType = KitType.FirstAid;  // Seleccionable desde el Inspector

    [Header("Cantidad")]
    public int amount = 1;  // Cuántas unidades da este kit al recogerlo

    [Header("Visual (opcional)")]
    public float rotationSpeed = 60f;           // Giro visual igual que las monedas
    public float bobAmplitude = 0.15f;          // Amplitud del flotado vertical
    public float bobSpeed = 2f;                 // Velocidad del flotado

    [Header("Feedback (opcional)")]
    public AudioClip collectSound;              // Sonido al recoger
    public GameObject collectParticles;         // Partículas al recoger

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        // Giro visual
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Flotado suave arriba/abajo
        float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 1. Suma al contador de sesión (para estadísticas)
        if (KitSessionManager.Instance != null)
            KitSessionManager.Instance.AddKit(kitType, amount);
        else
            Debug.LogWarning("[Kit] No hay KitSessionManager en la escena.");

        // 2. Suma al inventario del jugador (para poder gastarlo con los personajes)
        if (KitInventory.Instance != null)
            KitInventory.Instance.AddKit(kitType, amount);
        else
            Debug.LogWarning("[Kit] No hay KitInventory en la escena. Agrégalo al Player.");

        // Feedback sonoro
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Feedback de partículas
        if (collectParticles != null)
            Instantiate(collectParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // ─── Gizmo para ver el tipo de kit en el editor ───────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            $"[{kitType}]"
        );
    }
#endif
}