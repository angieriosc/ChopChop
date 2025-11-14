using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slices a mesh into N "cake slices" around a central axis using
/// recursive triangle subdivision.
/// This is a "solid" cut that does not require cap-filling.
/// </summary>
public class MeshSlicer
{
    // --- Internal data ---
    private Mesh originalMesh;            // Mesh original a cortar
    private GeneratedMesh[] sliceMeshes;  // Arreglo de meshes generados para cada slice
    private Plane[] cutPlanes;            // Planos de corte que definen cada slice
    private Vector3 cutCenter;            // Centro local de corte
    private Vector3 cutUpAxis;            // Eje alrededor del cual se rota
    private float angleStep;              // Ángulo de cada slice
    private int sliceCount;               // Cantidad de slices a generar

    // --- Configurable Settings ---
    private readonly Vector3 baseAxis = Vector3.right; // Eje de referencia para medir ángulos
    private const int MAX_RECURSION_DEPTH = 5;         // Limite de subdivisión de triángulos

    /// <summary>
    /// Slices a mesh into N equal "cake" slices.
    /// </summary>
    /// <param name="meshToSlice">Mesh original a cortar</param>
    /// <param name="localCenter">Centro del "pastel" en espacio local</param>
    /// <param name="localUpAxis">Eje local para rotación (Vector3.up, etc.)</param>
    /// <param name="slices">Número de slices a crear</param>
    /// <returns>Lista de meshes resultantes</returns>
    public List<Mesh> Slice(Mesh meshToSlice, Vector3 localCenter, Vector3 localUpAxis, int slices)
    {
        // 1. Inicializar datos
        this.originalMesh = meshToSlice;
        this.cutCenter = localCenter;
        this.cutUpAxis = localUpAxis.normalized;
        this.sliceCount = slices;
        this.angleStep = 360f / sliceCount;

        // 2. Inicializar arreglos de planos y meshes
        this.cutPlanes = new Plane[sliceCount];
        this.sliceMeshes = new GeneratedMesh[sliceCount];
        for (int i = 0; i < sliceCount; i++)
        {
            sliceMeshes[i] = new GeneratedMesh();
        }

        // 3. Definir planos de corte
        for (int i = 0; i < sliceCount; i++)
        {
            Vector3 planeNormal = Quaternion.AngleAxis(angleStep * i, this.cutUpAxis) * baseAxis;
            cutPlanes[i] = new Plane(planeNormal, localCenter);
        }

        // 4. Procesar todos los triángulos del mesh original
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            var subMeshIndices = originalMesh.GetTriangles(i);
            for (int j = 0; j < subMeshIndices.Length; j += 3)
            {
                // Obtener datos completos del triángulo
                var triangle = GetTriangle(
                    subMeshIndices[j],
                    subMeshIndices[j + 1],
                    subMeshIndices[j + 2],
                    i);

                // Comenzar el slicing recursivo
                ProcessTriangle(triangle, 0);
            }
        }

        // 5. Generar y retornar meshes finales
        List<Mesh> finalMeshes = new List<Mesh>();
        for (int i = 0; i < sliceCount; i++)
        {
            Mesh finalMesh = sliceMeshes[i].GetGeneratedMesh();
            if (finalMesh.vertexCount > 0)
            {
                finalMeshes.Add(finalMesh);
            }
        }

