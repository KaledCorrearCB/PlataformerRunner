using UnityEngine;
using UnityEngine.UI;

public class PlayerWater : MonoBehaviour
{
    [Header("Configuración de Agua")]
    public float currentWater = 0f;
    public float maxWater = 100f;
    public float waterPickupSpeed = 50f;

    [Header("UI")]
    public Slider waterBar;

    [Header("Detección")]
    public LayerMask waterSourceLayer;

    // Variables internas
    private bool isAbsorbingWater = false;
    private GameObject currentWaterSource;
    private PlayerController playerController;

    [Header("VFX Absorción")]
    public GameObject absorbVFX;
    private GameObject _absorbEffect;

    [Header("SFX Absorción")]
    public AudioClip absorbSound;
    [Range(0f, 1f)] public float absorbVolume = 0.5f;
    private AudioSource _audioSource;

    [Header("VFX Chorro de Agua")]
    public GameObject waterStreamVFX;
    public Transform streamSpawnPoint; // punto de spawn, ej: la mano del personaje
    private GameObject _streamEffect;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = true; // loop mientras absorbe
        _audioSource.volume = absorbVolume;

        // Configurar barra de agua
        if (waterBar != null)
        {
            waterBar.maxValue = maxWater;
            waterBar.minValue = 0;
            waterBar.value = currentWater;
        }
        else
        {
            Debug.LogError("⚠️ No has asignado el Water Bar en el Inspector");
        }
    }

    void Update()
    {
        // Absorber agua si está en fuente y presionando
        if (isAbsorbingWater && currentWaterSource != null)
        {
            AbsorbWater();
        }
    }

    void AbsorbWater()
    {
        // Aumentar agua
        currentWater += waterPickupSpeed * Time.deltaTime;
        currentWater = Mathf.Clamp(currentWater, 0, maxWater);

        // ACTUALIZAR BARRA (¡LA LÍNEA MÁGICA!)
        if (waterBar != null)
            waterBar.value = currentWater;

        // Si se llenó, dejar de absorber automáticamente
        if (currentWater >= maxWater)
        {
            StopAbsorbingWater();
        }
    }

    public void StartAbsorbingWater(GameObject waterSource)
    {
        if (absorbVFX != null)
        {
            _absorbEffect = Instantiate(absorbVFX, transform.position, Quaternion.identity);
            _absorbEffect.transform.SetParent(transform); // sigue al jugador
        }

        if (currentWater < maxWater)
        {
            isAbsorbingWater = true;
            currentWaterSource = waterSource;
        }

        if (absorbSound != null)
        {
            _audioSource.clip = absorbSound;
            _audioSource.Play();
        }
    }

    public void StopAbsorbingWater()
    {
        if (_absorbEffect != null)
        {
            Destroy(_absorbEffect);
            _absorbEffect = null;
        }

        isAbsorbingWater = false;
        currentWaterSource = null;

        _audioSource.Stop();
    }

    // Método para USAR agua (regar plantas)
    public bool UseWater(float amount)
    {
        if (currentWater <= 0f) return false;

        // Usa lo que haya disponible aunque sea menos de lo pedido
        float actualAmount = Mathf.Min(amount, currentWater);
        currentWater -= actualAmount;
        currentWater = Mathf.Max(currentWater, 0f); // nunca negativo

        if (waterBar != null)
            waterBar.value = currentWater;

        return actualAmount > 0f;
    }

    // Método de ayuda para verificar sin gastar
    public bool HasEnoughWater(float amount)
    {
        return currentWater >= amount;
    }

    // Esto es útil para pruebas en el Inspector
    void OnValidate()
    {
        if (waterBar != null && Application.isPlaying)
        {
            waterBar.value = currentWater;
        }
    }

    public void StartWateringStream()
    {
        if (waterStreamVFX != null && _streamEffect == null)
        {
            Transform spawnPoint = streamSpawnPoint != null ? streamSpawnPoint : transform;
            _streamEffect = Instantiate(waterStreamVFX, spawnPoint.position, spawnPoint.rotation);
            _streamEffect.transform.SetParent(spawnPoint);
        }
    }

    public void StopWateringStream()
    {
        if (_streamEffect != null)
        {
            Destroy(_streamEffect);
            _streamEffect = null;
        }
    }

}