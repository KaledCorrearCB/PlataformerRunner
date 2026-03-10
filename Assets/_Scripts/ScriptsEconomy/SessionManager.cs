using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;

    public int coinsCollectedThisRun { get; private set; }
    public float distanceTraveledThisRun { get; private set; }
    public int peopleHelpedThisRun { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        coinsCollectedThisRun = 0;
        distanceTraveledThisRun = 0f;
        peopleHelpedThisRun = 0;
    }

    public void AddCoin(int value)
    {
        coinsCollectedThisRun += value;
        Object.FindFirstObjectByType<CoinUIHandler>()?.UpdateSessionUI();
    }

    /// <summary>Llama esto cada frame desde RunnerController con la distancia acumulada.</summary>
    public void SetDistance(float distance)
    {
        distanceTraveledThisRun = distance;
    }

    /// <summary>Llama esto cada vez que el jugador ayuda a una persona.</summary>
    public void RegisterHelped(KitType type)
    {
        peopleHelpedThisRun++;
        HelpedCharactersData.RegisterHelped(type);
    }

    /// <summary>Se llama cuando el jugador muere o termina el nivel.</summary>
    public void FinalizeRun()
    {
        // Guardar globales

        GameData.AddToGlobalPocket(coinsCollectedThisRun);
        GameData.AddToGlobalDistance(distanceTraveledThisRun);


        // Guardar récord si es el mejor
        RecordData.TrySaveRecord(coinsCollectedThisRun, distanceTraveledThisRun, peopleHelpedThisRun);
    }
}