        return finalMeshes;
    }

    /// <summary>
    /// Procesa recursivamente un triángulo, subdividiéndolo hasta que
    /// quede completamente dentro de un slice.
    /// </summary>
    private void ProcessTriangle(MeshTriangle triangle, int depth)
    {
        // Obtener índice de slice para cada vértice
        int i0 = GetSliceIndex(triangle.Vertices[0]);
        int i1 = GetSliceIndex(triangle.Vertices[1]);
        int i2 = GetSliceIndex(triangle.Vertices[2]);

        // --- Caso 1: Todos los vértices están en el mismo slice ---
        if (i0 == i1 && i0 == i2)
        {
            sliceMeshes[i0].AddTriangle(triangle);
            return;
        }

        // --- Caso 2: Profundidad máxima de recursión alcanzada ---
        if (depth > MAX_RECURSION_DEPTH)
        {
            sliceMeshes[i0].AddTriangle(triangle);
            return;
        }

        // --- Caso 3: Triángulo cruza un límite ---
        // Subdividir en 4 triángulos y llamar recursivamente

        // Encontrar puntos medios de cada arista
        Vector3 v01 = (triangle.Vertices[0] + triangle.Vertices[1]) * 0.5f;
        Vector3 v12 = (triangle.Vertices[1] + triangle.Vertices[2]) * 0.5f;
        Vector3 v20 = (triangle.Vertices[2] + triangle.Vertices[0]) * 0.5f;

        // Interpolar normales, UVs y tangentes
        Vector3 n01 = (triangle.Normals[0] + triangle.Normals[1]).normalized;
        Vector3 n12 = (triangle.Normals[1] + triangle.Normals[2]).normalized;
        Vector3 n20 = (triangle.Normals[2] + triangle.Normals[0]).normalized;

        Vector2 uv01 = (triangle.UVs[0] + triangle.UVs[1]) * 0.5f;
        Vector2 uv12 = (triangle.UVs[1] + triangle.UVs[2]) * 0.5f;
        Vector2 uv20 = (triangle.UVs[2] + triangle.UVs[0]) * 0.5f;

        Vector4 t01 = (triangle.Tangents[0] + triangle.Tangents[1]).normalized;
        Vector4 t12 = (triangle.Tangents[1] + triangle.Tangents[2]).normalized;
        Vector4 t20 = (triangle.Tangents[2] + triangle.Tangents[0]).normalized;

        int submesh = triangle.SubmeshIndex;

        // Crear los 4 nuevos triángulos y procesarlos
        ProcessTriangle(new MeshTriangle(
            new[] { triangle.Vertices[0], v01, v20 },
            new[] { triangle.Normals[0], n01, n20 },
            new[] { triangle.UVs[0], uv01, uv20 },
            new[] { triangle.Tangents[0], t01, t20 },
            submesh),
            depth + 1);

        ProcessTriangle(new MeshTriangle(
            new[] { triangle.Vertices[1], v12, v01 },
            new[] { triangle.Normals[1], n12, n01 },
            new[] { triangle.UVs[1], uv12, uv01 },
            new[] { triangle.Tangents[1], t12, t01 },
            submesh),
            depth + 1);

        ProcessTriangle(new MeshTriangle(
            new[] { triangle.Vertices[2], v20, v12 },
            new[] { triangle.Normals[2], n20, n12 },
            new[] { triangle.UVs[2], uv20, uv12 },
            new[] { triangle.Tangents[2], t20, t12 },
            submesh),
            depth + 1);

        ProcessTriangle(new MeshTriangle(
            new[] { v01, v12, v20 },
            new[] { n01, n12, n20 },
            new[] { uv01, uv12, uv20 },
            new[] { t01, t12, t20 },
            submesh),
            depth + 1);
    }

    /// <summary>
    /// Devuelve el índice del slice correspondiente a un vértice.
    /// </summary>
    private int GetSliceIndex(Vector3 vertex)
    {
        // Dirección desde el centro hacia el vértice
        Vector3 direction = vertex - cutCenter;

        // Proyectar sobre el plano perpendicular al eje de corte
        Vector3 flatDirection = Vector3.ProjectOnPlane(direction, cutUpAxis).normalized;

        // Si está sobre el eje, asignar slice 0
        if (flatDirection.sqrMagnitude < 0.0001f)
        {
            return 0;
        }

        // Calcular ángulo respecto al eje base
        float angle = Vector3.SignedAngle(baseAxis, flatDirection, cutUpAxis);

        // Convertir ángulo de -180/180 a 0/360
        if (angle < 0) angle += 360f;

        // Calcular índice del slice
        int index = Mathf.FloorToInt(angle / angleStep);

        // Clampeo para evitar errores de punto flotante
        return Mathf.Clamp(index, 0, sliceCount - 1);
    }

    /// <summary>
    /// Devuelve todos los datos de un triángulo del mesh original.
    /// </summary>
    private MeshTriangle GetTriangle(int i0, int i1, int i2, int submesh)
    {
        return new MeshTriangle(
            new[] { originalMesh.vertices[i0], originalMesh.vertices[i1], originalMesh.vertices[i2] },
            new[] { originalMesh.normals[i0], originalMesh.normals[i1], originalMesh.normals[i2] },
            new[] { originalMesh.uv[i0], originalMesh.uv[i1], originalMesh.uv[i2] },
            new[] { originalMesh.tangents[i0], originalMesh.tangents[i1], originalMesh.tangents[i2] },
            submesh
        );
    }
}
