using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class ObjectPlacementManager2 : MonoBehaviour
{
    [Header("References")]
    //[SerializeField] private GameObject placementPrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Button undoButton;
    [Header("Settings")]
    [SerializeField] private bool useAnchors = true;
    [SerializeField] private int maxObjects = 10;
    [Header("Material Settings")]
    [SerializeField] private Shader preferredShader;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> placedObjects = new List<GameObject>();
    void Start()
    {
        undoButton.onClick.AddListener(UndoLast);
    }
    void Update()

    {
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

            PrimitiveType randomType = GetRandomPrimitiveType();
            GameObject newObject = GameObject.CreatePrimitive(randomType);
            
            float randomScale = UnityEngine.Random.Range(0.05f, 0.15f);
            newObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            
            Renderer renderer = newObject.GetComponent<Renderer>();
            newObject.AddComponent<PlacedObjectBehaviour2>();
            if (renderer != null && preferredShader != null) 
            {
                Material newMat = new Material(preferredShader);
                newMat.color = UnityEngine.Random.ColorHSV();
                renderer.material = newMat;
            }

            if (useAnchors)
            {
                var anchorGO = new GameObject("Anchor_" + randomType);
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
            Debug.Log($"Placed {randomType} #{placedObjects.Count} (scale {randomScale:F2})");
        }
    }

    private PrimitiveType GetRandomPrimitiveType()
    {
        // Випадковий вибір між Кубом (0), Сферою (1) та Циліндром (3)
        int rand = UnityEngine.Random.Range(0, 3);
        return rand switch
        {
            0 => PrimitiveType.Cube,
            1 => PrimitiveType.Sphere,
            _ => PrimitiveType.Cylinder
        };
    }
    public void UndoLast()
    {
        if (placedObjects.Count > 0)
        {
            int lastIndex = placedObjects.Count - 1;
            GameObject lastObj = placedObjects[lastIndex];
            
            Destroy(lastObj);
            placedObjects.RemoveAt(lastIndex);
            
            Debug.Log("Undo: Last object removed.");
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