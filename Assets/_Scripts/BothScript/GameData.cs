using UnityEngine;

public static class GameData
{
    private const string COIN_KEY = "TotalCoins";

    // Sumar y guardar
    public static void AddCoins(int amount)
    {
        int current = GetTotalCoins();
        PlayerPrefs.SetInt(COIN_KEY, current + amount);
        PlayerPrefs.Save();
    }

    // Leer el total
    public static int GetTotalCoins()
    {
        return PlayerPrefs.GetInt(COIN_KEY, 0);
    }
}