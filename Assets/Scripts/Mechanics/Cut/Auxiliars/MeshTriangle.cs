using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa un triángulo de malla con vértices, normales, UVs, tangentes y un índice de submesh.
/// Se utiliza para construir mallas dinámicas mediante la clase GeneratedMesh.
/// </summary>
public class MeshTriangle
{
    // Índice del submesh al que pertenece este triángulo
    private int submeshIndex;

    /// <summary>
    /// Vértices del triángulo.
    /// </summary>
    public List<Vector3> Vertices { get; set; } = new();

    /// <summary>
    /// Normales correspondientes a cada vértice.
    /// </summary>
    public List<Vector3> Normals { get; set; } = new();

    /// <summary>
    /// Coordenadas UV correspondientes a cada vértice.
    /// </summary>
    public List<Vector2> UVs { get; set; } = new();

    /// <summary>
    /// Tangentes correspondientes a cada vértice.
    /// </summary>
    public List<Vector4> Tangents { get; set; } = new();

    /// <summary>
    /// Índice del submesh al que pertenece el triángulo.
    /// Solo se puede establecer desde el constructor.
    /// </summary>
    public int SubmeshIndex { get => submeshIndex; private set => submeshIndex = value; }

    /// <summary>
    /// Constructor que inicializa un triángulo con vértices, normales y UVs.
    /// Asigna tangentes por defecto (1,0,0,-1) para cada vértice.
    /// </summary>
    /// <param name="_vertices">Vértices del triángulo.</param>
    /// <param name="_normals">Normales de cada vértice.</param>
    /// <param name="_uvs">Coordenadas UV de cada vértice.</param>
    /// <param name="_submeshIndex">Índice del submesh al que pertenece el triángulo.</param>
    public MeshTriangle(Vector3[] _vertices, Vector3[] _normals, Vector2[] _uvs, int _submeshIndex) 
    {
        Clear();

        Vertices.AddRange(_vertices);
        Normals.AddRange(_normals);
        UVs.AddRange(_uvs);

        for(int i = 0; i < _vertices.Length; i++)
        {
            Tangents.Add(new Vector4(1, 0, 0, -1));
        }

        submeshIndex = _submeshIndex;
    }

    /// <summary>
    /// Constructor que inicializa un triángulo con vértices, normales, UVs y tangentes personalizadas.
    /// </summary>
    /// <param name="_vertices">Vértices del triángulo.</param>
    /// <param name="_normals">Normales de cada vértice.</param>
    /// <param name="_uvs">Coordenadas UV de cada vértice.</param>
    /// <param name="_tangents">Tangentes personalizadas de cada vértice.</param>
    /// <param name="_submeshIndex">Índice del submesh al que pertenece el triángulo.</param>
    public MeshTriangle(Vector3[] _vertices, Vector3[] _normals, Vector2[] _uvs, Vector4[] _tangents, int _submeshIndex) 
    {
        Clear();

        Vertices.AddRange(_vertices);
        Normals.AddRange(_normals);
        UVs.AddRange(_uvs);
        Tangents.AddRange(_tangents);

        submeshIndex = _submeshIndex;
    }

    /// <summary>
    /// Limpia todos los datos del triángulo y reinicia el submesh a 0.
    /// </summary>
    private void Clear()
    {
        Vertices.Clear();
        Normals.Clear();
        UVs.Clear();
        Tangents.Clear();

        submeshIndex = 0;
    }
}
