using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class WeatherGameConfig
{
    public struct WeatherSample
    {
        public string location;
        public string dateTime;
        public float temperatureC;
        public float humidityPct;
        public float precipitationMm;
        public float windSpeedKmh;
    }

    public static bool Initialized { get; private set; }
    public static bool HasSelection => _hasSelection;

    public static WeatherSample SelectedSample { get; private set; }
    public static float DurationSeconds { get; private set; } = 60f;
    public static int TargetPlantCount { get; private set; } = 10;
    public static string InfoString { get; private set; } = string.Empty;

    private static bool _hasSelection = false;

    public static void Initialize(float highHeatThresholdC = 35f, bool forceReselect = false)
    {
        if (Initialized && !forceReselect)
        {
            return;
        }
        Initialized = true;

        // Reset prior selection if forcing a reselect (e.g., play mode without domain reload)
        if (forceReselect)
        {
            _hasSelection = false;
            InfoString = string.Empty;
            DurationSeconds = 60f;
            TargetPlantCount = 10;
            SelectedSample = default;
        }

        try
        {
            TextAsset csv = Resources.Load<TextAsset>("weather_data");
            if (csv == null || string.IsNullOrEmpty(csv.text))
            {
                Debug.LogWarning("WeatherGameConfig: Could not load Resources/weather_data.csv; using defaults.");
                return;
            }

            // Reservoir sampling over high-heat rows to avoid storing the entire file in memory
            using (var reader = new StringReader(csv.text))
            {
                string header = reader.ReadLine(); // skip header
                string line;
                int matchCount = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    // CSV format: Location,Date_Time,Temperature_C,Humidity_pct,Precipitation_mm,Wind_Speed_kmh
                    var parts = line.Split(',');
                    if (parts.Length < 6)
                    {
                        continue;
                    }

                    if (!TryParseFloat(parts[2], out float tempC))
                    {
                        continue;
                    }

                    if (tempC < highHeatThresholdC)
                    {
                        continue;
                    }

                    matchCount++;
                    // Pick this row with probability 1/matchCount
                    if (UnityEngine.Random.value < (1f / matchCount))
                    {
                        SelectedSample = new WeatherSample
                        {
                            location = parts[0],
                            dateTime = parts[1],
                            temperatureC = tempC,
                            humidityPct = TryParseFloat(parts[3], out var h) ? h : 0f,
                            precipitationMm = TryParseFloat(parts[4], out var p) ? p : 0f,
                            windSpeedKmh = TryParseFloat(parts[5], out var w) ? w : 0f
                        };
                        _hasSelection = true;
                    }
                }
            }

            if (_hasSelection)
            {
                // Map heat to game settings
                float factor = Mathf.InverseLerp(highHeatThresholdC, highHeatThresholdC + 10f, SelectedSample.temperatureC);
                factor = Mathf.Clamp01(factor);

                // Hotter => shorter duration, more plants required
                DurationSeconds = Mathf.Lerp(75f, 45f, factor);
                TargetPlantCount = Mathf.RoundToInt(Mathf.Lerp(8f, 20f, factor));

                InfoString = $"{SelectedSample.location} {SelectedSample.dateTime} | {SelectedSample.temperatureC:F1}°C | Plants: {TargetPlantCount} | Duration: {Mathf.RoundToInt(DurationSeconds)}s";
            }
            else
            {
                Debug.LogWarning("WeatherGameConfig: No high-heat rows found; using defaults.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"WeatherGameConfig: Exception while initializing - {ex.Message}");
        }
    }

    private static bool TryParseFloat(string s, out float value)
    {
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}


