using UnityEngine;

public class PhysicsDemo : MonoBehaviour
{
    [Header("Налаштування сили")]
    [Tooltip("Величина сили (для варіанту буде помножена на час утримування)")]
    public float forceAmount = 5f;
    [Tooltip("Напрямок сили")]
    public Vector3 forceDirection = Vector3.up;
    [Tooltip("Тип сили")]
    public ForceMode forceMode = ForceMode.Impulse;

    [SerializeField] bool isVariantTarget = false;
    private Vector3 startPosition;
    private float lastTapTime;
    private Rigidbody rb;
    
    // Нова змінна для збереження часу заряджання
    private float chargeTime = 0f;

    void Awake()
    {
        // Отримуємо посилання на Rigidbody один раз
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError($"PhysicsDemo: Rigidbody не знайдено на {gameObject.name}!");
        }
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // --- СТАНДАРТНА ПОВЕДІНКА (якщо це НЕ варіант) ---
        if (!isVariantTarget)
        {
            bool simpleTap = Input.GetKeyDown(KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            if (simpleTap)
            {
                ApplyForce(); // Миттєвий стрибок зі стандартною силою
            }
            return; // Виходимо, щоб не виконувати логіку заряджання і подвійного кліку
        }


        // --- ПОВЕДІНКА ДЛЯ ВАРІАНТУ (isVariantTarget == true) ---

        // 1. Логіка подвійного тапу для скидання позиції
        bool tapBegan = Input.GetKeyDown(KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        if (tapBegan)
        {
            if (Time.time - lastTapTime < 0.3f) 
            {
                // Це подвійний клік - повертаємо на старт
                transform.position = startPosition;
                rb.linearVelocity = Vector3.zero; // Скидаємо швидкість при телепортації
                rb.angularVelocity = Vector3.zero;
                chargeTime = 0f; // Скидаємо заряд
                lastTapTime = 0f; // Скидаємо таймер подвійного кліку
                return; // Виходимо з кадру
            }
            lastTapTime = Time.time;
        }

        // 2. Визначаємо стан: утримування або відпускання
        bool isHolding = Input.GetKey(KeyCode.Space);
        bool isReleased = Input.GetKeyUp(KeyCode.Space);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                isHolding = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isReleased = true;
            }
        }

        // 3. Заряджання (збільшення chargeTime)
        if (isHolding)
        {
            chargeTime += Time.deltaTime;
            Debug.Log($"Заряджання... Час: {chargeTime:F2}");
        }

        // 4. Застосування сили при відпусканні
        if (isReleased && chargeTime > 0f)
        {
            ApplyChargedForce();
            chargeTime = 0f; // Скидаємо час після стрибка
        }
    }

    // Стандартний метод сили (для звичайних об'єктів)
    private void ApplyForce()
    {
        if (rb == null) return;
        rb.AddForce(forceDirection.normalized * forceAmount, forceMode);
        Debug.Log($"Стандартна сила застосована: {forceDirection.normalized * forceAmount}, режим: {forceMode}");
    }

    // Метод сили із заряджанням (тільки для варіанту)
    private void ApplyChargedForce()
    {
        if (rb == null) return;

        // Розраховуємо підсумкову силу: Напрямок * Час утримування * Множник сили
        Vector3 appliedForce = forceDirection.normalized * chargeTime * forceAmount;
        
        rb.AddForce(appliedForce, forceMode);
        
        Debug.Log($"Стрибок! Сила із зарядом: {appliedForce}, режим: {forceMode}. Час заряду: {chargeTime:F2}");
    }
}