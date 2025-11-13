using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase responsable de bisecar una malla en dos partes usando un plano de corte.
/// Genera dos mallas separadas (izquierda y derecha) y rellena las caras cortadas.
/// </summary>
public class MeshBisector
{
    // Malla original a cortar
    private Mesh originalMesh;

    // Mallas generadas después del corte
    private GeneratedMesh leftMesh;
    private GeneratedMesh rightMesh;

    // Plano que define el corte
    private Plane cutPlane;

    // Vértices generados durante el corte para rellenar la superficie cortada
    private List<Vector3> addedVertices = new List<Vector3>();

    /// <summary>
    /// Realiza el corte de la malla dada según un plano definido por un punto y una normal local.
    /// </summary>
    /// <param name="mesh">Malla original a cortar.</param>
    /// <param name="localPlanePoint">Punto por el que pasa el plano de corte.</param>
    /// <param name="localPlaneNormal">Normal del plano de corte.</param>
    /// <returns>Array de dos mallas: [0] izquierda, [1] derecha.</returns>
    public Mesh[] Slice(Mesh mesh, Vector3 localPlanePoint, Vector3 localPlaneNormal)
    {
        this.originalMesh = mesh;
        this.cutPlane = new Plane(localPlaneNormal, localPlanePoint);

        this.leftMesh = new GeneratedMesh();
        this.rightMesh = new GeneratedMesh();
        this.addedVertices.Clear();

        SeparateMeshes();
        FillCut();

        Mesh finishedLeftMesh = leftMesh.GetGeneratedMesh();
        Mesh finishedRightMesh = rightMesh.GetGeneratedMesh();

        return new Mesh[] { finishedLeftMesh, finishedRightMesh };
    }

    /// <summary>
    /// Separa cada triángulo de la malla original según su posición respecto al plano de corte.
    /// Triángulos completamente a la izquierda o derecha se añaden directamente,
    /// otros se cortan en la función CutTriangle.
    /// </summary>
    private void SeparateMeshes()
    {
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            var subMeshIndices = originalMesh.GetTriangles(i);

            for (int j = 0; j < subMeshIndices.Length; j += 3)
            {
                var triangleIndexA = subMeshIndices[j];
                var triangleIndexB = subMeshIndices[j + 1];
                var triangleIndexC = subMeshIndices[j + 2];

                MeshTriangle currentTriangle = GetTriangle(triangleIndexA, triangleIndexB, triangleIndexC, i);

                bool triangleALeftSide = cutPlane.GetSide(originalMesh.vertices[triangleIndexA]);
                bool triangleBLeftSide = cutPlane.GetSide(originalMesh.vertices[triangleIndexB]);
                bool triangleCLeftSide = cutPlane.GetSide(originalMesh.vertices[triangleIndexC]);

                switch (triangleALeftSide)
                {
                    case true when triangleBLeftSide && triangleCLeftSide:
                        leftMesh.AddTriangle(currentTriangle);
                        break;
                    case false when !triangleBLeftSide && !triangleCLeftSide:
                        rightMesh.AddTriangle(currentTriangle);
                        break;
                    default:
                        CutTriangle(currentTriangle, triangleALeftSide, triangleBLeftSide, triangleCLeftSide);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Obtiene un MeshTriangle a partir de los índices de la malla original y el submesh correspondiente.
    /// </summary>
    private MeshTriangle GetTriangle(int _triangleIndexA, int _triangleIndexB, int _triangleIndexC, int _submeshIndex)
    {
        Vector3[] verticesToAdd = {
            originalMesh.vertices[_triangleIndexA],
            originalMesh.vertices[_triangleIndexB],
            originalMesh.vertices[_triangleIndexC]
        };

        Vector3[] normalsToAdd = {
            originalMesh.normals[_triangleIndexA],
            originalMesh.normals[_triangleIndexB],
            originalMesh.normals[_triangleIndexC]
        };

        Vector2[] uvsToAdd = {
            originalMesh.uv[_triangleIndexA],
            originalMesh.uv[_triangleIndexB],
            originalMesh.uv[_triangleIndexC]
        };

        Vector4[] tangentsToAdd = {
            originalMesh.tangents[_triangleIndexA],
            originalMesh.tangents[_triangleIndexB],
            originalMesh.tangents[_triangleIndexC]
        };

        return new MeshTriangle(verticesToAdd, normalsToAdd, uvsToAdd, tangentsToAdd, _submeshIndex);
    }

    /// <summary>
    /// Corta un triángulo que está parcialmente a ambos lados del plano.
    /// Calcula los vértices intermedios y genera nuevos triángulos para leftMesh y rightMesh.
    /// </summary>
    private void CutTriangle(MeshTriangle triangle, bool triangleALeftSide, bool triangleBLeftSide, bool triangleCLeftSide)
    {
        List<bool> leftSide = new List<bool> { triangleALeftSide, triangleBLeftSide, triangleCLeftSide };

        MeshTriangle leftMeshTriangle = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], new Vector4[2], triangle.SubmeshIndex);
        MeshTriangle rightMeshTriangle = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], new Vector4[2], triangle.SubmeshIndex);

