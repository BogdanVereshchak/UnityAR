using System;
using UnityEngine;

public class PlacedObjectBehaviour : MonoBehaviour
{
    [Header("Налаштування обертання")]
    [Tooltip("Вісь обертання")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Швидкість обертання (градуси/секунду)")]
    [Range(0f, 360f)]
    public float speed = 30f;
    
    void Update()
    {
        transform.Rotate(rotationAxis, speed * Time.deltaTime);
    }
}
