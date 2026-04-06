using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectPlacementManager5 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject placementPrefab; 
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager; 

    [Header("UI")]
    [SerializeField] private Image uiIndicator;

    [Header("Settings")]
    [SerializeField] private int maxObjects = 10;

    private List<ARRaycastHit> arHits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();

    void Update()
    {
        UpdateUIIndicator();

        if (GetInput(out Vector2 touchPosition))
        {
            // 1. Спершу перевіряємо, чи ми тапнули по існуючому об'єкту
            if (TryInteractWithObject(touchPosition))
            {
                return; // Якщо тапнули по об'єкту, не ставимо новий
            }

            // 2. Якщо не тапнули по об'єкту, пробуємо поставити новий на площину
            TryPlaceObject(touchPosition);
        }
    }

    private bool GetInput(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            touchPosition = Input.mousePosition;
            return true;
        }
        touchPosition = Vector2.zero;
        return false;
    }

    private bool TryInteractWithObject(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // Використовуємо звичайний Physics.Raycast для об'єктів
        if (Physics.Raycast(ray, out hit))
        {
            PlacedObjectBehaviour4 cycler = hit.collider.GetComponentInParent<PlacedObjectBehaviour4>();
            if (cycler != null)
            {
                cycler.TriggerNextColor();
                return true;
            }
        }
        return false;
    }

    private void TryPlaceObject(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, arHits, TrackableType.PlaneWithinPolygon))
        {
            if (placedObjects.Count >= maxObjects) return;

            Pose hitPose = arHits[0].pose;
            GameObject newObject = Instantiate(placementPrefab, hitPose.position, hitPose.rotation);
            
            // Додаємо ARAnchor для стабільності
            newObject.AddComponent<ARAnchor>();
            placedObjects.Add(newObject);
        }
    }

    private void UpdateUIIndicator()
    {
        if (uiIndicator == null) return;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        uiIndicator.color = raycastManager.Raycast(screenCenter, arHits, TrackableType.PlaneWithinPolygon) ? Color.green : Color.red;
    }
}