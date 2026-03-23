using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class ObjectPlacementManager2 : MonoBehaviour
{
    [SerializeField] private GameObject placementPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private int maxObjects = 10;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();
    void Update()
    {
    Vector2 inputPosition;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase ==TouchPhase.Began)
            inputPosition = Input.GetTouch(0).position;
        else if (Input.GetMouseButtonDown(0))
            inputPosition = Input.mousePosition;
        else
            return;
        if (raycastManager.Raycast(inputPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            if (placedObjects.Count >= maxObjects) return;
            Pose hitPose = hits[0].pose;
            var obj = Instantiate(placementPrefab, hitPose.position,
            hitPose.rotation);
            placedObjects.Add(obj);
        }
    }
}
