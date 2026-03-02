using UnityEngine;
using TMPro;
public class TriggerDemo : MonoBehaviour
{
    [Header("Ефекти")]
    [Tooltip("Колір при вході в тригер")]
    public Color triggerColor = Color.red;
    [Tooltip("Масштаб при вході в тригер")]
    [SerializeField]
    public float triggerScale = 1.5f;
    private Renderer rend;
    private Color originalColor;
    private Vector3 originalScale;
    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        originalScale = transform.localScale;
    }
    void OnTriggerEnter(Collider other)
    {
        // Об'єкт увійшов у тригер-зону
        Debug.Log($"{gameObject.name} увійшов у тригер:{ other.gameObject.name}");
        rend.material.color = triggerColor;
        transform.localScale = originalScale * triggerScale;
    }
    void OnTriggerExit(Collider other)
    {
        // Об'єкт вийшов з тригер-зони
        Debug.Log($"{gameObject.name} вийшов з тригера:{ other.gameObject.name}");
    rend.material.color = originalColor;
        transform.localScale = originalScale;
    }
}