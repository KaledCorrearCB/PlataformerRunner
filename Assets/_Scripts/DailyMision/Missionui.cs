using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la UI de misiones diarias.
/// Se suscribe al evento OnMissionProgressChanged para actualizarse automáticamente
/// sin importar el orden de inicialización.
/// </summary>
public class MissionUI : MonoBehaviour
{
    [Header("Slots de misiones (tamaño 3)")]
    public TextMeshProUGUI[] missionTexts;
    public Slider[] progressBars;
    public GameObject[] completedIcons;

    [Header("Temporizador")]
    public TextMeshProUGUI timerText;

    [Header("Panel (solo para HUD in-game)")]
    public GameObject missionPanel;

    private bool isPanelOpen = false;

    // ─── CICLO DE VIDA ─────────────────────────────────────────────────────────

    void OnEnable()
    {
        // Suscribirse al evento del manager
        DailyMissionManager.OnMissionProgressChanged += RefreshUI;

        // Intentar refrescar si el manager ya existe
        if (DailyMissionManager.Instance != null)
            RefreshUI();
    }

    void OnDisable()
    {
        DailyMissionManager.OnMissionProgressChanged -= RefreshUI;
    }

    void Update()
    {
        if (timerText != null && DailyMissionManager.Instance != null)
            timerText.text = "Resetea en: " + DailyMissionManager.Instance.GetTimeUntilReset();
    }

    // ─── REFRESH ───────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        if (DailyMissionManager.Instance == null) return;

        int count = DailyMissionManager.Instance.GetActiveMissionCount();

        for (int i = 0; i < count; i++)
        {
            MissionDefinition mission = DailyMissionManager.Instance.GetActiveMission(i);
            if (mission == null) continue;

            int progress = DailyMissionManager.Instance.GetProgress(i);
            bool completed = DailyMissionManager.Instance.IsCompleted(i);

            // Texto
            if (i < missionTexts.Length && missionTexts[i] != null)
            {
                string desc = string.Format(mission.descriptionTemplate, mission.goal);
                string status = completed ? "✓ Completada" : $"{progress}/{mission.goal}";
                missionTexts[i].text = $"{desc}\n<size=70%>{status}  |  🪙 +{mission.rewardCoins}</size>";
            }

            // Slider
            if (i < progressBars.Length && progressBars[i] != null)
            {
                progressBars[i].minValue = 0;
                progressBars[i].maxValue = mission.goal;
                progressBars[i].value = progress;
            }

            // Ícono completado
            if (i < completedIcons.Length && completedIcons[i] != null)
                completedIcons[i].SetActive(completed);
        }
    }

    // ─── BOTÓN HUD ─────────────────────────────────────────────────────────────

    public void ToggleMissionPanel()
    {
        if (missionPanel == null) return;
        isPanelOpen = !isPanelOpen;
        missionPanel.SetActive(isPanelOpen);
        if (isPanelOpen) RefreshUI();
    }

    public void OpenPanel()
    {
        if (missionPanel == null) return;
        isPanelOpen = true;
        missionPanel.SetActive(true);
        RefreshUI();
    }

    public void ClosePanel()
    {
        if (missionPanel == null) return;
        isPanelOpen = false;
        missionPanel.SetActive(false);
    }
}