using UnityEngine;
using System;

public class SimpleCinematic : MonoBehaviour
{
    public Transform[] points; 
    public float speed = 2f;

    private int index = 0;
    public Action OnCinematicEnd; // ← evento que se dispara al finalizar

    void Start()
    {
        transform.position = points[0].position;
        transform.rotation = points[0].rotation;
    }

    void Update()
    {
        if (index >= points.Length) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            points[index].position,
            speed * Time.deltaTime
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            points[index].rotation,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, points[index].position) < 0.1f)
        {
            if (index < points.Length - 1)
            {
                index++;
            }
            else
            {
                // Llegó al final
                OnCinematicEnd?.Invoke(); 
                enabled = false; // detiene la cámara
            }
        }
    }
}
