using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float rotationSpeed = 100f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Guardar moneda
            GameData.AddCoins(value);

            // 2. Avisar a la UI (esto funciona en cualquier escena)
            CoinUIHandler uiHandler = Object.FindFirstObjectByType<CoinUIHandler>();
            if (uiHandler != null)
            {
                uiHandler.ShowCoins();
            }

            Destroy(gameObject);
        }
    }
}