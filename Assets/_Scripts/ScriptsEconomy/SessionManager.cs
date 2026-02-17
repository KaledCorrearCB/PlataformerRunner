using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    public int coinsCollectedThisRun { get; private set; }

    void Awake()
    {
        // Singleton sencillo para acceder desde cualquier lado
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        coinsCollectedThisRun = 0; // Siempre empieza en 0
    }

    public void AddCoin(int value)
    {
        coinsCollectedThisRun += value;
        // Avisar a la UI que se actualice (paso 4)
        Object.FindFirstObjectByType<CoinUIHandler>()?.UpdateSessionUI();
    }

    // Se llama cuando el jugador muere o termina el nivel
    public void FinalizeRun()
    {
        GameData.AddToGlobalPocket(coinsCollectedThisRun);
    }
}