        bool left = false;
        bool right = false;

        for (int i = 0; i < 3; i++)
        {
            if (leftSide[i])
            {
                if (!left)
                {
                    left = true;
                    leftMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    leftMeshTriangle.Vertices[1] = leftMeshTriangle.Vertices[0];
                    leftMeshTriangle.UVs[0] = triangle.UVs[i];
                    leftMeshTriangle.UVs[1] = leftMeshTriangle.UVs[0];
                    leftMeshTriangle.Normals[0] = triangle.Normals[i];
                    leftMeshTriangle.Normals[1] = leftMeshTriangle.Normals[0];
                    leftMeshTriangle.Tangents[0] = triangle.Tangents[i];
                    leftMeshTriangle.Tangents[1] = leftMeshTriangle.Tangents[0];
                }
                else
                {
                    leftMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    leftMeshTriangle.Normals[1] = triangle.Normals[i];
                    leftMeshTriangle.UVs[1] = triangle.UVs[i];
                    leftMeshTriangle.Tangents[1] = triangle.Tangents[i];
                }
            }
            else
            {
                if (!right)
                {
                    right = true;
                    rightMeshTriangle.Vertices[0] = triangle.Vertices[i];
                    rightMeshTriangle.Vertices[1] = rightMeshTriangle.Vertices[0];
                    rightMeshTriangle.UVs[0] = triangle.UVs[i];
                    rightMeshTriangle.UVs[1] = rightMeshTriangle.UVs[0];
                    rightMeshTriangle.Normals[0] = triangle.Normals[i];
                    rightMeshTriangle.Normals[1] = rightMeshTriangle.Normals[0];
                    rightMeshTriangle.Tangents[0] = triangle.Tangents[i];
                    rightMeshTriangle.Tangents[1] = rightMeshTriangle.Tangents[0];
                }
                else
                {
                    rightMeshTriangle.Vertices[1] = triangle.Vertices[i];
                    rightMeshTriangle.Normals[1] = triangle.Normals[i];
                    rightMeshTriangle.UVs[1] = triangle.UVs[i];
                    rightMeshTriangle.Tangents[1] = triangle.Tangents[i];
                }
            }
        }

        float normalizedDistance;
        float distance;
        cutPlane.Raycast(new Ray(leftMeshTriangle.Vertices[0], (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).normalized), out distance);
        normalizedDistance = distance / (rightMeshTriangle.Vertices[0] - leftMeshTriangle.Vertices[0]).magnitude;

        Vector3 vertLeft = Vector3.Lerp(leftMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[0], normalizedDistance);
        Vector3 normalLeft = Vector3.Lerp(leftMeshTriangle.Normals[0], rightMeshTriangle.Normals[0], normalizedDistance);
        Vector2 uvLeft = Vector2.Lerp(leftMeshTriangle.UVs[0], rightMeshTriangle.UVs[0], normalizedDistance);
        Vector4 tangentLeft = Vector4.Lerp(leftMeshTriangle.Tangents[0], rightMeshTriangle.Tangents[0], normalizedDistance);
        addedVertices.Add(vertLeft);

