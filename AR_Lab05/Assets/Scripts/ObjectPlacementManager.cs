using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
public class ObjectPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ObjectSelector selector;
    [Header("Settings")]
    [SerializeField] private int maxObjects = 5;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI limitMessageText;
    
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();
    private Coroutine limitMessageCoroutine;
    void Start()
    {
        // Оновлюємо лічильник на старті (Objects: 0/5)
        UpdateCounterUI();

        // Ховаємо повідомлення про ліміт на початку
        if (limitMessageText != null)
        {
            limitMessageText.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (uiManager.GetCurrentMode() != UIManager.InteractionMode.Place)return;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
            TryPlaceObject(touch.position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            TryPlaceObject(Input.mousePosition);
        }
    }
    private void TryPlaceObject(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            if (placedObjects.Count >= maxObjects)
            {
                if (limitMessageCoroutine != null) StopCoroutine(limitMessageCoroutine);
                limitMessageCoroutine = StartCoroutine(ShowLimitMessageRoutine());
                Destroy(placedObjects[0]);
                placedObjects.RemoveAt(0);
            }
            Pose hitPose = hits[0].pose;
            GameObject prefab = uiManager.GetSelectedPrefab();
            if (prefab != null)
            {
                GameObject newObj = Instantiate(prefab, hitPose.position, hitPose.rotation);
                placedObjects.Add(newObj);
                UpdateCounterUI();
                Debug.Log($"Placed: {prefab.name} at {hitPose.position}");
            }
        }
    }
    private void UpdateCounterUI()
    {
        if (counterText != null)
        {
            counterText.text = $"Objects: {placedObjects.Count}/{maxObjects}";
        }
    }

    // Корутина для показу повідомлення на кілька секунд
    private IEnumerator ShowLimitMessageRoutine()
    {
        if (limitMessageText != null)
        {
            limitMessageText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(2.0f); // Чекаємо 2 секунди
            
            limitMessageText.gameObject.SetActive(false); // Ховаємо повідомлення
        }
    }
}