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
    void Start()
    {
        playerController = GetComponent<PlayerController>();

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
            Debug.Log("💧 Comenzó a absorber agua");
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
        Debug.Log("💧 Dejó de absorber agua");
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
}