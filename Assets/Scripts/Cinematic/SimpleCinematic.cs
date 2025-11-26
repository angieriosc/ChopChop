using UnityEngine;
using System;

/// <summary>
/// Controla una cinemática mediante el movimiento secuencial
/// de un objeto a través de puntos definidos.
/// </summary>
public class SimpleCinematic : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;

    private int index;
    public Action OnCinematicEnd;

    /// <summary>
    /// Inicializa la posición y rotación del objeto en el primer punto
    /// de la secuencia, si existe al menos uno.
    /// </summary>
    private void Start()
    {
        if (points.Length == 0) return;

        transform.position = points[0].position;
        transform.rotation = points[0].rotation;
    }

    /// <summary>
    /// Actualiza continuamente el movimiento hacia el punto actual,
    /// avanza al siguiente cuando lo alcanza y finaliza la cinemática
    /// cuando se recorren todos los puntos.
    /// </summary>
    private void Update()
    {
        if (index >= points.Length) return;

        MoveToPoint(points[index]);

        if (Reached(points[index]))
        {
            index++;

            if (index >= points.Length)
            {
                OnCinematicEnd?.Invoke();
                enabled = false;
            }
        }
    }

    /// <summary>
    /// Desplaza y rota el objeto hacia el punto indicado utilizando
    /// interpolación suave y velocidad constante.
    /// </summary>
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
    /// Determina si el objeto ha llegado lo suficientemente cerca
    /// del punto objetivo, usando una tolerancia mínima.
    /// </summary>
    private bool Reached(Transform point)
    {
        return Vector3.Distance(
            transform.position,
            point.position
        ) < 0.1f;
    }
}
