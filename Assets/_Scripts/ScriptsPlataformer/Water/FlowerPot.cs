using UnityEngine;
using UnityEngine.UI;

public class FlowerPot : MonoBehaviour
{
    [Header("Configuración de Riego")]
    public float wateringSpeed = 30f;           // Agua transferida por segundo
    public float requiredWater = 100f;          // Agua total necesaria para crecer
    private float currentWaterReceived = 0f;     // Agua acumulada

    [Header("UI")]
    public Slider progressBar;                   // Barra de progreso del riego (opcional)
    public GameObject canvas;                    // Canvas que contiene la barra

    [Header("Resultado")]
    public GameObject treePrefab;
    public Transform spawnPoint;

    // Referencias internas
    private bool isWatering = false;
    private PlayerController playerController;
    private PlayerWater playerWater;

    [Header("VFX Riego")]
    public GameObject wateringVFX;
    private GameObject _wateringEffect;

    [Header("SFX Riego")]
    public AudioClip wateringSound;
    [Range(0f, 1f)] public float wateringVolume = 0.5f;
    public float soundStartTime = 1.5f; // ← segundo exacto donde empieza el audio
    private AudioSource _audioSource;
    void Start()
    {

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = true;
        _audioSource.volume = wateringVolume;

        if (progressBar != null)
        {
            progressBar.maxValue = requiredWater;
            progressBar.value = 0;
        }

        if (canvas != null)
            canvas.SetActive(false);
    }

    void Update()
    {
        if (!isWatering) return; // *** NUEVO: salir inmediatamente si no está regando ***

        if (playerWater != null && playerWater.currentWater > 0)
        {
            float waterToTransfer = wateringSpeed * Time.deltaTime;
            waterToTransfer = Mathf.Min(waterToTransfer, playerWater.currentWater);
            float remaining = requiredWater - currentWaterReceived;
            waterToTransfer = Mathf.Min(waterToTransfer, remaining);

            if (waterToTransfer > 0)
            {
                playerWater.UseWater(waterToTransfer);
                currentWaterReceived += waterToTransfer;

                if (progressBar != null)
                    progressBar.value = currentWaterReceived;
            }
        }

        // Primero checar si completó
        if (currentWaterReceived >= requiredWater)
        {
            GrowTree();
            return; // *** NUEVO: return para no ejecutar el check de abajo ***
        }

        // Luego checar si se quedó sin agua
        if (playerWater == null || playerWater.currentWater <= 0)
        {
            StopWatering();
        }
    }

    public void StartWatering()
    {
        // Obtener referencia al jugador y su agua
        if (playerController == null)
            playerController = PlayerController.instance;

        if (playerController != null)
            playerWater = playerController.playerWater;

        if (playerWater != null && currentWaterReceived < requiredWater)
        {
            isWatering = true;

            playerWater.StartWateringStream(); // ✅

            // ✅ VFX
            if (wateringVFX != null && _wateringEffect == null)
            {
                _wateringEffect = Instantiate(wateringVFX, transform.position, Quaternion.identity);
                _wateringEffect.transform.SetParent(transform);
            }

            // ✅ SFX desde segundo específico
            if (wateringSound != null)
            {
                _audioSource.clip = wateringSound;
                // ✅ Asegurar que el tiempo no supere la duración del clip
                _audioSource.time = Mathf.Clamp(soundStartTime, 0f, wateringSound.length - 0.1f);
                _audioSource.Play();
            }

            Debug.Log("Comenzó a regar");
        }
    }

    public void StopWatering()
    {
        isWatering = false;

        if (playerWater != null) playerWater.StopWateringStream();

        // ✅ Detener VFX
        if (_wateringEffect != null)
        {
            Destroy(_wateringEffect);
            _wateringEffect = null;
        }

        // ✅ Detener SFX
        _audioSource.Stop();
        Debug.Log("Dejó de regar");
    }

    void GrowTree()
    {
        isWatering = false;
        if (playerWater != null) playerWater.StopWateringStream(); // ✅
        // ✅ Limpiar VFX y SFX al crecer
        if (_wateringEffect != null)
        {
            Destroy(_wateringEffect);
            _wateringEffect = null;
        }
        _audioSource.Stop();

        // *** NUEVO: avisar al PlayerWater que deje de "regar" ***
        if (playerController == null)
            playerController = PlayerController.instance;

        SessionManager.Instance?.AddCoin(5);

        // En realidad el problema está en que FlowerPot.Update() sigue
        // llamando UseWater() después de completarse. El fix correcto:
        Instantiate(treePrefab, spawnPoint.position, Quaternion.identity);

        if (PlayerController.instance != null && PlayerController.instance.currentFlowerPot == this)
            PlayerController.instance.currentFlowerPot = null;

        if (canvas != null)
            canvas.SetActive(false);

        GetComponent<Collider>().enabled = false;

        enabled = false; // Esto detiene Update() — ¡ya no llama UseWater()!
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
                canvas.SetActive(true);

            if (PlayerController.instance != null)
                PlayerController.instance.currentFlowerPot = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
                canvas.SetActive(false);

            StopWatering(); // Por si acaso

            if (PlayerController.instance != null && PlayerController.instance.currentFlowerPot == this)
                PlayerController.instance.currentFlowerPot = null;
        }
    }
}