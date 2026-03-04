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

    void Start()
    {
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
        if (isWatering)
        {
            // Verificar que el jugador sigue teniendo agua
            if (playerWater != null && playerWater.currentWater > 0)
            {
                // Cuánta agua transferir en este frame (sin exceder lo necesario)
                float waterToTransfer = wateringSpeed * Time.deltaTime;

                // Limitar por el agua disponible del jugador
                waterToTransfer = Mathf.Min(waterToTransfer, playerWater.currentWater);

                // Limitar por lo que falta para completar la maceta
                float remaining = requiredWater - currentWaterReceived;
                waterToTransfer = Mathf.Min(waterToTransfer, remaining);

                if (waterToTransfer > 0)
                {
                    // 1. Restar agua al jugador
                    playerWater.UseWater(waterToTransfer); // Este método ya actualiza la barra del jugador

                    // 2. Sumar a la maceta
                    currentWaterReceived += waterToTransfer;

                    // 3. Actualizar barra de progreso (si existe)
                    if (progressBar != null)
                        progressBar.value = currentWaterReceived;

                    Debug.Log($"Regando: +{waterToTransfer} agua. Progreso: {currentWaterReceived}/{requiredWater}");
                }
            }

            // Si ya se completó, crecer
            if (currentWaterReceived >= requiredWater)
            {
                GrowTree();
            }

            // Si el jugador se quedó sin agua, detener riego automáticamente
            if (playerWater == null || playerWater.currentWater <= 0)
            {
                StopWatering();
            }
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
            Debug.Log("Comenzó a regar");
        }
    }

    public void StopWatering()
    {
        isWatering = false;
        Debug.Log("Dejó de regar");
    }

    void GrowTree()
    {
        Instantiate(treePrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log("🌳 Árbol crecido");

        if (canvas != null)
            canvas.SetActive(false);

        GetComponent<Collider>().enabled = false;
        enabled = false; // Desactiva este script
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