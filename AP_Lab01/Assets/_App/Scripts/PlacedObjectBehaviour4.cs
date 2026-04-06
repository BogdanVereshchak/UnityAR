using UnityEngine;
using System.Collections;

public class PlacedObjectBehaviour4 : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField] private Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
    [SerializeField] private float transitionDuration = 0.5f;

    private Renderer objRenderer;
    private int currentColorIndex = 0;
    private bool isChangingColor = false;

    void Start()
    {
        objRenderer = GetComponentInChildren<Renderer>();
        if (objRenderer != null && colors.Length > 0)
        {
            objRenderer.material.color = colors[0];
        }
    }

    public void TriggerNextColor()
    {
        if (isChangingColor) return;

        // Вираховуємо наступний індекс
        int nextIndex = (currentColorIndex + 1) % colors.Length;
        
        // Починаємо плавну зміну
        StartCoroutine(LerpColor(colors[nextIndex]));

        currentColorIndex = nextIndex;
        Debug.Log($"[COLOR] New color: {colors[currentColorIndex]}");

        // Якщо повернулися до нульового кольору — коло завершено
        if (currentColorIndex == 0)
        {
            ScaleObject();
        }
    }

    private IEnumerator LerpColor(Color targetColor)
    {
        isChangingColor = true;
        Color startColor = objRenderer.material.color;
        float elapsed = 0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            objRenderer.material.color = Color.Lerp(startColor, targetColor, elapsed / transitionDuration);
            yield return null;
        }

        objRenderer.material.color = targetColor;
        isChangingColor = false;
    }

    private void ScaleObject()
    {
        transform.localScale *= 1.1f; // Збільшуємо на 10%
        Debug.Log("[SCALE] Object cycled through all colors and grew by 10%");
    }
}