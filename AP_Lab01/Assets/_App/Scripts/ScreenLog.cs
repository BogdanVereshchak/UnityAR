using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;
public class ScreenLog : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("TextMeshPro елемент для відображення логів")]
    [SerializeField] private TextMeshProUGUI logText;
    [Header("Налаштування")]
    [Tooltip("Максимальна кількість рядків на екрані")]
    [SerializeField] private int maxLines = 20;
    // Звук для помилки
    [SerializeField] private AudioClip errorClip;
    // Джерело де звук буде відтворюватися
    [SerializeField] public AudioSource source;
    // Прямокутник, червона смужка
    [SerializeField] public Image errorIcon;
    [SerializeField] private TextMeshProUGUI errorCounterText;
    [SerializeField] public bool isMuted = false;
    [Tooltip("Показувати час повідомлення")]
    [SerializeField] private bool showTimestamp = true;
    private Queue<string> logQueue = new Queue<string>();
    private static ScreenLog instance;
    private Coroutine fadeRoutine;
    private int errorCount = 0;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Application.logMessageReceived += HandleLog;
        if (errorIcon != null)
        {
            Color c = errorIcon.color;
            c.a = 0f;
            errorIcon.color = c;
        }
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
                errorCount++;
                color = "#FF4444"; // червоний
                if (errorCounterText != null) errorCounterText.text = $"Errors: {errorCount}";
                if (!isMuted && source != null && errorClip != null)
                {
                    source.PlayOneShot(errorClip);
                }
                if (errorIcon != null)
                {
                    if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                    fadeRoutine = StartCoroutine(FadeIconRoutine());
                }

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

    private IEnumerator FadeIconRoutine()
    {
        // Показуємо (Альфа 1)
        Color c = errorIcon.color;
        c.a = 1f;
        errorIcon.color = c;

        yield return new WaitForSeconds(1.0f); // Скільки часу іконка горить яскраво

        // Плавно гасимо
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            errorIcon.color = c;
            yield return null;
        }
    }

    public void ClearLog()
    {
        logQueue.Clear();
        if (logText != null)
            logText.text = "";
    }

    public static void Log(string message)
    {
        Debug.Log(message);
    }
    public void ToggleMute()
    {
        isMuted = !isMuted;
        // Опціонально: міняй колір кнопки або текст тут
    }
}
