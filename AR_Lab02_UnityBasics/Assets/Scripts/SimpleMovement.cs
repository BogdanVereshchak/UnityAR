using UnityEngine;
public class SimpleMovement : MonoBehaviour
{
    [Tooltip("Швидкість руху")]
    public float moveSpeed = 3f;
    private const float DeadZone = 0.05f;

    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;
        // На телефоні — акселерометр, у редакторі — клавіатура
        if (!Application.isEditor && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                horizontal = delta.x * 0.01f;
                vertical = delta.y * 0.01f;
            }
            if (Mathf.Abs(horizontal) < DeadZone) horizontal = 0f;
            if (Mathf.Abs(vertical) < DeadZone) vertical = 0f;
        }
        else
        {
            horizontal = Input.GetAxis("Horizontal"); // A/D або ←/→
            vertical = Input.GetAxis("Vertical"); // W/S або ↑/↓
        }
        Vector3 movement = new Vector3(horizontal, 0, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}

