using UnityEngine;
using TMPro;

public class CoinUIHandler : MonoBehaviour
{
    public TextMeshProUGUI sessionText;
    public GameObject uiContainer; // El CoinGroup
    public float displayDuration = 3f;
    private float timer;

    void Start()
    {
        if (uiContainer != null) uiContainer.SetActive(false);
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                if (uiContainer != null) uiContainer.SetActive(false);
            }
        }
    }

    public void UpdateSessionUI()
    {
        if (SessionManager.Instance != null && sessionText != null)
        {
            sessionText.text = SessionManager.Instance.coinsCollectedThisRun.ToString() + "c";
        }

        if (uiContainer != null)
        {
            uiContainer.SetActive(true);
            timer = displayDuration;
        }
    }
}