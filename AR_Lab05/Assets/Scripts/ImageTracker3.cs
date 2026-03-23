using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker3 : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager imageManager;

    [Header("Content Prefabs")]
    [Tooltip("Prefab для кожного зображення. Element 0 = marker_01...")]
    [SerializeField] private GameObject[] contentPrefabs;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;

    private Dictionary<string, GameObject> spawnedContent = new Dictionary<string, GameObject>();
    private Dictionary<string, Vector3> originalScales = new Dictionary<string, Vector3>();
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, bool> trackingStates = new Dictionary<string, bool>();

    void OnEnable()
    {
        if (imageManager == null)
        {
            Debug.LogError("ImageTracker: Image Manager не призначено в Inspector!");
            return;
        }
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
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

        GameObject content = Instantiate(
            contentPrefabs[index],
            trackedImage.transform.position,
            trackedImage.transform.rotation,
            trackedImage.transform
        );

        string key = imageName ?? trackedImage.referenceImage.guid.ToString();
        spawnedContent[key] = content;
        
        Vector3 originalScale = content.transform.localScale;
        originalScales[key] = originalScale;
        content.transform.localScale = Vector3.zero;

        bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
        trackingStates[key] = isTracking;

        if (isTracking)
        {
            RunScaleRoutine(key, content, originalScale, true);
        }
    }

    private void HandleUpdatedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        string key = imageName ?? trackedImage.referenceImage.guid.ToString();
        
        if (!spawnedContent.TryGetValue(key, out GameObject content)) return;

        bool isTracking = trackedImage.trackingState == TrackingState.Tracking;

        if (trackingStates.ContainsKey(key) && trackingStates[key] == isTracking)
        {
            return; 
        }

        trackingStates[key] = isTracking;
        Vector3 targetScale = isTracking ? originalScales[key] : Vector3.zero;

        RunScaleRoutine(key, content, targetScale, isTracking);
    }

    private void HandleRemovedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        string key = imageName ?? trackedImage.referenceImage.guid.ToString();
        
        if (spawnedContent.TryGetValue(key, out GameObject content))
        {
            content.transform.SetParent(null);
            
            StartCoroutine(DestroyAfterAnimation(content));
            
            spawnedContent.Remove(key);
            originalScales.Remove(key);
            trackingStates.Remove(key);
            if (activeCoroutines.ContainsKey(key)) activeCoroutines.Remove(key);
        }
    }

    private void RunScaleRoutine(string key, GameObject content, Vector3 targetScale, bool isTracking)
    {
        if (activeCoroutines.ContainsKey(key) && activeCoroutines[key] != null)
        {
            StopCoroutine(activeCoroutines[key]);
        }

        if (isTracking && !content.activeSelf)
        {
            content.SetActive(true);
        }

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
        if (deactivateOnComplete)
        {
            target.SetActive(false);
        }
    }

    private IEnumerator DestroyAfterAnimation(GameObject target)
    {
        Vector3 initialScale = target.transform.localScale;
        float timeElapsed = 0f;

        while (timeElapsed < animationDuration)
        {
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
        
        if (imageManager != null && imageManager.referenceLibrary != null)
        {
            var library = imageManager.referenceLibrary;
            for (int i = 0; i < library.count; i++)
            {
                if (library[i].name == imageName) return i;
            }
        }
        return 0;
    }
}