using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;

public class ScreenLog2 : MonoBehaviour
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
    private static ScreenLog2 instance;
    
    
    // Нові змінні для завдань
    private int errorCount = 0;
    private Coroutine blinkRoutine;

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
        string bgColor;
        string textColor = "#000000"; // Чорний текст краще читається на кольоровому тлі
        bool isError = false;

        // Визначаємо колір фону (<mark>) за типом
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                errorCount++;
                isError = true;
                bgColor = "#FF4444AA"; // Напівпрозорий червоний
                textColor = "#FFFFFF"; // Білий текст для помилок
                break;
            case LogType.Warning:
                bgColor = "#FFAA00AA"; // Напівпрозорий жовтий
                break;
            default:
                bgColor = "#FFFFFFAA"; // Напівпрозорий білий
                break;
        }

        // 1. Форматування часу (Завдання: HH:mm:ss)
        string timestamp = showTimestamp ? $"[{DateTime.Now:mm:ss.fff}] " : "";

        // 2. Автоматичний [ALERT] (Завдання: якщо помилок > 10)
        string alertPrefix = (errorCount > 10 && isError) ? "<b><size=150%>[ALERT]</size></b> " : "";

        // 3. Формування рядка з кольоровим тлом (Завдання: <mark>)
        // Використовуємо <mark> для фону та <color> для тексту
        string formattedLog = $"<mark={bgColor}><color={textColor}>{timestamp}{alertPrefix}{logString}</color></mark>";

        logQueue.Enqueue(formattedLog);

        while (logQueue.Count > maxLines)
            logQueue.Dequeue();

        if (logText != null)
            logText.text = string.Join("\n", logQueue);

        // 4. Мигання при помилці (Завдання: 3 рази, 0.3с)
        if (isError)
        {
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = StartCoroutine(BlinkTextRoutine());
        }
    }

    // Корутина для мигання тексту
    private IEnumerator BlinkTextRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            logText.alpha = 0.2f; // Пригасання
            yield return new WaitForSeconds(0.3f);
            logText.alpha = 1.0f; // Повна яскравість
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void ClearLog()
    {
        logQueue.Clear();
        errorCount = 0;
        if (logText != null) logText.text = "";
    }

    public static void Log(string message)
    {
        Debug.Log(message);
    }
}