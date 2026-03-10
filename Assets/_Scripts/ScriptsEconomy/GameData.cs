using UnityEngine;

public static class GameData
{
    // ─── MONEDAS ──────────────────────────────────────────────────────────────
    private const string TOTAL_COINS_KEY = "TotalCoins";

    public static int GetGlobalPocket()
    {
        return PlayerPrefs.GetInt(TOTAL_COINS_KEY, 0);
    }

    public static void AddToGlobalPocket(int amount)
    {
        int current = GetGlobalPocket();
        PlayerPrefs.SetInt(TOTAL_COINS_KEY, current + amount);
        PlayerPrefs.Save();
    }

    // ─── DISTANCIA TOTAL ACUMULADA ────────────────────────────────────────────
    private const string TOTAL_DISTANCE_KEY = "TotalDistance";

    public static float GetGlobalDistance()
    {
        return PlayerPrefs.GetFloat(TOTAL_DISTANCE_KEY, 0f);
    }

    public static void AddToGlobalDistance(float amount)
    {
        float current = GetGlobalDistance();
        PlayerPrefs.SetFloat(TOTAL_DISTANCE_KEY, current + amount);
        PlayerPrefs.Save();
    }

    // ─── KITS ─────────────────────────────────────────────────────────────────
    private static string KitKey(KitType type) => $"Kit_{type}";

    public static int GetGlobalKits(KitType type)
    {
        return PlayerPrefs.GetInt(KitKey(type), 0);
    }

    public static void AddKitsToGlobalPocket(KitType type, int amount)
    {
        int current = GetGlobalKits(type);
        PlayerPrefs.SetInt(KitKey(type), current + amount);
        PlayerPrefs.Save();
    }

    // ─── UTILIDAD ─────────────────────────────────────────────────────────────
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[GameData] Todos los datos borrados.");
    }
}