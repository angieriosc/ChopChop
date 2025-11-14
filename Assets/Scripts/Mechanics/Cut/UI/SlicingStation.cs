using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estación de corte que realiza el slicing en el objeto que se le asigne.
/// También maneja el bloqueo del jugador y la interacción de entrada/salida.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SlicingStation : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField]
    [Tooltip("Referencia al GameManager para notificar cuando se completa un corte.")]
    private GameManager gameManager;

    [Header("Spawning")]
    [SerializeField]
    [Tooltip("Punto en la escena donde se generará el ingrediente.")]
    private Transform itemSpawnPoint;

    [SerializeField]
    [Tooltip("Prefab de 'solo masa' que se genera cuando se coloca el bowl.")]
    private GameObject doughPrefabToSpawn;

    [Header("Player, UI & Camera")]
    [SerializeField]
    [Tooltip("Panel de UI con los controles de corte.")]
    private GameObject sliceControlsPanel;

    [SerializeField]
    [Tooltip("Script de movimiento del jugador.")]
    private PlayerMovement playerMovement;

    [SerializeField]
    [Tooltip("Script que hace que la cámara siga al jugador.")]
    private FollowPlayer cameraFollow;
    
    [SerializeField]
    [Tooltip("Cámara que se activa al usar esta estación.")]
    private Camera stationCamera;

    [SerializeField]
    [Tooltip("Controlador central de cámaras (usualmente en el Player).")]
    private CameraController cameraController;

    private Camera previousCamera;

    [SerializeField]
    [Tooltip("Tecla para salir de la estación.")]
    private KeyCode exitKey = KeyCode.Q;
    private bool playerLocked;
    private GameObject originalBowlObject;
    private CuttableItemData currentItemData;

    [Header("Slice Settings")]
    [Range(2, 12)]
    [Tooltip("Cantidad de cortes que se harán al objeto.")]
    public int sliceCount = 6;

    [Header("Cutting Axis (Local)")]
    [Tooltip("Eje local alrededor del cual se realizarán los cortes (relativo al objeto padre).")]
    public Vector3 cutUpAxis = Vector3.up;

    [Header("Object to Cut")]
    [Tooltip("Referencia al objeto actualmente asignado para cortar.")]
    public GameObject objectToCut;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<FollowPlayer>();

        if (cameraController == null)
            cameraController = FindFirstObjectByType<CameraController>();
    }

    /// <summary>
    /// Revisa si el jugador presiona la tecla de salida mientras está bloqueado.
    /// </summary>
    private void Update()
    {
        if (playerLocked && Input.GetKeyDown(exitKey))
        {
            UnlockPlayer();
        }
    }

    /// <summary>
    /// Verifica si la estación está disponible (no tiene un objeto).
    /// </summary>
    public bool IsAvailable() => objectToCut == null;

    /// <summary>
    /// Llamado por PlayerPickup para entrar a la estación con manos vacías.
    /// </summary>
    public void EnterStation()
    {
        if (IsAvailable())
        {
            LockPlayer();
        }
    }

    /// <summary>
    /// Llamado por PlayerPickup al colocar un item.
    /// Intercambia el bowl por la masa y bloquea al jugador.
    /// </summary>
    public bool AssignItemToStation(GameObject itemFromPlayer)
    {
        if (objectToCut != null)
        {
            Debug.LogWarning("[SlicingStation] La estación ya está llena.");
            return false;
        }

        this.currentItemData = itemFromPlayer.GetComponent<CuttableItemData>();
        
        if (this.currentItemData == null)
        {
            Debug.LogError($"[SlicingStation] El objeto '{itemFromPlayer.name}' no tiene el script 'CuttableItemData'. No se podrán guardar las rebanadas.", itemFromPlayer);
        }
        else if (this.currentItemData.sliceResultPrefab == null)
        {
            Debug.LogError($"[SlicingStation] 'CuttableItemData' en '{itemFromPlayer.name}' no tiene un 'sliceResultPrefab' asignado.", itemFromPlayer);
        }

        if (doughPrefabToSpawn == null)
        {
            Debug.LogError("[SlicingStation] 'doughPrefabToSpawn' no está asignado.");
            return false;
        }

        Transform spawnTransform = (itemSpawnPoint != null) ? itemSpawnPoint : this.transform;

        GameObject doughObject = Instantiate(
            doughPrefabToSpawn,
            spawnTransform.position,
            spawnTransform.rotation
        );

        this.objectToCut = doughObject;
        
        this.originalBowlObject = itemFromPlayer;
        this.originalBowlObject.SetActive(false);

        LockPlayer();
        return true;
    }

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

        GameObject originalObject = objectToCut;
        MeshFilter meshFilter = originalObject.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError($"[SlicingStation] No se encontró MeshFilter en '{originalObject.name}'.");
            return;
        }

        Mesh originalMesh = meshFilter.mesh;
        MeshRenderer originalRenderer = meshFilter.GetComponent<MeshRenderer>();
        if (originalRenderer == null)
        {
            Debug.LogError($"[SlicingStation] '{meshFilter.gameObject.name}' no tiene MeshRenderer.");
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
            gameManager.OnCutComplete(originalObject, newPieces, newPieces.Count, this);
        }

        if (currentItemData != null && currentItemData.sliceResultPrefab != null)
        {
            if (CuttingInventory.Instance != null)
            {
                string key = currentItemData.sliceResultPrefab.name;
                GameObject prefab = currentItemData.sliceResultPrefab;
                int amount = newPieces.Count; 
                
                CuttingInventory.Instance.AddSlices(key, prefab, amount);
            }
            else
            {
                Debug.LogWarning("[SlicingStation] CuttingInventory.Instance no encontrado. No se pudo añadir al inventario.");
            }
        }
        else
        {
            Debug.LogWarning("[SlicingStation] No se encontró 'currentItemData' o 'sliceResultPrefab'. Las rebanadas no se guardaron en el inventario.");
        }

        Destroy(originalObject);
        objectToCut = null;

        if (originalBowlObject != null)
        {
            Destroy(originalBowlObject);
            originalBowlObject = null;
        }

        currentItemData = null;
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

    /// <summary>
    /// Bloquea el movimiento del jugador, muestra UI y cambia la cámara.
    /// </summary>
    public void LockPlayer()
    {
        if (playerLocked) return;
        playerLocked = true;

        if (sliceControlsPanel != null) sliceControlsPanel.SetActive(true);

        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraFollow != null) cameraFollow.enabled = false;

        if (cameraController != null && stationCamera != null)
        {
            previousCamera = cameraController.GetActiveCamera();
            cameraController.ActivateCamera(stationCamera);
        }
    }

    /// <summary>
    /// Desbloquea al jugador, oculta UI y restaura la cámara.
    /// </summary>
    public void UnlockPlayer()
    {
        if (objectToCut != null)
        {
            Debug.Log("Corte cancelado. Devolviendo el objeto original.");
            Destroy(objectToCut);
            objectToCut = null;

            if (originalBowlObject != null)
            {
                PlayerPickup pickup = FindFirstObjectByType<PlayerPickup>();
                if(pickup != null)
                {
                    StartCoroutine(ReturnObjectToPlayerNextFrame(originalBowlObject, pickup));
                }
                originalBowlObject = null;
            }
            currentItemData = null;
        }

        if (!playerLocked) return;
        playerLocked = false;

        if (sliceControlsPanel != null) sliceControlsPanel.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraFollow != null) cameraFollow.enabled = true;

        if (cameraController != null && previousCamera != null)
        {
            cameraController.ActivateCamera(previousCamera);
        }
        
        if (gameManager != null)
        {
            gameManager.ResetBoard();
        }
    }

    /// <summary>
    /// Espera un frame antes de devolver el objeto al jugador.
    /// Esto evita el conflicto de input de 'soltar' (Q) y 'salir' (Q).
    /// </summary>
    private IEnumerator ReturnObjectToPlayerNextFrame(GameObject objectToReturn, PlayerPickup playerPickup)
    {
        yield return null;

        if (objectToReturn != null && playerPickup != null)
        {
            objectToReturn.SetActive(true);
            playerPickup.GrabObject(objectToReturn);
        }
        else if (objectToReturn != null)
        {
            objectToReturn.SetActive(true);
        }
    }
}