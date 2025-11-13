using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase que representa una malla generada dinámicamente con soporte para múltiples submeshes.
/// Permite agregar triángulos de manera flexible y obtener un objeto Mesh listo para usar en Unity.
/// </summary>
public class GeneratedMesh 
{
    // Variables privadas que almacenan la información de la malla
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector4> tangents = new List<Vector4>();
    private List<List<int>> submeshIndices = new List<List<int>>();

    /// <summary>
    /// Lista de vértices de la malla.
    /// </summary>
    public List<Vector3> Vertices { get { return vertices; } set { vertices = value; } }

    /// <summary>
    /// Lista de normales de la malla.
    /// </summary>
    public List<Vector3> Normals { get { return normals; } set { normals = value; } }

    /// <summary>
    /// Lista de coordenadas UV de la malla.
    /// </summary>
    public List<Vector2> UVs { get { return uvs; } set { uvs = value; } }

    /// <summary>
    /// Lista de tangentes de la malla.
    /// </summary>
    public List<Vector4> Tangents { get { return tangents; } set { tangents = value; } }

    /// <summary>
    /// Listas de índices para cada submesh.
    /// </summary>
    public List<List<int>> SubmeshIndices { get { return submeshIndices; } set { submeshIndices = value; } }

    /// <summary>
    /// Agrega un triángulo a la malla utilizando un objeto MeshTriangle.
    /// </summary>
    /// <param name="_triangle">Triángulo con vértices, normales, UVs, tangentes y submesh al que pertenece.</param>
    public void AddTriangle(MeshTriangle _triangle)
    {
        int currentVerticeCount = vertices.Count;

        vertices.AddRange(_triangle.Vertices);
        normals.AddRange(_triangle.Normals);
        uvs.AddRange(_triangle.UVs);
        tangents.AddRange(_triangle.Tangents);

        // Asegura que la lista de submeshes tenga espacio suficiente
        if (submeshIndices.Count < _triangle.SubmeshIndex + 1)
        {
            for (int i = submeshIndices.Count; i < _triangle.SubmeshIndex + 1; i++)
            {
                submeshIndices.Add(new List<int>());
            }
        }

        // Agrega los índices del triángulo al submesh correspondiente
        for (int i = 0; i < 3; i++)
        {
            submeshIndices[_triangle.SubmeshIndex].Add(currentVerticeCount + i);
        }
    }

    /// <summary>
    /// Agrega un triángulo a la malla utilizando arrays de vértices, normales, UVs y opcionalmente tangentes.
    /// </summary>
    /// <param name="_vertices">Vértices del triángulo.</param>
    /// <param name="_normals">Normales correspondientes a cada vértice.</param>
    /// <param name="_uvs">Coordenadas UV correspondientes a cada vértice.</param>
    /// <param name="_submeshIndex">Índice del submesh al que pertenece el triángulo.</param>
    /// <param name="_tangents">Tangentes correspondientes a cada vértice. Si es null, se usan valores por defecto.</param>
    public void AddTriangle(Vector3[] _vertices, Vector3[] _normals, Vector2[] _uvs, int _submeshIndex, Vector4[] _tangents = null)
    {
        int currentVerticeCount = vertices.Count;

        vertices.AddRange(_vertices);
        normals.AddRange(_normals);
        uvs.AddRange(_uvs);

        // Asigna tangentes o valores por defecto si no se proporcionan
        if (_tangents != null && _tangents.Length == _vertices.Length)
        {
            tangents.AddRange(_tangents);
        }
        else
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                tangents.Add(new Vector4(1, 0, 0, -1));
            }
        }

        // Asegura que la lista de submeshes tenga espacio suficiente
        if (submeshIndices.Count < _submeshIndex + 1)
        {
            for (int i = submeshIndices.Count; i < _submeshIndex + 1; i++)
            {
                submeshIndices.Add(new List<int>());
            }
        }

        // Agrega los índices del triángulo al submesh correspondiente
        for (int i = 0; i < 3; i++)
        {
            submeshIndices[_submeshIndex].Add(currentVerticeCount + i);
        }
    }

    /// <summary>
    /// Genera y devuelve un objeto Mesh listo para ser usado en Unity.
    /// </summary>
    /// <returns>Objeto Mesh que contiene todos los vértices, normales, UVs, tangentes y submeshes agregados.</returns>
    public Mesh GetGeneratedMesh()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTangents(tangents);

        mesh.subMeshCount = submeshIndices.Count;
        for (int i = 0; i < submeshIndices.Count; i++)
        {
            mesh.SetTriangles(submeshIndices[i], i);
        }

        mesh.RecalculateBounds();

        return mesh;
    }
}
