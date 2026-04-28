using UnityEngine;

public class WaterSource : MonoBehaviour
{
    [Header("Configuración")]
    public bool isInfinite = true;        // Si la fuente nunca se agota
    public float waterAmount = 100f;      // Si es finita, cuánta agua tiene
    public float absorptionMultiplier = 1f; // Velocidad extra (opcional)

    [Header("Visuales")]
    public GameObject waterVisual;        // Para animar cuando absorben
    public ParticleSystem absorptionParticles;

    [Header("UI")]
    public GameObject interactCanvas;      // Canvas con "Presiona E"

    // Referencias internas
    private bool playerInRange = false;

    private PlayerController playerInZone;

    void Awake()
    {
        Debug.Log($"[WaterSource] ParticleSystem encontrado: {absorptionParticles}");

        if (absorptionParticles == null)
            absorptionParticles = GetComponentInChildren<ParticleSystem>();
    }

    void Start()
    {


        if (interactCanvas != null)
            interactCanvas.SetActive(false);

        if (absorptionParticles != null)
            absorptionParticles.Stop();
    }

    // Llamado desde PlayerController cuando el jugador empieza a absorber
    public void StartAbsorbing(PlayerController player)
    {
        if (!isInfinite && waterAmount <= 0) return;

        if (absorptionParticles != null)
        {
            // 🔍 Revisar toda la cadena de padres
            Transform t = absorptionParticles.transform;
            while (t != null)
            {
                Debug.Log($"[WaterSource] Objeto: {t.name} | activeSelf={t.gameObject.activeSelf} | activeInHierarchy={t.gameObject.activeInHierarchy}");
                t = t.parent;
            }

            absorptionParticles.gameObject.SetActive(true);
            absorptionParticles.Play(true);
        }
    }

    // Llamado desde PlayerController cuando el jugador deja de absorber
    public void StopAbsorbing(PlayerController player)
    {
        Debug.Log("Dejó de absorber agua");

        if (absorptionParticles != null)
            absorptionParticles.Stop(true); // ✅ detiene hijos también
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInZone = other.GetComponent<PlayerController>();

            if (playerInZone != null)
            {
                playerInZone.currentWaterSource = this;

                if (interactCanvas != null)
                    interactCanvas.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (playerInZone != null)
            {
                // Si el jugador se va, aseguramos que deje de absorber
                if (playerInZone.currentWaterSource == this)
                {
                    playerInZone.currentWaterSource = null;
                    StopAbsorbing(playerInZone);
                }

                playerInZone = null;
            }

            if (interactCanvas != null)
                interactCanvas.SetActive(false);
        }
    }
}