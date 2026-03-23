using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ARObjectInteractor : MonoBehaviour
{
    [Header("UI Components (World Space)")]
    [Tooltip("Canvas, який має режим Render Mode = World Space")]
    [SerializeField] private GameObject worldSpaceCanvas;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Buttons")]
    [SerializeField] private Button rotateButton;
    [SerializeField] private Button scaleButton;
    [SerializeField] private Button colorButton;

    [Header("Settings")]
    [Tooltip("Висота панелі над вибраним об'єктом")]
    [SerializeField] private float yOffset = 0.3f;

    // Словник (Map) для перекладу технічних назв у зрозумілі
    private Dictionary<string, string> nameMap = new Dictionary<string, string>()
    {
        { "Content_01", "Arduino Uno" },
        { "Content_02", "LM386" },
        { "Content_03", "AC-DC Перетворювач" },
        { "Content_04", "ESP32" },
    };

    private GameObject selectedObject;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Ховаємо Canvas на старті
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.SetActive(false);

        // Підписуємо кнопки на виконання відповідних методів
        if (rotateButton != null) rotateButton.onClick.AddListener(RotateObject);
        if (scaleButton != null) scaleButton.onClick.AddListener(ScaleObject);
        if (colorButton != null) colorButton.onClick.AddListener(ChangeColor);
    }

    void Update()
    {
        // Якщо Canvas активний, він має завжди повертатися "обличчям" до камери (Billboard ефект)
        if (worldSpaceCanvas.activeSelf && mainCamera != null)
        {
            Vector3 directionToCamera = worldSpaceCanvas.transform.position - mainCamera.transform.position;
            worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(directionToCamera);
        }

        // Обробка тапу/кліку
        Vector2 inputPosition = Vector2.zero;
        bool isInteractionBegan = false;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPosition = Input.GetTouch(0).position;
            isInteractionBegan = true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            isInteractionBegan = true;
        }

        if (isInteractionBegan)
        {
            // Перевіряємо, чи ми не тапнули по самій UI панелі (щоб вона не зникла, коли ми тиснемо кнопку)
            if (IsPointerOverUI(inputPosition)) return;

            Ray ray = mainCamera.ScreenPointToRay(inputPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Якщо влучили в об'єкт, виділяємо його
                SelectObject(hit.collider.gameObject);
            }
            else
            {
                // Якщо тапнули в порожнечу — знімаємо виділення
                DeselectObject();
            }
        }
    }

    private void SelectObject(GameObject obj)
    {
        selectedObject = obj;

        // Показуємо UI панель та переміщуємо її над об'єктом
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.SetActive(true);
            worldSpaceCanvas.transform.position = selectedObject.transform.position + Vector3.up * yOffset;
        }

        // Оновлюємо опис з використанням словника
        if (descriptionText != null)
        {
            string friendlyName = GetFriendlyName(obj.name);
            descriptionText.text = $"Selected: {friendlyName}\nTap buttons to interact.";
        }
    }

    private void DeselectObject()
    {
        selectedObject = null;
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.SetActive(false);
        }
    }

    // --- Метод для отримання гарної назви ---
    private string GetFriendlyName(string rawName)
    {
        // Проходимось по нашому словнику і шукаємо збіги
        foreach (var kvp in nameMap)
        {
            if (rawName.Contains(kvp.Key))
            {
                return kvp.Value; // Якщо знайшли (наприклад, "content_01(Clone)" містить "content_01"), повертаємо гарну назву
            }
        }

        // Якщо в словнику такої назви немає, просто відрізаємо "(Clone)", щоб було красиво
        return rawName.Replace("(Clone)", "").Trim();
    }

    // --- Дії кнопок ---

    private void RotateObject()
    {
        if (selectedObject != null)
        {
            // Повертаємо на 45 градусів по осі Y
            selectedObject.transform.Rotate(Vector3.up, 45f, Space.World);
        }
    }

    private void ScaleObject()
    {
        if (selectedObject != null)
        {
            // Збільшуємо масштаб. Якщо він стає завеликим, скидаємо до стандартного (наприклад, 0.1f)
            Vector3 newScale = selectedObject.transform.localScale * 1.5f;
            
            // Захист від гігантських об'єктів (тут можна налаштувати свій ліміт)
            if (newScale.x > 0.5f) 
                newScale = new Vector3(0.1f, 0.1f, 0.1f); 

            selectedObject.transform.localScale = newScale;
        }
    }

    private void ChangeColor()
    {
        if (selectedObject != null)
        {
            Renderer rend = selectedObject.GetComponent<Renderer>();
            if (rend != null)
            {
                // Призначаємо випадковий колір
                rend.material.color = Random.ColorHSV();
            }
        }
    }

    // --- Перевірка UI ---

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        return results.Count > 0;
    }
}