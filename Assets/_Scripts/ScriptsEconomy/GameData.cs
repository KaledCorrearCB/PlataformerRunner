using UnityEngine;

public static class GameData
{
    private const string TOTAL_COINS_KEY = "TotalCoins";

    // Lee el disco duro
    public static int GetGlobalPocket()
    {
        return PlayerPrefs.GetInt(TOTAL_COINS_KEY, 0);
    }

    // Suma y guarda en el disco duro
    public static void AddToGlobalPocket(int amount)
    {
        int current = GetGlobalPocket();
        PlayerPrefs.SetInt(TOTAL_COINS_KEY, current + amount);
        PlayerPrefs.Save();
    }
}