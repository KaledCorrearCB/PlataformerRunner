using UnityEngine;
using System;
using System.Collections.Generic;

public class DailyMissionManager : MonoBehaviour
{
    public static DailyMissionManager Instance;

    // Evento que dispara cada vez que cambia el progreso → MissionUI se suscribe
    public static event Action OnMissionProgressChanged;

    [Header("Configuración")]
    public int activeMissionCount = 3;

    [Header("Pool de misiones disponibles")]
    public List<MissionDefinition> allPossibleMissions = new List<MissionDefinition>();

    private int[] activeMissionIndices = new int[3];
    private int[] missionProgress = new int[3];
    private bool[] missionCompleted = new bool[3];

    private const string KEY_LAST_RESET = "DailyMission_LastReset";
    private const string KEY_MISSION_PREFIX = "DailyMission_Index_";
    private const string KEY_PROGRESS_PREFIX = "DailyMission_Progress_";
    private const string KEY_COMPLETED_PREFIX = "DailyMission_Completed_";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadOrResetMissions();
    }

    // ─── INICIALIZACIÓN ────────────────────────────────────────────────────────

    void LoadOrResetMissions()
    {
        string lastResetStr = PlayerPrefs.GetString(KEY_LAST_RESET, "");
        bool shouldReset = true;

        if (!string.IsNullOrEmpty(lastResetStr))
        {
            DateTime lastReset = DateTime.Parse(lastResetStr);
            shouldReset = (DateTime.Now - lastReset).TotalHours >= 24;
        }

        if (shouldReset) GenerateNewMissions();
        else LoadSavedMissions();

        // Notifica a cualquier UI que ya esté activa
        OnMissionProgressChanged?.Invoke();
    }

    void GenerateNewMissions()
    {
        if (allPossibleMissions.Count < activeMissionCount)
        {
            Debug.LogWarning("[DailyMissions] No hay suficientes misiones en el pool.");
            return;
        }

        List<int> available = new List<int>();
        for (int i = 0; i < allPossibleMissions.Count; i++) available.Add(i);

        for (int i = 0; i < activeMissionCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            activeMissionIndices[i] = available[randomIndex];
            available.RemoveAt(randomIndex);
            missionProgress[i] = 0;
            missionCompleted[i] = false;
        }

        PlayerPrefs.SetString(KEY_LAST_RESET, DateTime.Now.ToString());
        SaveMissions();
        Debug.Log("[DailyMissions] Nuevas misiones generadas.");
    }

    void LoadSavedMissions()
    {
        for (int i = 0; i < activeMissionCount; i++)
        {
            activeMissionIndices[i] = PlayerPrefs.GetInt(KEY_MISSION_PREFIX + i, 0);
            missionProgress[i] = PlayerPrefs.GetInt(KEY_PROGRESS_PREFIX + i, 0);
            missionCompleted[i] = PlayerPrefs.GetInt(KEY_COMPLETED_PREFIX + i, 0) == 1;
        }
        Debug.Log("[DailyMissions] Misiones cargadas desde disco.");
    }

    void SaveMissions()
    {
        for (int i = 0; i < activeMissionCount; i++)
        {
            PlayerPrefs.SetInt(KEY_MISSION_PREFIX + i, activeMissionIndices[i]);
            PlayerPrefs.SetInt(KEY_PROGRESS_PREFIX + i, missionProgress[i]);
            PlayerPrefs.SetInt(KEY_COMPLETED_PREFIX + i, missionCompleted[i] ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    // ─── PROGRESO EN TIEMPO REAL ───────────────────────────────────────────────

    /// <summary>
    /// Actualiza el progreso de una misión tipo específico.
    /// Llama con el valor ACUMULADO del run actual (no el delta).
    /// </summary>
    public void UpdateRealtimeProgress(MissionType type, int currentRunAmount)
    {
        bool changed = false;

        for (int i = 0; i < activeMissionCount; i++)
        {
            if (missionCompleted[i]) continue;

            MissionDefinition mission = GetActiveMission(i);
            if (mission == null || mission.type != type) continue;

            // Leer el progreso base (lo que había antes de este run)
            int baseProgress = PlayerPrefs.GetInt("DailyMission_Base_" + i, 0);
            int newProgress = Mathf.Min(baseProgress + currentRunAmount, mission.goal);

            if (newProgress != missionProgress[i])
            {
                missionProgress[i] = newProgress;
                changed = true;

                if (missionProgress[i] >= mission.goal)
                    CompleteMission(i);
            }
        }

        if (changed)
        {
            SaveMissions();
            OnMissionProgressChanged?.Invoke();
        }
    }

    /// <summary>
    /// Consolida el progreso de este run como base permanente al finalizar.
    /// Llama esto desde SessionManager.FinalizeRun().
    /// </summary>
    public void FinalizeRunProgress(int coins, int distance, int helped)
    {
        FinalizeType(MissionType.CollectCoins, coins);
        FinalizeType(MissionType.TravelDistance, distance);
        FinalizeType(MissionType.HelpPeople, helped);
        SaveMissions();
        OnMissionProgressChanged?.Invoke();
    }

    void FinalizeType(MissionType type, int amount)
    {
        for (int i = 0; i < activeMissionCount; i++)
        {
            MissionDefinition mission = GetActiveMission(i);
            if (mission == null || mission.type != type) continue;

            int baseProgress = PlayerPrefs.GetInt("DailyMission_Base_" + i, 0);
            int newBase = Mathf.Min(baseProgress + amount, mission.goal);
            PlayerPrefs.SetInt("DailyMission_Base_" + i, newBase);
            missionProgress[i] = newBase;

            if (!missionCompleted[i] && missionProgress[i] >= mission.goal)
                CompleteMission(i);
        }
    }

    void CompleteMission(int index)
    {
        missionCompleted[index] = true;
        MissionDefinition mission = GetActiveMission(index);
        if (mission != null)
        {
            GameData.AddToGlobalPocket(mission.rewardCoins);
            Debug.Log($"[DailyMissions] ¡Misión completada! Recompensa: +{mission.rewardCoins} monedas");
        }
    }

    // ─── GETTERS ───────────────────────────────────────────────────────────────

    public MissionDefinition GetActiveMission(int index)
    {
        if (index < 0 || index >= activeMissionCount) return null;
        int mi = activeMissionIndices[index];
        if (mi < 0 || mi >= allPossibleMissions.Count) return null;
        return allPossibleMissions[mi];
    }

    public int GetProgress(int index) => missionProgress[index];
    public bool IsCompleted(int index) => missionCompleted[index];
    public int GetActiveMissionCount() => activeMissionCount;

    public string GetTimeUntilReset()
    {
        string lastResetStr = PlayerPrefs.GetString(KEY_LAST_RESET, "");
        if (string.IsNullOrEmpty(lastResetStr)) return "00:00:00";

        DateTime nextReset = DateTime.Parse(lastResetStr).AddHours(24);
        TimeSpan remaining = nextReset - DateTime.Now;
        if (remaining.TotalSeconds <= 0) return "00:00:00";
        return $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    [ContextMenu("Forzar reset de misiones (testing)")]
    public void ForceReset()
    {
        for (int i = 0; i < activeMissionCount; i++)
            PlayerPrefs.DeleteKey("DailyMission_Base_" + i);
        PlayerPrefs.DeleteKey(KEY_LAST_RESET);
        LoadOrResetMissions();
    }
}