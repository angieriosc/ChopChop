using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase estática que se encarga de triangulación de polígonos usando el método Ear Clipping.
/// </summary>
public static class PolygonTriangulator
{
    /// <summary>
    /// Triangula un polígono definido por una lista de vértices 2D.
    /// </summary>
    /// <param name="vertices">Lista de vértices del polígono en orden.</param>
    /// <returns>Lista de índices de los triángulos generados en orden de 3 en 3.</returns>
    public static List<int> Triangulate(List<Vector2> vertices)
    {
        List<int> outputIndices = new List<int>();
        if (vertices == null || vertices.Count < 3)
        {
            return outputIndices;
        }

        // Lista de índices activos para procesar
        List<int> activeIndices = new List<int>();
        for (int i = 0; i < vertices.Count; i++)
        {
            activeIndices.Add(i);
        }

        // Determina si el polígono está en sentido horario
        bool isClockwise = GetArea(vertices) > 0;

        int safetyCounter = activeIndices.Count * 10;

        // Mientras haya más de 3 vértices y no se supere el contador de seguridad
        while (activeIndices.Count > 3 && safetyCounter > 0)
        {
            safetyCounter--;
            bool earFound = false;

            for (int i = 0; i < activeIndices.Count; i++)
            {
                int p = activeIndices[i];
                int c = activeIndices[(i + 1) % activeIndices.Count];
                int n = activeIndices[(i + 2) % activeIndices.Count];

                Vector2 p_v = vertices[p];
                Vector2 c_v = vertices[c];
                Vector2 n_v = vertices[n];

                // Verifica si el triángulo es convexo
                if (!IsConvex(p_v, c_v, n_v, isClockwise))
                {
                    continue;
                }

                // Verifica si algún otro vértice está dentro del triángulo
                bool pointInTriangle = false;
                for (int k = 0; k < activeIndices.Count; k++)
                {
                    int testIndex = activeIndices[k];
                    if (testIndex == p || testIndex == c || testIndex == n)
                    {
                        continue;
                    }

                    if (IsPointInTriangle(vertices[testIndex], p_v, c_v, n_v))
                    {
                        pointInTriangle = true;
                        break;
                    }
                }

                // Si no hay vértices dentro, se considera una "oreja"
                if (!pointInTriangle)
                {
                    outputIndices.Add(p);
                    outputIndices.Add(c);
                    outputIndices.Add(n);

                    // Remueve el vértice central de la oreja
                    int removeIndex = (i + 1) % activeIndices.Count;
                    activeIndices.RemoveAt(removeIndex);

                    earFound = true;
                    break;
                }
            }

            // Si no se encuentra oreja y safetyCounter se acabó, retorna vacío
            if (!earFound && safetyCounter <= 0)
            {
                return new List<int>();
            }
        }

        // Añade el último triángulo restante
        if (activeIndices.Count == 3)
        {
            outputIndices.Add(activeIndices[0]);
            outputIndices.Add(activeIndices[1]);
            outputIndices.Add(activeIndices[2]);
        }

        return outputIndices;
    }

    /// <summary>
    /// Calcula el área del polígono para determinar su orientación.
    /// </summary>
    /// <param name="vertices">Lista de vértices del polígono.</param>
    /// <returns>Valor del área (positivo = horario, negativo = antihorario).</returns>
    private static float GetArea(List<Vector2> vertices)
    {
        float area = 0.0f;
        for (int p = vertices.Count - 1, q = 0; q < vertices.Count; p = q++)
        {
            area += (vertices[p].x * vertices[q].y) - (vertices[q].x * vertices[p].y);
        }
        return area * 0.5f;
    }

    /// <summary>
    /// Determina si un triángulo formado por tres puntos es convexo según la orientación.
    /// </summary>
    /// <param name="p">Primer vértice.</param>
    /// <param name="c">Segundo vértice (pico de la oreja).</param>
    /// <param name="n">Tercer vértice.</param>
    /// <param name="isClockwise">Indica si el polígono es horario.</param>
    /// <returns>True si es convexo, false si es cóncavo.</returns>
    private static bool IsConvex(Vector2 p, Vector2 c, Vector2 n, bool isClockwise)
    {
        float cross = (c.x - p.x) * (n.y - c.y) - (c.y - p.y) * (n.x - c.x);

        return isClockwise ? cross > 0 : cross < 0;
    }

    /// <summary>
    /// Determina si un punto está dentro de un triángulo.
    /// </summary>
    /// <param name="p">Punto a verificar.</param>
    /// <param name="a">Primer vértice del triángulo.</param>
    /// <param name="b">Segundo vértice del triángulo.</param>
    /// <param name="c">Tercer vértice del triángulo.</param>
    /// <returns>True si el punto está dentro del triángulo, false si está fuera.</returns>
    private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float s1 = Sign(p, a, b);
        float s2 = Sign(p, b, c);
        float s3 = Sign(p, c, a);

        bool hasNegative = (s1 < 0) || (s2 < 0) || (s3 < 0);
        bool hasPositive = (s1 > 0) || (s2 > 0) || (s3 > 0);

        return !(hasNegative && hasPositive);
    }

    /// <summary>
    /// Calcula el signo de un punto respecto a una línea formada por dos puntos.
    /// </summary>
    /// <param name="p1">Punto a evaluar.</param>
    /// <param name="p2">Primer vértice de la línea.</param>
    /// <param name="p3">Segundo vértice de la línea.</param>
    /// <returns>Valor positivo o negativo según el lado del punto respecto a la línea.</returns>
    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p1.y - p3.y) * (p2.x - p3.x);
    }
}
