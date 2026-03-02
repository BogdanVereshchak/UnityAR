using UnityEngine;

public class Spinner : MonoBehaviour
{
    [Header("Налаштування обертання")]
    [Tooltip("Вісь обертання")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Швидкість обертання (градуси/секунду)")]
    [Range(0f, 360f)]
    public float speed = 30f;
    [Range(0f, 360f)]
    public float maxSpeed = 5f;
    [SerializeField] private bool isVariantTarget = false;

    void Update()
    {
        if (isVariantTarget){
            transform.Rotate(rotationAxis* Mathf.Abs(Mathf.Sin(Time.time)) *maxSpeed* Time.deltaTime);
        }
        else{
            transform.Rotate(rotationAxis * speed * Time.deltaTime);
        }
    }
}
