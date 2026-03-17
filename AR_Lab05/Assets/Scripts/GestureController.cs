using UnityEngine;
public class GestureController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectSelector objectSelector;
    [SerializeField] private UIManager uiManager;
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.02f;
    [SerializeField] private float maxScale = 2.0f;
    [SerializeField] private float scrollScaleSpeed = 0.1f;
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [Header("Height Settings")]
    [SerializeField] private float liftSpeed = 0.001f;
    [SerializeField] private float keyboardLiftSpeed = 0.5f;
    [SerializeField] private float minHeight = -0.5f;
    [SerializeField] private float maxHeight = 2.0f;
    private float initialPinchDistance;
    private Vector3 initialScale;
    void Update()
    {
        if (uiManager.GetCurrentMode() != UIManager.InteractionMode.Select) return;
        GameObject selected = objectSelector.GetSelectedObject();
        if (selected == null) return;
        if (Input.touchCount == 2) { HandlePinchToScale(selected); }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                HandleManipulation(selected, touch);
            }
        }
        else if (Input.touchCount == 0)
        {
            float verticalInput = 0;
            if (Input.GetKey(KeyCode.UpArrow)) verticalInput = 1;
            else if (Input.GetKey(KeyCode.DownArrow)) verticalInput = -1;

            if (verticalInput != 0)
            {
                Vector3 currentPos = selected.transform.position;
                float newY = Mathf.Clamp(currentPos.y + verticalInput * keyboardLiftSpeed * Time.deltaTime, minHeight, maxHeight);
                selected.transform.position = new Vector3(currentPos.x, newY, currentPos.z);
            }
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f) HandleMouseScale(selected, scroll);
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * 10f;
                selected.transform.Rotate(Vector3.up, -mouseX, Space.World);
            }
        }
    }
    private void HandlePinchToScale(GameObject target)
    {
        Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
        float dist = Vector2.Distance(t0.position, t1.position);
        if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
        { initialPinchDistance = dist; initialScale = target.transform.localScale; return; }
        if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
        {
            if (initialPinchDistance <= 0f) return;
            float factor = dist / initialPinchDistance;
            float clamped = Mathf.Clamp((initialScale * factor).x, minScale, maxScale);
            target.transform.localScale = Vector3.one * clamped;
        }
    }
    private void HandleManipulation(GameObject target, Touch touch)
    {
        float rotationAmount = -touch.deltaPosition.x * rotationSpeed;
        target.transform.Rotate(Vector3.up, rotationAmount, Space.World);
        float liftAmount = touch.deltaPosition.y * liftSpeed;
        Vector3 currentPos = target.transform.position;
        float newY = Mathf.Clamp(currentPos.y + liftAmount, minHeight, maxHeight);
        target.transform.position = new Vector3(currentPos.x, newY, currentPos.z);
    }
    private void HandleMouseScale(GameObject target, float scrollDelta)
    {
        float s = Mathf.Clamp(target.transform.localScale.x + scrollDelta * scrollScaleSpeed, minScale, maxScale);
        target.transform.localScale = Vector3.one * s;
    }
}