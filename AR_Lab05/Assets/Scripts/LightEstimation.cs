using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LightEstimation : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARCameraManager cameraManager;
    
    [Header("Scene Light")]
    [SerializeField] private Light mainLight;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    [Header("Data Recording & Graph")]
    [Tooltip("Лінія для малювання графіка (LineRenderer)")]
    [SerializeField] private LineRenderer graphLine;
    [Tooltip("Як часто записувати дані (в секундах)")]
    [SerializeField] private float recordInterval = 0.5f;
    [Tooltip("Масштаб графіка по X та Y")]
    [SerializeField] private Vector2 graphScale = new Vector2(0.1f, 1f);
    [Tooltip("Скільки останніх точок показувати на екрані (вікно графіка)")]
    [SerializeField] private int maxVisiblePoints = 20;
    [SerializeField] private float backupBrightnessValue=1.2f;

    private float smoothBrightness = 1f;
    private float currentSmoothValue; 
    private float smoothTemperature = 6500f;
    private int frameCount = 0;

    private List<Vector2> recordedData = new List<Vector2>(); // X = час, Y = яскравість
    private float lastRecordTime = 0f;
    private float startTime = 0f;

    void OnEnable()
    {
        if (cameraManager == null || mainLight == null)
        {
            Debug.LogError("[LIGHT] Camera Manager або Main Light не призначені!");
            enabled = false;
            return;
        }
        cameraManager.frameReceived += OnFrameReceived;
        startTime = Time.time;
        Debug.Log("[LIGHT] ✓ LightEstimation підписався");
    }

    void OnDisable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= OnFrameReceived;
    }

    private void OnFrameReceived(ARCameraFrameEventArgs args)
    {
        frameCount++;
        var est = args.lightEstimation;

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

        if (est.mainLightDirection.HasValue)
            mainLight.transform.rotation = Quaternion.LookRotation(est.mainLightDirection.Value);

        if (est.mainLightColor.HasValue)
        {
            mainLight.color = est.mainLightColor.Value;
        }
        else if (est.colorCorrection.HasValue)
        {
            mainLight.color = est.colorCorrection.Value;
            mainLight.useColorTemperature = false;
        }

        if (est.averageColorTemperature.HasValue)
        {
            smoothTemperature = Mathf.Lerp(smoothTemperature, est.averageColorTemperature.Value, Time.deltaTime * 5f);
            mainLight.colorTemperature = smoothTemperature;
            mainLight.useColorTemperature = true;
        }
        if (smoothBrightness == 1) {
            float targetValue = backupBrightnessValue * Random.Range(0.5f, 1.2f);
            currentSmoothValue = Mathf.Lerp(currentSmoothValue, targetValue, Time.deltaTime * 5f);
            RecordAndDrawGraph(currentSmoothValue);
        
        }
        else RecordAndDrawGraph(smoothBrightness);
    }

    private void RecordAndDrawGraph(float currentBrightness)
    {
        if (Time.time - lastRecordTime >= recordInterval)
        {
            float elapsedTime = Time.time - startTime;
            recordedData.Add(new Vector2(elapsedTime, currentBrightness));
            lastRecordTime = Time.time;

            UpdateGraph();
        }
    }

    private void UpdateGraph()
    {
        if (graphLine == null || recordedData.Count == 0) return;

        int totalPoints = recordedData.Count;
        int startIndex = Mathf.Max(0, totalPoints - maxVisiblePoints);
        int displayCount = totalPoints - startIndex;

        graphLine.positionCount = displayCount;

        for (int i = 0; i < displayCount; i++)
        {
            Vector2 dataPoint = recordedData[startIndex + i];
            
            float drawX = i * graphScale.x; 
            float drawY = dataPoint.y * graphScale.y;

            graphLine.SetPosition(i, new Vector3(drawX, drawY, 0f));
        }
    }

     public void ExportToCSV()
    {
         string filePath = Path.Combine(Application.persistentDataPath, "BrightnessLog.csv");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Time(s),Brightness"); 

        foreach (var data in recordedData)
        {
            sb.AppendLine($"{data.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{data.y.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}");
        }

        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"[LIGHT] ✓ Дані успішно експортовано у CSV! Шлях: {filePath}");
    }
}