        cutPlane.Raycast(new Ray(leftMeshTriangle.Vertices[1], (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).normalized), out distance);
        normalizedDistance = distance / (rightMeshTriangle.Vertices[1] - leftMeshTriangle.Vertices[1]).magnitude;

        Vector3 vertRight = Vector3.Lerp(leftMeshTriangle.Vertices[1], rightMeshTriangle.Vertices[1], normalizedDistance);
        Vector3 normalRight = Vector3.Lerp(leftMeshTriangle.Normals[1], rightMeshTriangle.Normals[1], normalizedDistance);
        Vector2 uvRight = Vector2.Lerp(leftMeshTriangle.UVs[1], rightMeshTriangle.UVs[1], normalizedDistance);
        Vector4 tangentRight = Vector4.Lerp(leftMeshTriangle.Tangents[1], rightMeshTriangle.Tangents[1], normalizedDistance);
        addedVertices.Add(vertRight);

        MeshTriangle currentTriangle;
        Vector3[] updatedVertices;
        Vector3[] updatedNormals;
        Vector2[] updatedUVs;
        Vector4[] updatedTangents;

        updatedVertices = new Vector3[] { leftMeshTriangle.Vertices[0], vertLeft, vertRight };
        updatedNormals = new Vector3[] { leftMeshTriangle.Normals[0], normalLeft, normalRight };
        updatedUVs = new Vector2[] { leftMeshTriangle.UVs[0], uvLeft, uvRight };
        updatedTangents = new Vector4[] { leftMeshTriangle.Tangents[0], tangentLeft, tangentRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, updatedTangents, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        updatedVertices = new Vector3[] { leftMeshTriangle.Vertices[0], leftMeshTriangle.Vertices[1], vertRight };
        updatedNormals = new Vector3[] { leftMeshTriangle.Normals[0], leftMeshTriangle.Normals[1], normalRight };
        updatedUVs = new Vector2[] { leftMeshTriangle.UVs[0], leftMeshTriangle.UVs[1], uvRight };
        updatedTangents = new Vector4[] { leftMeshTriangle.Tangents[0], leftMeshTriangle.Tangents[1], tangentRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, updatedTangents, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        updatedVertices = new Vector3[] { rightMeshTriangle.Vertices[0], vertLeft, vertRight };
        updatedNormals = new Vector3[] { rightMeshTriangle.Normals[0], normalLeft, normalRight };
        updatedUVs = new Vector2[] { rightMeshTriangle.UVs[0], uvLeft, uvRight };
        updatedTangents = new Vector4[] { rightMeshTriangle.Tangents[0], tangentLeft, tangentRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, updatedTangents, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }

        updatedVertices = new Vector3[] { rightMeshTriangle.Vertices[0], rightMeshTriangle.Vertices[1], vertRight };
        updatedNormals = new Vector3[] { rightMeshTriangle.Normals[0], rightMeshTriangle.Normals[1], normalRight };
        updatedUVs = new Vector2[] { rightMeshTriangle.UVs[0], rightMeshTriangle.UVs[1], uvRight };
        updatedTangents = new Vector4[] { rightMeshTriangle.Tangents[0], rightMeshTriangle.Tangents[1], tangentRight };

        currentTriangle = new MeshTriangle(updatedVertices, updatedNormals, updatedUVs, updatedTangents, triangle.SubmeshIndex);
        if (updatedVertices[0] != updatedVertices[1] && updatedVertices[0] != updatedVertices[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedVertices[1] - updatedVertices[0], updatedVertices[2] - updatedVertices[0]), updatedNormals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }
    }

    /// <summary>
    /// Invierte los vértices de un triángulo para corregir la orientación de su normal.
    /// </summary>
    private void FlipTriangle(MeshTriangle _triangle)
    {
        Vector3 tempVert = _triangle.Vertices[2];
        _triangle.Vertices[2] = _triangle.Vertices[0];
        _triangle.Vertices[0] = tempVert;

        Vector3 tempNormal = _triangle.Normals[2];
        _triangle.Normals[2] = _triangle.Normals[0];
        _triangle.Normals[0] = tempNormal;

        (_triangle.UVs[2], _triangle.UVs[0]) = (_triangle.UVs[0], _triangle.UVs[2]);
        (_triangle.Tangents[2], _triangle.Tangents[0]) = (_triangle.Tangents[0], _triangle.Tangents[2]);
    }

    /// <summary>
    /// Rellena los cortes generados con triángulos que cubren la superficie expuesta.
    /// </summary>
    private void FillCut()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> polygon = new List<Vector3>();

        for (int i = 0; i < addedVertices.Count; i += 2)
        {
            if (!vertices.Contains(addedVertices[i]))
            {
                polygon.Clear();
                polygon.Add(addedVertices[i]);
                polygon.Add(addedVertices[i + 1]);

                vertices.Add(addedVertices[i]);
                vertices.Add(addedVertices[i + 1]);

                EvaluatePairs(vertices, polygon);
                Fill(polygon);
            }
        }
    }

