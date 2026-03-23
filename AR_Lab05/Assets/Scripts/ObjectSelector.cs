using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class ObjectSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [Header("Highlight")]
    [SerializeField] private Color highlightColor = Color.yellow;
    private GameObject selectedObject;
    private Color originalColor;
    private Renderer selectedRenderer;
    void Update()
    {
        if (uiManager.GetCurrentMode() != UIManager.InteractionMode.Select)
            return;
        if (Input.touchCount == 1)
        {

            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
            if (IsPointerOverUI(touch.position)) return;
            TrySelectObject(touch.position);if (IsPointerOverUI(touch.position)) return;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            TrySelectObject(Input.mousePosition);
        }
    }
    public void ChangeSelectedMesh(Mesh newMesh, Material newMaterial)
    {
        if (selectedObject == null) return;
        selectedObject.GetComponent<MeshFilter>().mesh = newMesh;
        MeshCollider collider = selectedObject.GetComponent<MeshCollider>();
        if (collider != null) collider.sharedMesh = newMesh;
        if (selectedRenderer != null)
        {
            selectedRenderer.material = newMaterial;
            originalColor = newMaterial.color; // Оновлюємо базовий колір
            selectedRenderer.material.color = highlightColor; // Повертаємо хайлайт, бо об'єкт все ще виділений
        }
    }

    public void ChangeColorToRed() { ApplyNewColor(Color.red); }
    public void ChangeColorToGreen() { ApplyNewColor(Color.green); }
    public void ChangeColorToBlue() { ApplyNewColor(Color.blue); }
    public void ApplyNewColor(Color newColor)
    {
        if (selectedRenderer != null)
        {
            originalColor = newColor; 
            
            // Якщо хочеш, щоб під час виділення колір одразу змінився на вибраний (ігноруючи highlightColor) - розкоментуй наступний рядок:
            selectedRenderer.material.color = originalColor;
            
            Debug.Log($"Color changed to {newColor}");
        }
    }
    private void TrySelectObject(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            GameObject hitObject = hit.collider.gameObject;
            DeselectCurrent();
            selectedObject = hitObject;
            selectedRenderer = hitObject.GetComponent<Renderer>();
            if (selectedRenderer != null)
            {
                originalColor = selectedRenderer.material.color;
                selectedRenderer.material.color = highlightColor;
            }
            Debug.Log($"Selected: {hitObject.name}");
        }
        else { DeselectCurrent(); }
    }
    private void DeselectCurrent()
    {
        if (selectedRenderer != null)
            selectedRenderer.material.color = originalColor;
        selectedObject = null;
        selectedRenderer = null;
    }
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = screenPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    public GameObject GetSelectedObject() { return selectedObject; }
}