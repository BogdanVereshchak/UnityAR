using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectPlacementManager4 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject placementPrefab; 
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager; 

    [Header("UI Components")]
    [SerializeField] private Image uiIndicator; // Індикатор (зелений/червоний)

    [Header("Settings")]
    [SerializeField] private bool useAnchors = true;
    [SerializeField] private int maxObjects = 10;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();

    void Update()
    {
        UpdateUIIndicator();

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

    private void UpdateUIIndicator()
    {
        if (uiIndicator == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARPlane plane = planeManager.GetPlane(hits[0].trackableId);
            
            if (plane != null && (plane.alignment == PlaneAlignment.HorizontalUp || plane.alignment == PlaneAlignment.HorizontalDown))
            {
                uiIndicator.color = Color.green; 
            }
            else
            {
                uiIndicator.color = Color.red; 
            }
        }
        else
        {
            uiIndicator.color = Color.red; 
        }
    }

    private void TryPlaceObject(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            ARPlane plane = planeManager.GetPlane(hits[0].trackableId);
            if (plane == null) return;
            if (placedObjects.Count >= maxObjects)
            {
                Debug.Log($"[LIMIT] Maximum {maxObjects} objects reached");
                return;
            }

            Pose hitPose = hits[0].pose;
            GameObject newObject = Instantiate(placementPrefab);

            if (useAnchors)
            {
                var anchorGO = new GameObject("Anchor_PlacedObject");
                anchorGO.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                anchorGO.AddComponent<ARAnchor>();
                newObject.transform.SetParent(anchorGO.transform);
                newObject.transform.localPosition = Vector3.zero;
                newObject.transform.localRotation = Quaternion.identity;
                placedObjects.Add(anchorGO);
            }
            else
            {
                newObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                placedObjects.Add(newObject);
            }
        }
    }

    public void ClearAllObjects()
    {
        foreach (var obj in placedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        placedObjects.Clear();
        Debug.Log("All objects cleared.");
    }
}