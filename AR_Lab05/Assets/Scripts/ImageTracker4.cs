using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker4 : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager imageManager;

    [Header("Content Prefabs")]
    [Tooltip("Prefab для кожного зображення.")]
    [SerializeField] private GameObject[] contentPrefabs;

    [Header("Frame Settings")]
    [Tooltip("Префаб Quad з контурним матеріалом")]
    [SerializeField] private GameObject framePrefab;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;

    private Dictionary<string, GameObject> spawnedContent = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> spawnedFrames = new Dictionary<string, GameObject>(); // Словник для рамок
    private Dictionary<string, Vector3> originalScales = new Dictionary<string, Vector3>();
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, bool> trackingStates = new Dictionary<string, bool>();

    void OnEnable()
    {
        if (imageManager != null) imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (imageManager != null) imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added) HandleAddedImage(trackedImage);
        foreach (var trackedImage in args.updated) HandleUpdatedImage(trackedImage);
        foreach (var trackedImage in args.removed) HandleRemovedImage(trackedImage);
    }

    private void HandleAddedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        int index = GetImageIndex(imageName);
        if (index < 0 || index >= contentPrefabs.Length) return;

        string key = imageName ?? trackedImage.referenceImage.guid.ToString();

        // 1. Створюємо основний контент
        GameObject content = Instantiate(contentPrefabs[index], trackedImage.transform);
        spawnedContent[key] = content;
        originalScales[key] = content.transform.localScale;
        content.transform.localScale = Vector3.zero;

        // 2. СТВОРЮЄМО РАМКУ (Завдання 9)
        if (framePrefab != null)
        {
            GameObject frame = Instantiate(framePrefab, trackedImage.transform);
            
            // Масштабуємо Quad під розмір фізичного маркера
            // Quad за замовчуванням 1x1 метр, тому множимо на size
            frame.transform.localScale = new Vector3(trackedImage.size.x, trackedImage.size.y, 1);
            
            frame.transform.localRotation = Quaternion.Euler(90, 0, 0); 
            
            spawnedFrames[key] = frame;
            frame.SetActive(false); // Спочатку вимкнена
        }

        bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
        trackingStates[key] = isTracking;

        if (isTracking)
        {
            RunScaleRoutine(key, content, originalScales[key], true);
            if (spawnedFrames.ContainsKey(key)) spawnedFrames[key].SetActive(true);
        }
    }

    private void HandleUpdatedImage(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name ?? trackedImage.referenceImage.guid.ToString();
        if (!spawnedContent.TryGetValue(key, out GameObject content)) return;

        bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
        if (trackingStates.ContainsKey(key) && trackingStates[key] == isTracking) return;

        trackingStates[key] = isTracking;
        Vector3 targetScale = isTracking ? originalScales[key] : Vector3.zero;

        // Керування рамкою при оновленні
        if (spawnedFrames.TryGetValue(key, out GameObject frame))
        {
            frame.SetActive(isTracking);
            // Оновлюємо розмір, якщо маркер було розпізнано точніше
            frame.transform.localScale = new Vector3(trackedImage.size.x, trackedImage.size.y, 1f);
        }

        RunScaleRoutine(key, content, targetScale, isTracking);
    }

    private void HandleRemovedImage(ARTrackedImage trackedImage)
    {
        string key = trackedImage.referenceImage.name ?? trackedImage.referenceImage.guid.ToString();
        
        // Видалення контенту
        if (spawnedContent.TryGetValue(key, out GameObject content))
        {
            content.transform.SetParent(null);
            StartCoroutine(DestroyAfterAnimation(content));
            spawnedContent.Remove(key);
        }

        // Видалення рамки
        if (spawnedFrames.TryGetValue(key, out GameObject frame))
        {
            Destroy(frame);
            spawnedFrames.Remove(key);
        }

        originalScales.Remove(key);
        trackingStates.Remove(key);
    }

    // --- Допоміжні методи та анімації ---

    private void RunScaleRoutine(string key, GameObject content, Vector3 targetScale, bool isTracking)
    {
        if (activeCoroutines.ContainsKey(key) && activeCoroutines[key] != null)
            StopCoroutine(activeCoroutines[key]);

        if (isTracking && !content.activeSelf) content.SetActive(true);
        activeCoroutines[key] = StartCoroutine(ScaleRoutine(content, targetScale, !isTracking));
    }

    private IEnumerator ScaleRoutine(GameObject target, Vector3 targetScale, bool deactivateOnComplete)
    {
        Vector3 initialScale = target.transform.localScale;
        float timeElapsed = 0f;
        while (timeElapsed < animationDuration)
        {
            target.transform.localScale = Vector3.Lerp(initialScale, targetScale, timeElapsed / animationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        target.transform.localScale = targetScale;
        if (deactivateOnComplete) target.SetActive(false);
    }

    private IEnumerator DestroyAfterAnimation(GameObject target)
    {
        Vector3 initialScale = target.transform.localScale;
        float timeElapsed = 0f;
        while (timeElapsed < animationDuration)
        {
            if (target == null) yield break;
            target.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, timeElapsed / animationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(target);
    }

    private int GetImageIndex(string imageName)
    {
        if (string.IsNullOrEmpty(imageName)) return 0;
        if (imageName.StartsWith("marker_"))
        {
            string numberStr = imageName.Replace("marker_", "");
            if (int.TryParse(numberStr, out int number)) return number - 1;
        }
        return 0;
    }
}