using UnityEngine;
using UnityEngine.UI;

public class FlowerPot : MonoBehaviour
{
    public Slider waterBar;

    public float waterAmount = 0;
    public float maxWater = 100;

    public float wateringSpeed = 30f;

    public GameObject treePrefab;
    public Transform spawnPoint;

    public GameObject canvas;

    private bool isWatering = false;

    void Start()
    {
        waterBar.value = 0;
        canvas.SetActive(false);
    }

    void Update()
    {
        if (isWatering)
        {
            waterAmount += wateringSpeed * Time.deltaTime;

            waterAmount = Mathf.Clamp(waterAmount, 0, maxWater);
            waterBar.value = waterAmount;

            if (waterAmount >= maxWater)
            {
                GrowTree();
            }
        }
    }

    public void StartWatering()
    {
        isWatering = true;
    }

    public void StopWatering()
    {
        isWatering = false;
    }

    void GrowTree()
    {
        Instantiate(treePrefab, spawnPoint.position, Quaternion.identity);

        canvas.SetActive(false); // ocultar barra

        GetComponent<Collider>().enabled = false; // ya no se puede interactuar

        enabled = false; // desactiva este script
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.SetActive(true);
            PlayerController.instance.currentFlowerPot = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.SetActive(false);
            isWatering = false;

            if (PlayerController.instance.currentFlowerPot == this)
                PlayerController.instance.currentFlowerPot = null;
        }
    }
}