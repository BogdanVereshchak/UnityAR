using UnityEngine;
public class PhysicsDemo : MonoBehaviour
{
    [Header("Налаштування сили")]
    [Tooltip("Величина сили")]
    public float forceAmount = 5f;
    [Tooltip("Напрямок сили")]
    public Vector3 forceDirection = Vector3.up;
    [Tooltip("Тип сили")]
    public ForceMode forceMode = ForceMode.Impulse;
    private Rigidbody rb;
    void Awake()
    {
        // Отримуємо посилання на Rigidbody один раз
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"PhysicsDemo: Rigidbody не знайдено на{ gameObject.name}!");
        }
    }
    void Update()
    {
        // У редакторі — пробіл, на телефоні — тап по екрану
        bool activate = Input.GetKeyDown(KeyCode.Space);
        if (!activate && Input.touchCount > 0 && Input.GetTouch(0).phase ==
        TouchPhase.Began)
        {
            activate = true;
        }
        if (activate)
        {
            ApplyForce();
        }
    }
private void ApplyForce()
    {
        if (rb == null) return;
        rb.AddForce(forceDirection.normalized * forceAmount, forceMode);
        Debug.Log($"Сила застосована: {forceDirection.normalized *forceAmount}, режим: {forceMode}");
    }
}