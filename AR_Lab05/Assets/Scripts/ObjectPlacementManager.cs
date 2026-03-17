using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using System.Collections;
public class ObjectPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ObjectSelector selector;
    [Header("Settings")]
    [SerializeField] private int maxObjects = 10;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();
    
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
                Destroy(placedObjects[0]);
                placedObjects.RemoveAt(0);
            }
            Pose hitPose = hits[0].pose;
            GameObject prefab = uiManager.GetSelectedPrefab();
            if (prefab != null)
            {
                GameObject newObj = Instantiate(prefab, hitPose.position, hitPose.rotation);
                placedObjects.Add(newObj);
                Debug.Log($"Placed: {prefab.name} at {hitPose.position}");
            }
        }
    }
}