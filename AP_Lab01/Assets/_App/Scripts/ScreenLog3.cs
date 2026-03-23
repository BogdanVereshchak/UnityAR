using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ScreenLog3 : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("TextMeshPro елемент для відображення логів")]
    [SerializeField] private TextMeshProUGUI logText;
    
    [Header("Налаштування")]
    [Tooltip("Максимальна кількість рядків на екрані")]
    [SerializeField] private int maxLines = 20;
    
    [Tooltip("Показувати час повідомлення")]
    [SerializeField] private bool showTimestamp = true;

    private Queue<string> logQueue = new Queue<string>();
    private static ScreenLog3 instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
        if (instance == this) instance = null;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color;

        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                color = "#FF4444"; // червоний
                break;
            case LogType.Warning:
                color = "#FFAA00"; // жовтий
                break;
            default:
                color = "#FFFFFF"; // білий
                break;
        }

        string timestamp = showTimestamp ? $"[{DateTime.Now:mm:ss.fff}] " : "";
        string formattedLog = $"<color={color}>{timestamp}{logString}</color>";

        logQueue.Enqueue(formattedLog);

        while (logQueue.Count > maxLines)
            logQueue.Dequeue();

        if (logText != null)
            logText.text = string.Join("\n", logQueue);
    }

    public void ClearLog()
    {
        logQueue.Clear();
        if (logText != null) logText.text = "";
    }

    public static void Log(string message)
    {
        Debug.Log(message);
    }
}