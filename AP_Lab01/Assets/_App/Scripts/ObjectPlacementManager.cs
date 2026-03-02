using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class ObjectPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject placementPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [Header("Settings")]
    [SerializeField] private bool useAnchors = true;
    [SerializeField] private int maxObjects = 10;

    [Header("ActivityLog")]
    [SerializeField] private ScrollRect logs;
    [SerializeField] private TextMeshProUGUI textLogPrefab;
    [SerializeField] private UnityEngine.UI.Button clearButton;
    [SerializeField] private UnityEngine.UI.Button muteButton;
    [SerializeField] private AudioClip sound;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();
    private List<TextMeshProUGUI> logEntries = new List<TextMeshProUGUI>();
    private AudioSource audioSource;
    private bool playSound = true;
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.clip = sound;
        audioSource.playOnAwake = false;

        muteButton.onClick.AddListener(OnMuteClick);
        clearButton.onClick.AddListener(OnClearClick);
        Debug.Log("AR додаток запущено!");
        Debug.LogWarning("Це попередження для тесту");
        Debug.LogError("Це помилка для тесту");

    }
    void OnMuteClick()
    {
        playSound = !playSound;
    }
    void OnClearClick()
    {
        foreach (var log in logEntries) Destroy(log);
        logEntries.Clear();
    }

    private void AddLog(string message)
    {
        if (logs == null || textLogPrefab == null) return;
        TextMeshProUGUI newLog = Instantiate(textLogPrefab, logs.content);
        newLog.text = message;

        logEntries.Add(newLog);
        Canvas.ForceUpdateCanvases();
        logs.verticalNormalizedPosition = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.acceleration.sqrMagnitude > 2.5 * 2.5)
        {
            ClearAllObjects();
            return;
        }
        Vector2 inputPosition;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPosition = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
        }
        else
        {
            return;
        }

        TryPlaceObject(inputPosition);
    }
    private void TryPlaceObject(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            if (placedObjects.Count >= maxObjects)
            {
                Debug.Log($"[LIMIT] Maximum {maxObjects} objects reached");
                return;
            }
            Pose hitPose = hits[0].pose;
            GameObject newObject;
            if (useAnchors)
            {
                var anchorGO = new GameObject("Anchor");
                anchorGO.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                anchorGO.AddComponent<ARAnchor>();
                newObject = Instantiate(placementPrefab, hitPose.position, hitPose.rotation, anchorGO.transform);
            }
            else
            {
                newObject = Instantiate(placementPrefab, hitPose.position, hitPose.rotation);
            }
            if (playSound)
            {
                audioSource.PlayOneShot(sound);
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            }

            AddLog(DateTime.Now.ToString("HH:mm:ss") + $" - Placed at {hitPose.position}");
            placedObjects.Add(newObject);
            Debug.Log($"Object #{placedObjects.Count} placed at:{hitPose.position}");
        }
    }
    public void ClearAllObjects()
    {
        foreach (var obj in placedObjects)
        {
            if (obj == null) continue;
            if (obj.transform.parent != null)
                Destroy(obj.transform.parent.gameObject);
            else
                Destroy(obj);
        }
        placedObjects.Clear();
    }
}