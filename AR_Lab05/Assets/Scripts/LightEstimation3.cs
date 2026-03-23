using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro; // Обов'язково для роботи з UI (Image, Text)

/// Адаптує Directional Light до реального освітлення.
/// Додано: Кольоровий UI-індикатор температури світла.
public class LightEstimation3 : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARCameraManager cameraManager;

    [Header("Scene Light")]
    [SerializeField] private Light mainLight;

    [Header("UI Temperature Indicator")]
    [Tooltip("UI Зображення (Image), яке буде змінювати колір")]
    [SerializeField] private Image temperatureColorIndicator;
    [Tooltip("UI Текст (Text) для виводу цифр (опціонально)")]
    [SerializeField] private TMP_Text temperatureText;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private float smoothBrightness = 1f;
    [SerializeField] private float smoothTemperature = 6500f; // Дефолтне денне світло
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
        Debug.Log("[LIGHT] ✓ LightEstimation підписався на frameReceived");
    }

    void OnDisable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= OnFrameReceived;
        Debug.Log("[LIGHT] ✗ LightEstimation відписався від frameReceived");
    }

    // Додаємо плавне оновлення UI в Update
    void Update()
    {
        UpdateTemperatureUI();
    }

    private void OnFrameReceived(ARCameraFrameEventArgs args)
    {
        frameCount++;
        var est = args.lightEstimation;

        if (showDebugInfo && frameCount % 60 == 1)
        {
            Debug.Log($"[LIGHT] === Кадр #{frameCount} === Доступні поля: " +
            $"mainLumens={est.mainLightIntensityLumens.HasValue}, " +
            $"ambientLumens={est.averageIntensityInLumens.HasValue}, " +
            $"brightness={est.averageBrightness.HasValue}, " +
            $"direction={est.mainLightDirection.HasValue}, " +
            $"mainColor={est.mainLightColor.HasValue}, " +
            $"colorCorrection={est.colorCorrection.HasValue}, " +
            $"temperature={est.averageColorTemperature.HasValue}");
            Debug.Log($"[LIGHT] Light стан: intensity={mainLight.intensity:F2}, " +
            $"color={mainLight.color}, rotation={mainLight.transform.eulerAngles}, " +
            $"colorTemp={mainLight.colorTemperature:F0}K, shadows={mainLight.shadows}");
        }


        // ── Інтенсивність: mainLightLumens → ambientLumens → brightness ──
        if (est.mainLightIntensityLumens.HasValue)
        {
            float lumens = est.mainLightIntensityLumens.Value;
            float target = Mathf.Clamp(lumens / 1000f, 0.1f, 3f);
            smoothBrightness = Mathf.Lerp(smoothBrightness, target, Time.deltaTime * 5f);
            mainLight.intensity = smoothBrightness;
        }
        else if (est.averageIntensityInLumens.HasValue)
        {
            float lumens = est.averageIntensityInLumens.Value;
            float target = Mathf.Clamp(lumens / 1000f, 0.1f, 3f);
            smoothBrightness = Mathf.Lerp(smoothBrightness, target, Time.deltaTime * 5f);
            mainLight.intensity = smoothBrightness;
        }
        else if (est.averageBrightness.HasValue)
        {
            smoothBrightness = Mathf.Lerp(smoothBrightness, est.averageBrightness.Value, Time.deltaTime * 5f);
            mainLight.intensity = smoothBrightness;
        }

        // ── Напрямок ──
        if (est.mainLightDirection.HasValue)
            mainLight.transform.rotation = Quaternion.LookRotation(est.mainLightDirection.Value);

        // ── Колір (базовий) ──
        if (est.mainLightColor.HasValue)
        {
            mainLight.color = est.mainLightColor.Value;
        }
        else if (est.colorCorrection.HasValue)
        {
            mainLight.color = est.colorCorrection.Value;
            mainLight.useColorTemperature = false;
        }

        // ── Колірна температура ──
        if (est.averageColorTemperature.HasValue)
        {
            // Отримуємо значення з AR і плавно його змінюємо
            smoothTemperature = Mathf.Lerp(smoothTemperature, est.averageColorTemperature.Value, Time.deltaTime * 5f);
            mainLight.colorTemperature = smoothTemperature;
            mainLight.useColorTemperature = true;
        }
    }

    // МЕТОД ДЛЯ ОНОВЛЕННЯ UI
    private void UpdateTemperatureUI()
    {
        if (temperatureColorIndicator != null)
        {
            // Конвертуємо Кельвіни (напр. 6500) у зрозумілий RGB колір
            Color colorFromTemp = Mathf.CorrelatedColorTemperatureToRGB(mainLight.colorTemperature);

            // Задаємо цей колір нашій плашці в UI
            temperatureColorIndicator.color = colorFromTemp;
        }

        if (temperatureText != null)
        {
            // Виводимо цифру, округлену до цілого, і додаємо "K" (Кельвіни)
            temperatureText.text = $"{Mathf.RoundToInt(mainLight.colorTemperature)} K";
        }
    }
}