    /// <summary>
    /// Evalúa pares de vértices generados para completar el contorno del polígono a rellenar.
    /// </summary>
    private void EvaluatePairs(List<Vector3> _vertices, List<Vector3> _polygon)
    {
        bool isDone = false;
        while (!isDone)
        {
            isDone = true;
            for (int i = 0; i < addedVertices.Count; i += 2)
            {
                if (addedVertices[i] == _polygon[_polygon.Count - 1] && !_vertices.Contains(addedVertices[i + 1]))
                {
                    isDone = false;
                    _polygon.Add(addedVertices[i + 1]);
                    _vertices.Add(addedVertices[i + 1]);
                }
                else if (addedVertices[i + 1] == _polygon[_polygon.Count - 1] && !_vertices.Contains(addedVertices[i]))
                {
                    isDone = false;
                    _polygon.Add(addedVertices[i]);
                    _vertices.Add(addedVertices[i]);
                }
            }
        }
    }

    /// <summary>
    /// Genera los triángulos para rellenar un polígono cortado.
    /// </summary>
    private void Fill(List<Vector3> _vertices)
    {
        Vector3 centerPosition = Vector3.zero;
        for (int i = 0; i < _vertices.Count; i++)
        {
            centerPosition += _vertices[i];
        }
        centerPosition /= _vertices.Count;

        Vector3 up = new Vector3(cutPlane.normal.x, cutPlane.normal.y, cutPlane.normal.z);
        Vector3 left = Vector3.Cross(cutPlane.normal, up);

        Vector4 tangent = new Vector4(left.x, left.y, left.z, -1);
        Vector4[] tangents = { tangent, tangent, tangent };


        Vector3 displacement;
        Vector2 uv1;
        Vector2 uv2;
        Vector2 uvCenter = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < _vertices.Count; i++)
        {
            displacement = _vertices[i] - centerPosition;
            uv1 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up)
            };

            displacement = _vertices[(i + 1) % _vertices.Count] - centerPosition;
            uv2 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up)
            };

            Vector3[] vertices = { _vertices[i], _vertices[(i + 1) % _vertices.Count], centerPosition };
            Vector2[] uvs = { uv1, uv2, uvCenter };

            Vector3[] normalsLeft = { -cutPlane.normal, -cutPlane.normal, -cutPlane.normal };
            MeshTriangle currentTriangle = new MeshTriangle(vertices, normalsLeft, uvs, tangents, originalMesh.subMeshCount + 1);
            if (Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), normalsLeft[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);

            Vector3[] normalsRight = { cutPlane.normal, cutPlane.normal, cutPlane.normal };
            currentTriangle = new MeshTriangle(vertices, normalsRight, uvs, tangents, originalMesh.subMeshCount + 1);
            if (Vector3.Dot(Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]), normalsRight[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }
    }
}