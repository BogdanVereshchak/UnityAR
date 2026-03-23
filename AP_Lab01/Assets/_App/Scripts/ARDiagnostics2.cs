using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDiagnostics2 : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Діагностика (Логи)")]
    [Tooltip("Інтервал оновлення діагностики (секунди)")]
    [SerializeField] private float updateInterval = 3.0f; // Змінено на 3 секунди
    private float nextUpdateTime;

    [Header("Тестування ScreenLog (Штучні помилки)")]
    [SerializeField] private bool autoSendError = false;
    [SerializeField] private bool autoSendWarning = false;
    [Tooltip("Інтервал відправки тестових повідомлень (секунди)")]
    [SerializeField] private float testInterval = 5.0f;
    private float nextTestTime;

    private float fps;
    private int frameCount;
    private float fpsTimer;

    void Update()
    {
        CalculateFPS();

        // 1. Основна діагностика
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            PrintDiagnostics();
        }

        // 2. Автоматичне тестування помилок/попереджень
        if (Time.time >= nextTestTime)
        {
            nextTestTime = Time.time + testInterval;
            HandleTestLogs();
        }
    }

    private void HandleTestLogs()
    {
        if (autoSendError) 
            Debug.LogError($"[TEST ERROR] Авто-помилка");

        if (autoSendWarning) 
            Debug.LogWarning($"[TEST WARNING] Попередження");
    }

    private void CalculateFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 0.5f)
        {
            fps = frameCount / fpsTimer;
            frameCount = 0;
            fpsTimer = 0f;
        }
    }

    private void PrintDiagnostics()
    {
        // Додано номер кадру
        Debug.Log($"Frame {Time.frameCount}");

        Debug.Log($"[AR] Session State: {ARSession.state}");

        string fpsColor = fps >= 30 ? "" : " [LOW]";
        Debug.Log($"[PERF] FPS: {fps:F1}{fpsColor}");

        if (planeManager != null)
        {
            int planeCount = 0;
            foreach (var plane in planeManager.trackables)
            {
                if (plane.gameObject.activeSelf)
                    planeCount++;
            }
            Debug.Log($"[AR] Planes detected: {planeCount}");
        }

        long memoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
        Debug.Log($"[MEM] Managed memory: {memoryMB} MB");

        // Розширена статистика (GameObject, MeshRenderer, Texture)
        int activeGameObjects = FindObjectsOfType<GameObject>(false).Length;
        int activeMeshRenderers = FindObjectsOfType<MeshRenderer>(false).Length;
        int textureCount = Resources.FindObjectsOfTypeAll<Texture>().Length;

        Debug.Log($"[STATS] GOs: {activeGameObjects} | Meshes: {activeMeshRenderers} | Textures: {textureCount}");
    }

    // Метод для створення скріншоту
    public void TakeScreenshot()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"Diagnostic_{timestamp}.png";
        
        ScreenCapture.CaptureScreenshot(filename);
        Debug.Log($"[SCREENSHOT] Збережено: {filename}");
    }
}