using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estación de corte que realiza el slicing en el objeto que se le asigne.
/// </summary>
[RequireComponent(typeof(BoxCollider))] 
public class SlicingStation : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField]
    [Tooltip("Referencia al GameManager para notificar cuando se completa un corte.")]
    private GameManager gameManager;

    [Header("Slice Settings")]
    [Range(2, 12)]
    [Tooltip("Cantidad de cortes que se harán al objeto.")]
    public int sliceCount = 6;

    [Header("Cutting Axis (Local)")]
    [Tooltip("Eje local alrededor del cual se realizarán los cortes (relativo al objeto padre).")]
    public Vector3 cutUpAxis = Vector3.up;

    [Header("Object to Cut")]
    [Tooltip("Referencia al objeto actualmente asignado para cortar (asignado por InventoryUI).")]
    public GameObject objectToCut;

    // [Header("Cut Visualizer")]
    // [Tooltip("Referencia al visualizador de cortes para mostrar efectos visuales.")]
    // private CutVisualizer cutVisualizer;

    // private void Start()
    // {
    //     cutVisualizer = GetComponent<CutVisualizer>();
    // }

    /// <summary>
    /// Realiza el corte del objeto actualmente asignado en la estación.
    /// </summary>
    [ContextMenu("SliceObject")]
    public void SliceObject()
    {
        if (objectToCut == null)
        {
            return;
        }

        // if (cutVisualizer != null)
        // {
        //     cutVisualizer.HideHologram();
        // }

        GameObject originalObject = objectToCut;

        MeshFilter meshFilter = originalObject.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError($"[SlicingStation] No se encontró un MeshFilter en '{originalObject.name}' o sus hijos. No se puede cortar.");
            return;
        }

        Mesh originalMesh = meshFilter.mesh;
        MeshRenderer originalRenderer = meshFilter.GetComponent<MeshRenderer>();
        if (originalRenderer == null)
        {
            Debug.LogError($"[SlicingStation] El objeto '{meshFilter.gameObject.name}' tiene un MeshFilter pero no un MeshRenderer. No se puede cortar.");
            return;
        }
        
        Material[] originalMaterials = originalRenderer.materials;
        Transform meshTransform = meshFilter.transform;
        Vector3 worldCutUpAxis = originalObject.transform.TransformDirection(cutUpAxis);
        Vector3 localCutUpAxis = meshTransform.InverseTransformDirection(worldCutUpAxis);
        MeshSlicer slicer = new MeshSlicer(); 
        List<Mesh> sliceMeshes = slicer.Slice(originalMesh, Vector3.zero, localCutUpAxis, sliceCount);

        List<GameObject> newPieces = new List<GameObject>();
        foreach (Mesh sliceMesh in sliceMeshes)
        {
            GameObject newPiece = CreateSliceGameObject(sliceMesh, originalMaterials, meshTransform);
            if(newPiece != null)
            {
                newPieces.Add(newPiece);
            }
        }
        
        if (gameManager != null)
        {
            gameManager.OnCutComplete(originalObject, newPieces, newPieces.Count);
        }

        Destroy(originalObject);
        objectToCut = null;
    }

    /// <summary>
    /// Crea un GameObject a partir de un mesh cortado y aplica materiales, colisión y físicas.
    /// </summary>
    private GameObject CreateSliceGameObject(Mesh sliceMesh, Material[] materials, Transform originalTransform)
    {
        if (sliceMesh.vertexCount == 0) return null;

        GameObject slice = new GameObject($"Slice");
        
        slice.transform.position = originalTransform.position;
        slice.transform.rotation = originalTransform.rotation;
        slice.transform.localScale = originalTransform.localScale;

        slice.AddComponent<MeshFilter>().mesh = sliceMesh;
        slice.AddComponent<MeshRenderer>().materials = materials;
        
        var collider = slice.AddComponent<MeshCollider>();
        collider.convex = true;
        
        var rb = slice.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        
        slice.AddComponent<EnablePhysicsDelay>();

        return slice; 
    }
}