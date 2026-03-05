// HelpedCharactersData.cs
// Base de datos persistente de personas ayudadas.
// Funciona exactamente igual que GameData pero para personas, no kits.
// Guarda en PlayerPrefs cuántas personas de cada tipo han sido ayudadas
// a lo largo de todas las sesiones.
//
// No necesita estar en un GameObject, es una clase estática como GameData.

using UnityEngine;

public static class HelpedCharactersData
{
    // Claves de guardado por tipo de kit (= tipo de persona ayudada)
    private static string HelpedKey(KitType type) => $"Helped_{type}";
    private const string HELPED_TOTAL_KEY = "Helped_Total";

    /// <summary>Registra que una persona fue ayudada con un kit específico.</summary>
    public static void RegisterHelped(KitType type)
    {
        // Suma al contador de ese tipo
        int currentType = GetHelpedByType(type);
        PlayerPrefs.SetInt(HelpedKey(type), currentType + 1);

        // Suma al total global
        int currentTotal = GetTotalHelped();
        PlayerPrefs.SetInt(HELPED_TOTAL_KEY, currentTotal + 1);

        PlayerPrefs.Save();

        Debug.Log($"[HelpedData] {type} ayudados: {currentType + 1} | Total global: {currentTotal + 1}");
    }

    /// <summary>Cuántas personas de un tipo específico han sido ayudadas (todas las sesiones).</summary>
    public static int GetHelpedByType(KitType type)
    {
        return PlayerPrefs.GetInt(HelpedKey(type), 0);
    }

    /// <summary>Total de personas ayudadas en todas las sesiones y tipos.</summary>
    public static int GetTotalHelped()
    {
        return PlayerPrefs.GetInt(HELPED_TOTAL_KEY, 0);
    }

    /// <summary>Resetea el conteo. Útil para testing.</summary>
    public static void ResetAll()
    {
        foreach (KitType type in System.Enum.GetValues(typeof(KitType)))
            PlayerPrefs.DeleteKey(HelpedKey(type));

        PlayerPrefs.DeleteKey(HELPED_TOTAL_KEY);
        PlayerPrefs.Save();
        Debug.Log("[HelpedData] Base de datos reseteada.");
    }
}