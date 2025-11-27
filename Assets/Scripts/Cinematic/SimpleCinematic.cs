using UnityEngine;
using System;

public class SimpleCinematic : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;

    private int index;
    public Action OnCinematicEnd;

    private bool playing = false;

    private void Start()
    {
        if (points.Length == 0) return;

        transform.position = points[0].position;
        transform.rotation = points[0].rotation;

    }

    private void Update()
    {
        if (!playing || index >= points.Length) return;

        MoveToPoint(points[index]);

        if (Reached(points[index]))
        {
            index++;

            if (index >= points.Length)
            {
                playing = false;
                enabled = false;
                OnCinematicEnd?.Invoke();
            }
        }
    }

    /// <summary>
    /// Inicia la cinemática desde el punto 0.
    /// </summary>
    public void Play()
    {
        if (points.Length == 0) return;

        index = 0;
        playing = true;
        enabled = true;

        // Reiniciar posición siempre que se llame Play
        transform.position = points[0].position;
        transform.rotation = points[0].rotation;
    }

    private void MoveToPoint(Transform point)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            point.position,
            speed * Time.deltaTime
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            point.rotation,
            speed * Time.deltaTime
        );
    }

    private bool Reached(Transform point)
    {
        return Vector3.Distance(transform.position, point.position) < 0.1f;
    }
}
