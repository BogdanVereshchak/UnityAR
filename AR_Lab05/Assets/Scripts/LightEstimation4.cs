using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;

/// Адаптує Directional Light до реального освітлення.
/// Відображає яскравість та температуру на UI.
public class LightEstimation4 : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARCameraManager cameraManager;

    [Header("Scene Light")]
    [SerializeField] private Light mainLight;

    [Header("UI Indicators")]
    [Tooltip("Зображення, яке змінює колір відповідно до температури")]
    [SerializeField] private Image temperatureColorIndicator;
    [SerializeField] private Image brightnessIndicator;
    
    [Tooltip("Текст для виводу температури (Кельвіни)")]
    [SerializeField] private TMP_Text temperatureText;

    [Tooltip("Текст для виводу яскравості (Люмени або інтенсивність)")]
    [SerializeField] private TMP_Text brightnessText;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private float smoothBrightness = 1f;
    private float smoothTemperature = 6500f; 
    private int frameCount = 0;

    void OnEnable()
    {
        if (cameraManager == null || mainLight == null)
        {
            Debug.LogError("[LIGHT] Camera Manager або Main Light не призначені в Inspector!");
            enabled = false;
            return;
        }
        cameraManager.frameReceived += OnFrameReceived;
    }

    void OnDisable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= OnFrameReceived;
    }

    void Update()
    {
        // Оновлюємо інтерфейс кожного кадру для плавності
        UpdateLightUI();
    }

    private void OnFrameReceived(ARCameraFrameEventArgs args)
    {
        frameCount++;
        var est = args.lightEstimation;

        // 1. Обробка Інтенсивності (Яскравості)
        if (est.averageIntensityInLumens.HasValue)
        {
            float lumens = est.averageIntensityInLumens.Value;
            // Конвертуємо люмени в інтенсивність для Light (зазвичай 0.1 - 2.0)
            float target = Mathf.Clamp(lumens / 1000f, 0.1f, 3f);
            smoothBrightness = Mathf.Lerp(smoothBrightness, target, Time.deltaTime * 5f);
            mainLight.intensity = smoothBrightness;
        }
        else if (est.averageBrightness.HasValue)
        {
            smoothBrightness = Mathf.Lerp(smoothBrightness, est.averageBrightness.Value, Time.deltaTime * 5f);
            mainLight.intensity = smoothBrightness;
        }

        // 2. Напрямок світла (якщо підтримується пристроєм)
        if (est.mainLightDirection.HasValue)
            mainLight.transform.rotation = Quaternion.LookRotation(est.mainLightDirection.Value);

        // 3. Колірна температура
        if (est.averageColorTemperature.HasValue)
        {
            smoothTemperature = Mathf.Lerp(smoothTemperature, est.averageColorTemperature.Value, Time.deltaTime * 5f);
            mainLight.colorTemperature = smoothTemperature;
            mainLight.useColorTemperature = true;
        }

        // Debug лог раз на секунду (60 кадрів)
        if (showDebugInfo && frameCount % 60 == 0)
        {
            Debug.Log($"[LIGHT] Яскравість: {mainLight.intensity:F2}, Температура: {mainLight.colorTemperature:F0}K");
        }
    }

    private void UpdateLightUI()
    {
        // Відображення Температури
        if (temperatureColorIndicator != null)
        {
            // Перетворюємо Кельвіни в колір RGB для наочності
            Color colorFromTemp = Mathf.CorrelatedColorTemperatureToRGB(mainLight.colorTemperature);
            temperatureColorIndicator.color = colorFromTemp;
        }

        if (temperatureText != null)
        {
            temperatureText.text = $"Temp: {Mathf.RoundToInt(mainLight.colorTemperature)} K";
        }

        if (brightnessIndicator != null)
        {
            Color baseColor = Color.yellow;
            brightnessIndicator.color = baseColor * Mathf.Clamp01(mainLight.intensity / 1.0f);
        }

        // Відображення Яскравості
        if (brightnessText != null)
        {
            // Виводимо інтенсивність у відсотках або просто значенням
            brightnessText.text = $"Brightness: {mainLight.intensity:F2}";
        }
    }
}