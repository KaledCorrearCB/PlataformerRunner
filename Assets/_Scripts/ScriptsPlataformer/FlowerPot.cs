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
        isWatering = false;

        // *** NUEVO: avisar al PlayerWater que deje de "regar" ***
        if (playerController == null)
            playerController = PlayerController.instance;



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