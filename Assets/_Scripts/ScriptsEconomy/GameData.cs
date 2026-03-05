// GameData.cs  ← REEMPLAZA tu GameData actual con esta versión
// Mantiene todo lo que ya tenías para monedas y agrega soporte para kits.
// Los kits se guardan en PlayerPrefs con la clave "Kit_FirstAid", "Kit_Food", etc.

using UnityEngine;

public static class GameData
{
    // ─── MONEDAS (sin cambios) ────────────────────────────────────────────────

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

    // ─── KITS ─────────────────────────────────────────────────────────────────

    // Genera automáticamente la clave de guardado según el tipo de kit.
    // Ejemplo: KitType.FirstAid → "Kit_FirstAid"
    private static string KitKey(KitType type) => $"Kit_{type}";

    /// <summary>
    /// Lee del disco cuántos kits del tipo indicado se han acumulado en total.
    /// </summary>
    public static int GetGlobalKits(KitType type)
    {
        return PlayerPrefs.GetInt(KitKey(type), 0);
    }

    /// <summary>
    /// Suma kits al total global y guarda en disco.
    /// </summary>
    public static void AddKitsToGlobalPocket(KitType type, int amount)
    {
        int current = GetGlobalKits(type);
        PlayerPrefs.SetInt(KitKey(type), current + amount);
        PlayerPrefs.Save();
    }

    // ─── UTILIDAD ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Borra todos los datos guardados. Útil para testing o resetear el juego.
    /// </summary>
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[GameData] Todos los datos borrados.");
    }
}