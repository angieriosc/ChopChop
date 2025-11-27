using UnityEngine;
using System;

public class SimpleCinematic : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;

    private int index;
    public Action OnCinematicEnd;

    private bool playing = false;

    /// <summary>
    /// Inicializa la posición y rotación del objeto en el primer punto
    /// de la cinemática si existen puntos definidos.
    /// </summary>
    private void Start()
    {
        if (points.Length == 0) return;

        transform.position = points[0].position;
        transform.rotation = points[0].rotation;

    }

    /// <summary>
    /// Actualiza el movimiento de la cinemática mientras esté activa.
    /// - Mueve al siguiente punto.
    /// - Detecta cuando se llega al punto actual.
    /// - Ejecuta el evento de fin de cinemática cuando termina.
    /// </summary>
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
    /// Inicia la cinemática desde el primer punto.
    /// Reinicia el índice, activa el estado de reproducción y
    /// coloca al objeto en la posición y rotación inicial.
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

    /// <summary>
    /// Mueve el objeto transform hacia el punto especificado utilizando
    /// interpolación lineal para posición y rotación.
    /// </summary>
    /// <param name="point">Punto objetivo al que se desea mover.</param>
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

    /// <summary>
    /// Verifica si el objeto ha alcanzado un punto específico.
    /// Usa distancia mínima para determinar el arribo.
    /// </summary>
    /// <param name="point">Punto a comprobar.</param>
    /// <returns>true si se ha llegado al punto; de lo contrario, false.</returns>
    private bool Reached(Transform point)
    {
        return Vector3.Distance(transform.position, point.position) < 0.1f;
    }
}
