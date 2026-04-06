using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

public class ScreenLog4 : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private TextMeshProUGUI counterText; // Для відображення статистики

    [Header("Settings")]
    [SerializeField] private int maxLines = 15;
    private string currentFilter = "ALL";

    // Словник кольорів для тегів
    private Dictionary<string, string> tagColors = new Dictionary<string, string>()
    {
        { "[PHYSICS]", "#FF5555" }, // Червонуватий
        { "[AR]", "#55FF55" },      // Зелений
        { "[UI]", "#5555FF" },      // Синій
        { "[PERF]", "#FFFF55" }     // Жовтий
    };

    // Лічильники
    private Dictionary<string, int> tagCounters = new Dictionary<string, int>()
    {
        { "[PHYSICS]", 0 }, { "[AR]", 0 }, { "[UI]", 0 }, { "[PERF]", 0 }
    };

    private List<LogEntry> allLogs = new List<LogEntry>();

    struct LogEntry
    {
        public string originalText;
        public string formattedText;
        public string tag;
    }

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string detectedTag = "NONE";
        string processedMessage = logString;

        // 1. Парсинг тегів та фарбування
        foreach (var tag in tagColors.Keys)
        {
            if (logString.Contains(tag))
            {
                detectedTag = tag;
                tagCounters[tag]++;
                // Замінюємо звичайний тег на кольоровий
                processedMessage = logString.Replace(tag, $"<color={tagColors[tag]}>{tag}</color>");
                break;
            }
        }

        // 2. Форматування (Час HH:mm:ss + повідомлення)
        string timeStr = DateTime.Now.ToString("HH:mm:ss");
        string finalEntry = $"[{timeStr}] {processedMessage}";

        allLogs.Add(new LogEntry { 
            originalText = logString, 
            formattedText = finalEntry, 
            tag = detectedTag 
        });

        UpdateDisplay();
        UpdateCounters();
    }

    private void UpdateDisplay()
    {
        if (logText == null) return;

        // Фільтруємо список
        var filtered = allLogs
            .Where(x => currentFilter == "ALL" || x.tag == currentFilter)
            .Select(x => x.formattedText)
            .Reverse() // Нові зверху
            .Take(maxLines);

        logText.text = string.Join("\n", filtered);
    }

    private void UpdateCounters()
    {
        if (counterText == null) return;
        counterText.text = $"AR: {tagCounters["[AR]"]} | PERF: {tagCounters["[PERF]"]} | UI: {tagCounters["[UI]"]} | PHYS: {tagCounters["[PHYSICS]"]}";
    }

    // Метод для кнопок фільтрації
    public void SetFilter(string filter)
    {
        // Очікує "ALL", "[AR]", "[PERF]" тощо
        currentFilter = filter;
        UpdateDisplay();
        Debug.Log($"[UI] Filter changed to: {filter}");
    }

    public void ClearLog()
    {
        allLogs.Clear();
        foreach (var key in tagCounters.Keys.ToList()) tagCounters[key] = 0;
        UpdateDisplay();
        UpdateCounters();
    }
}