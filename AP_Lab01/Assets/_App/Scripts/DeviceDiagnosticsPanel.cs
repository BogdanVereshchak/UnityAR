using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DeviceDiagnosticsPanel : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Панель, яка буде згортатися/розгортатися")]
    [SerializeField] private GameObject contentPanel;
    [Tooltip("Кнопка для згортання/розгортання панелі")]
    [SerializeField] private Button toggleButton;

    [Header("UI Text Elements")]
    [Tooltip("Текст для показу OS, Model, RAM, Status")]
    [SerializeField] private TextMeshProUGUI systemInfoText;
    [Tooltip("Текст для показу Warning (з часовою міткою mm:ss.fff)")]
    [SerializeField] private TextMeshProUGUI warningText;

    [Header("Battery UI (Progress Bar)")]
    [Tooltip("Зображення типу 'Filled' для відображення рівня заряду")]
    [SerializeField] private Image batteryProgressBar;
    [Tooltip("Текст для показу відсотків батареї")]
    [SerializeField] private TextMeshProUGUI batteryPercentageText;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 5.0f;
    private float nextUpdateTime;

    void Start()
    {
        // Підписуємо кнопку на подію згортання/розгортання
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }

        // Очищаємо текст попередження на старті
        if (warningText != null) warningText.text = "";

        // Примусово оновлюємо дані при старті
        UpdateDiagnostics();
    }

    void Update()
    {
        // Оновлення кожні 5 секунд
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            UpdateDiagnostics();
        }
    }

    private void UpdateDiagnostics()
    {
        // 1. Збір системної інформації
        string os = SystemInfo.operatingSystem;
        string model = SystemInfo.deviceModel;
        int ramMB = SystemInfo.systemMemorySize;
        BatteryStatus batteryStatus = SystemInfo.batteryStatus;

        if (systemInfoText != null)
        {
            systemInfoText.text = $"<b>Model:</b> {model}\n" +
                                  $"<b>OS:</b> {os}\n" +
                                  $"<b>RAM:</b> {ramMB} MB\n" +
                                  $"<b>Status:</b> {batteryStatus}";
        }

        // 2. Логіка Батареї
        float batteryLevel = SystemInfo.batteryLevel;

        // SystemInfo.batteryLevel повертає -1, якщо пристрій не підтримує запит (наприклад, Editor або XR Sim)
        if (batteryLevel < 0f)
        {
            if (batteryPercentageText != null) batteryPercentageText.text = "N/A";
            if (batteryProgressBar != null) batteryProgressBar.fillAmount = 0f;
            if (warningText != null) warningText.text = ""; // Пропускаємо Warning
        }
        else
        {
            // Відображаємо % та заповнюємо прогрес-бар
            if (batteryPercentageText != null) batteryPercentageText.text = $"{(batteryLevel * 100):F0}%";
            if (batteryProgressBar != null) batteryProgressBar.fillAmount = batteryLevel;

            // Перевірка на низький заряд (< 20%)
            if (batteryLevel < 0.2f && batteryStatus != BatteryStatus.Charging)
            {
                if (warningText != null)
                {
                    // Додаємо часову мітку у форматі mm:ss.fff
                    string timeStamp = DateTime.Now.ToString("mm:ss.fff");
                    warningText.text = $"[{timeStamp}] <color=red>WARNING: Low Battery!</color>";
                }
            }
            else
            {
                // Ховаємо попередження, якщо все ок
                if (warningText != null) warningText.text = "";
            }
        }
    }

    // Метод для кнопки (Згорнути/Розгорнути)
    private void TogglePanel()
    {
        if (contentPanel != null)
        {
            contentPanel.SetActive(!contentPanel.activeSelf);
        }
    }
}