using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TimerScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider; // Attach the Slider UI
    [SerializeField] private TextMeshProUGUI timeLabel; // TMP text e.g., "1:00:00" meaning 1 minute
    [SerializeField] private TextMeshProUGUI heatInfoLabel; // Optional TMP to show selected weather info

    [Header("Navigation")]
    [SerializeField] private int gameOverSceneBuildIndex = 2; // Scene 2 = Game Over

    private float durationSeconds = 60f;
    private float elapsedSeconds = 0f;
    private bool isRunning = false;

    public static int TargetPlantCount { get; private set; } = 10; // Exposed difficulty for other scripts

    void Start()
    {
        // Initialize heat-based game config (force reselection to ensure randomness each play)
        WeatherGameConfig.Initialize(forceReselect: true);

        if (WeatherGameConfig.HasSelection)
        {
            durationSeconds = WeatherGameConfig.DurationSeconds;
            TargetPlantCount = WeatherGameConfig.TargetPlantCount;
            if (heatInfoLabel != null)
            {
                heatInfoLabel.text = WeatherGameConfig.InfoString;
            }
        }
        else
        {
            durationSeconds = ParseDurationSeconds(timeLabel);
        }
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 100f;
            progressSlider.value = 0f;
        }
        elapsedSeconds = 0f;
        isRunning = true;
        UpdateTimeLabel(durationSeconds);
    }

    void Update()
    {
        if (!isRunning)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        float t = durationSeconds > 0f ? Mathf.Clamp01(elapsedSeconds / durationSeconds) : 1f;
        float percentage = t * 100f;

        if (progressSlider != null)
        {
            progressSlider.value = percentage;
        }

        float remaining = Mathf.Max(0f, durationSeconds - elapsedSeconds);
        UpdateTimeLabel(remaining);

        if (percentage >= 100f)
        {
            isRunning = false;
            UpdateTimeLabel(0f);
            SceneManager.LoadScene(gameOverSceneBuildIndex);
        }
    }

    private static float ParseDurationSeconds(TextMeshProUGUI label)
    {
        // Expected label examples: "1:00:00" => 1 minute, "2:30:00" => 2 minutes 30 seconds
        // We interpret as mm:ss:ff (ff optional). If parsing fails, default to 60 seconds.
        if (label == null || string.IsNullOrEmpty(label.text))
        {
            return 60f;
        }

        string text = label.text.Trim();
        string[] parts = text.Split(':');
        int minutes = 0;
        int seconds = 0;
        int centiseconds = 0;

        if (parts.Length >= 1)
        {
            int.TryParse(parts[0], out minutes);
        }
        if (parts.Length >= 2)
        {
            int.TryParse(parts[1], out seconds);
        }
        if (parts.Length >= 3)
        {
            int.TryParse(parts[2], out centiseconds);
        }

        minutes = Mathf.Max(0, minutes);
        seconds = Mathf.Clamp(seconds, 0, 59);
        centiseconds = Mathf.Clamp(centiseconds, 0, 99);

        float totalSeconds = minutes * 60f + seconds + (centiseconds / 100f);
        if (totalSeconds <= 0f)
        {
            totalSeconds = 60f;
        }
        return totalSeconds;
    }

    private void UpdateTimeLabel(float remainingSeconds)
    {
        if (timeLabel == null)
        {
            return;
        }
        if (remainingSeconds < 0f)
        {
            remainingSeconds = 0f;
        }
        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((remainingSeconds - Mathf.Floor(remainingSeconds)) * 100f);
        timeLabel.text = $"{minutes}:{seconds:00}:{centiseconds:00}";
    }
}
