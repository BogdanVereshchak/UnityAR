using System;
using UnityEngine;

public class PlacedObjectBehaviour2 : MonoBehaviour
{
    [Header("Налаштування пульсації")]
    public float maxScale = 0.2f;
    public float speed = 2f;
    private Vector3 initialScale;
    void Start()
    {
        initialScale = transform.localScale;
    }
    void Update()
    {
        transform.localScale = initialScale * (1f + maxScale * Mathf.Sin(Time.time*speed));
    }
}
