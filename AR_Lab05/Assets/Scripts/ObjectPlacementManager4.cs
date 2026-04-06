using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ObjectPlacementManager4 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private UIManager uiManager; 

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {

        if (uiManager.GetCurrentMode() != UIManager.InteractionMode.Place) return;

        Vector2 inputPosition;
        int fingerId = -1;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPosition = Input.GetTouch(0).position;
            fingerId = Input.GetTouch(0).fingerId;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
        }
        else return;

        // 2. ПЕРЕВІРКА НА UI
        if (IsPointerOverUI(fingerId)) return;

        // 3. ПЕРЕВІРКА НА ІСНУЮЧІ ОБ'ЄКТИ:
        // Якщо ми тапнули по вже існуючому кубу, не треба ставити під нього ще один.
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        if (Physics.Raycast(ray, out RaycastHit physicsHit))
        {
            // Якщо промінь влучив у колайдер (об'єкт), зупиняємо виконання
            return; 
        }

        // 4. РОЗСТАНОВКА (AR Raycast)
        if (raycastManager.Raycast(inputPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            GameObject prefabToPlace = uiManager.GetSelectedPrefab();
            if (prefabToPlace == null) return;

            Pose hitPose = hits[0].pose;
            GameObject obj = Instantiate(prefabToPlace, hitPose.position, hitPose.rotation);

            // Додаємо ARAnchor, щоб об'єкт не "плив"
            obj.AddComponent<ARAnchor>();

            // Налаштовуємо текст мітки
            // Зверни увагу: якщо ти назвав скрипт ObjectLabel9, то змініть назву тут
            ObjectLabel label = obj.GetComponentInChildren<ObjectLabel>();
            if (label != null)
            {
                string fullName = prefabToPlace.name;
                string nameBeforeUnderscore = fullName.Split('_')[0];
                label.SetText(nameBeforeUnderscore);
            }
        }
    }

    private bool IsPointerOverUI(int fingerId)
    {
        if (EventSystem.current == null) return false;
        return fingerId >= 0 ? EventSystem.current.IsPointerOverGameObject(fingerId) : EventSystem.current.IsPointerOverGameObject();
    }
}