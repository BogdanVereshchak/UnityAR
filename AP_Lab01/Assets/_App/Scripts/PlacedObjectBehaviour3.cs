using UnityEngine;
using TMPro;

public class PlacedObjectBehaviour3 : MonoBehaviour
{
    [Header("Налаштування обертання (Варіант B)")]
    [Tooltip("Об'єкт 3D-моделі (куб/сфера), який буде обертатись")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Налаштування Billboard та UI (Варіант 8)")]
    [Tooltip("Canvas, який має постійно дивитися на камеру")]
    [SerializeField] private Transform canvasTransform;
    [Tooltip("Текст для виведення відстані")]
    [SerializeField] private TextMeshProUGUI distanceText;

    private Camera mainCam;
    private Renderer objRenderer;
    private Color originalColor;
    private bool isProximityTriggered = false;

    void Start()
    {
        mainCam = Camera.main;

        // Шукаємо Renderer на моделі, щоб потім міняти їй колір
        if (modelTransform != null)
        {
            objRenderer = modelTransform.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                originalColor = objRenderer.material.color;
            }
        }
    }

    void Update()
    {
        if (mainCam == null) return;

        // --- ВАРІАНТ B: Обертання моделі навколо осі Y ---
        if (modelTransform != null)
        {
            modelTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        // --- ВАРІАНТ 8: Billboard (поворот Canvas до камери) ---
        if (canvasTransform != null)
        {
            // Canvas дивиться рівно на камеру, щоб текст не був віддзеркаленим
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - mainCam.transform.position);
        }

        // --- ВАРІАНТ 8: Відстань до камери ---
        float distance = Vector3.Distance(transform.position, mainCam.transform.position);
        
        if (distanceText != null)
        {
            distanceText.text = $"{distance:F2} m";
        }

        // --- ВАРІАНТ 8: Proximity (< 0.5m) ---
        if (distance < 0.5f)
        {
            if (!isProximityTriggered)
            {
                isProximityTriggered = true;
                if (objRenderer != null) objRenderer.material.color = Color.red;
                Debug.Log("[PROXIMITY]");
            }
        }
        else
        {
            // Повертаємо оригінальний колір, якщо відійшли далі
            if (isProximityTriggered)
            {
                isProximityTriggered = false;
                if (objRenderer != null) objRenderer.material.color = originalColor;
            }
        }
    }
}