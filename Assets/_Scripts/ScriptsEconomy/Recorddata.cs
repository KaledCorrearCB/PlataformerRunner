using UnityEngine;

/// <summary>
/// Guarda y recupera el mejor run del jugador usando PlayerPrefs.
/// El récord se actualiza solo si la distancia recorrida supera la anterior.
/// </summary>
public static class RecordData
{
    private const string KEY_COINS = "Record_Coins";
    private const string KEY_DISTANCE = "Record_Distance";
    private const string KEY_HELPED = "Record_Helped";

    public static int GetRecordCoins() => PlayerPrefs.GetInt(KEY_COINS, 0);
    public static float GetRecordDistance() => PlayerPrefs.GetFloat(KEY_DISTANCE, 0f);
    public static int GetRecordHelped() => PlayerPrefs.GetInt(KEY_HELPED, 0);

    /// <summary>
    /// Compara el run actual contra el récord guardado.
    /// Actualiza el récord si la distancia de este run es mayor.
    /// Retorna true si se batió el récord.
    /// </summary>
    public static bool TrySaveRecord(int coins, float distance, int helped)
    {
        if (distance > GetRecordDistance())
        {
            PlayerPrefs.SetInt(KEY_COINS, coins);
            PlayerPrefs.SetFloat(KEY_DISTANCE, distance);
            PlayerPrefs.SetInt(KEY_HELPED, helped);
            PlayerPrefs.Save();
            Debug.Log($"[RecordData] ¡Nuevo récord! Distancia: {distance:F0}m | Monedas: {coins} | Personas: {helped}");
            return true;
        }
        return false;
    }

    /// <summary>Borra el récord guardado. Útil para testing.</summary>
    public static void ResetRecord()
    {
        PlayerPrefs.DeleteKey(KEY_COINS);
        PlayerPrefs.DeleteKey(KEY_DISTANCE);
        PlayerPrefs.DeleteKey(KEY_HELPED);
        PlayerPrefs.Save();
        Debug.Log("[RecordData] Récord borrado.");
    }
}