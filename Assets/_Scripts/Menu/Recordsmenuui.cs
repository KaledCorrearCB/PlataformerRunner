using UnityEngine;
using TMPro;

/// <summary>
/// Adjunta este script al Panel del menú de récords.
/// Asigna los TextMeshPro en el Inspector según la sección:
///
///  ── MEJOR RUN (Record 1) ──────────────────────────────────────
///   recordCoinsText       → monedas del mejor run
///   recordDistanceText    → distancia del mejor run
///
///  ── TOTALES ACUMULADOS (Record 2) ────────────────────────────
///   totalCoinsText        → suma de monedas de todos los runs
///   totalDistanceText     → suma de distancia de todos los runs
///   totalHelpedText       → total de personas ayudadas
/// </summary>
public class RecordsMenuUI : MonoBehaviour
{
    [Header("Mejor Run — Record 1")]
    [Tooltip("Monedas del mejor run")]
    public TextMeshProUGUI recordCoinsText;

    [Tooltip("Distancia del mejor run")]
    public TextMeshProUGUI recordDistanceText;

    [Header("Totales Acumulados — Record 2")]
    [Tooltip("Suma de todas las monedas recolectadas en todos los runs")]
    public TextMeshProUGUI totalCoinsText;

    [Tooltip("Suma de toda la distancia recorrida en todos los runs")]
    public TextMeshProUGUI totalDistanceText;

    [Tooltip("Total de personas ayudadas en todos los runs")]
    public TextMeshProUGUI totalHelpedText;

    // Se auto-actualiza cada vez que el panel se activa
    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // ── Mejor run ──
        if (recordCoinsText != null)
            recordCoinsText.text = RecordData.GetRecordCoins().ToString();

        if (recordDistanceText != null)
            recordDistanceText.text = Mathf.FloorToInt(RecordData.GetRecordDistance()) + "m";

        // ── Totales acumulados ──
        if (totalCoinsText != null)
            totalCoinsText.text = GameData.GetGlobalPocket().ToString();

        if (totalDistanceText != null)
            totalDistanceText.text = Mathf.FloorToInt(GameData.GetGlobalDistance()) + "m";

        if (totalHelpedText != null)
            totalHelpedText.text = HelpedCharactersData.GetTotalHelped().ToString();
    }
}