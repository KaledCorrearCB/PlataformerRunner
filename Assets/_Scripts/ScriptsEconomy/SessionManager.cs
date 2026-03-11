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

    // ─── MONEDAS ───────────────────────────────────────────────────────────────

    public void AddCoin(int value)
    {
        coinsCollectedThisRun += value;
        Object.FindFirstObjectByType<CoinUIHandler>()?.UpdateSessionUI();

        // ★ Actualiza misiones en tiempo real
        DailyMissionManager.Instance?.UpdateRealtimeProgress(
            MissionType.CollectCoins, coinsCollectedThisRun);
    }

    // ─── DISTANCIA ─────────────────────────────────────────────────────────────

    /// <summary>Llama esto cada frame desde RunnerController con la distancia acumulada del run.</summary>
    public void SetDistance(float distance)
    {
        distanceTraveledThisRun = distance;

        // ★ Actualiza misiones en tiempo real (cada frame, el manager filtra si hay cambio)
        DailyMissionManager.Instance?.UpdateRealtimeProgress(
            MissionType.TravelDistance, Mathf.FloorToInt(distanceTraveledThisRun));
    }

    // ─── PERSONAS ──────────────────────────────────────────────────────────────

    public void RegisterHelped(KitType type)
    {
        peopleHelpedThisRun++;
        HelpedCharactersData.RegisterHelped(type);

        // ★ Actualiza misiones en tiempo real
        DailyMissionManager.Instance?.UpdateRealtimeProgress(
            MissionType.HelpPeople, peopleHelpedThisRun);
    }

    // ─── FIN DE RUN ────────────────────────────────────────────────────────────

    public void FinalizeRun()
    {
        // Guardar globales
        GameData.AddToGlobalPocket(coinsCollectedThisRun);
        GameData.AddToGlobalDistance(distanceTraveledThisRun);

        // Guardar récord si es el mejor run
        RecordData.TrySaveRecord(coinsCollectedThisRun, distanceTraveledThisRun, peopleHelpedThisRun);

        // ★ Consolida el progreso de este run en las misiones (guarda en disco)
        DailyMissionManager.Instance?.FinalizeRunProgress(
            coinsCollectedThisRun,
            Mathf.FloorToInt(distanceTraveledThisRun),
            peopleHelpedThisRun);
    }
}