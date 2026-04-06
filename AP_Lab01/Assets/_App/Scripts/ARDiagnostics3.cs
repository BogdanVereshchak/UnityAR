using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDiagnostics3 : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 2.0f; 
    private float nextUpdateTime;

    private float fps;
    private int frameCount;
    private float fpsTimer;

    void Update()
    {
        CalculateFPS();

        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            PrintDiagnostics();
        }
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
        // [PERF] Тег для продуктивності
        string fpsColor = fps >= 30 ? "" : " [LOW]";
        Debug.Log($"[PERF] FPS: {fps:F1}{fpsColor} | Frame: {Time.frameCount}");

        // [AR] Тег для AR станів
        if (planeManager != null)
        {
            int planeCount = 0;
            foreach (var plane in planeManager.trackables)
                if (plane.gameObject.activeSelf) planeCount++;
            
            Debug.Log($"[AR] Planes: {planeCount} | State: {ARSession.state}");
        }

        // [UI] Використовуємо цей тег для загальної статистики об'єктів
        int activeGOs = FindObjectsOfType<GameObject>(false).Length;
        Debug.Log($"[UI] Active Objects: {activeGOs}");

        // [PHYSICS] Можна додати перевірку колайдерів (як приклад)
        int colliders = FindObjectsOfType<Collider>().Length;
        Debug.Log($"[PHYSICS] Total Colliders in scene: {colliders}");
    }

    public void TakeScreenshot()
    {
        // Новий формат часу за завданням: HH:mm:ss
        string timestamp = System.DateTime.Now.ToString("HH-mm-ss");
        string filename = $"Snap_{timestamp}.png";
        ScreenCapture.CaptureScreenshot(filename);
        Debug.Log($"[UI] Screenshot saved at {System.DateTime.Now:HH:mm:ss}");
    }
}