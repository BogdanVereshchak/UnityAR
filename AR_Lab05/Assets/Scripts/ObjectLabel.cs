using UnityEngine;
using TMPro;

public class ObjectLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // Повертаємо Canvas обличчям до камери (Billboard)
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                         mainCameraTransform.rotation * Vector3.up);
    }

    public void SetText(string text)
    {
        if (labelText != null)
            labelText.text = text;
    }
}