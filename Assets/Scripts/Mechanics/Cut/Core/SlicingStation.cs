using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estación de corte V5 (Optimizada con Corrutinas).
/// - Usa Corrutinas para evitar congelar la pantalla durante cortes complejos.
/// - Soporta fusión de mallas grandes y Pizza vs Masa.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SlicingStation : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    [Header("Spawning")]
    [SerializeField] private Transform itemSpawnPoint;
    [SerializeField] private GameObject doughPrefabToSpawn;

    [Header("Player, UI & Camera")]
    [SerializeField] private GameObject sliceControlsPanel;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private FollowPlayer cameraFollow;
    [SerializeField] private Camera stationCamera;
    [SerializeField] private CameraController cameraController;

    private Camera previousCamera;

    [SerializeField] private KeyCode exitKey = KeyCode.Q;
    private bool playerLocked;
    private GameObject originalBowlObject;
    private CuttableItemData currentItemData;
    private bool isSlicing = false; // Para evitar doble corte

    [Header("Slice Settings")]
    [Range(2, 12)]
    public int sliceCount = 6;

    [Header("Cutting Axis (Local)")]
    public Vector3 cutUpAxis = Vector3.up;

    [Header("Object to Cut")]
    public GameObject objectToCut;

    private void Awake()
    {
        if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (cameraFollow == null) cameraFollow = FindFirstObjectByType<FollowPlayer>();
        if (cameraController == null) cameraController = FindFirstObjectByType<CameraController>();
    }

    private void Update()
    {
        // Bloqueamos la salida si estamos en medio del proceso de corte (isSlicing)
        if (playerLocked && !isSlicing && Input.GetKeyDown(exitKey))
        {
            UnlockPlayer();
        }
    }

    public bool IsAvailable() => objectToCut == null;

    public void EnterStation()
    {
        if (IsAvailable()) LockPlayer();
    }

    public bool AssignItemToStation(GameObject itemFromPlayer)
    {
        if (objectToCut != null) return false;

        this.currentItemData = itemFromPlayer.GetComponent<CuttableItemData>();
        
        if (this.currentItemData == null || this.currentItemData.sliceResultPrefab == null)
        {
            Debug.LogError($"[SlicingStation] Error en CuttableItemData del objeto '{itemFromPlayer.name}'.");
        }

        Transform spawnTransform = (itemSpawnPoint != null) ? itemSpawnPoint : this.transform;
        
        BakeableIngredient bakeData = itemFromPlayer.GetComponent<BakeableIngredient>();

        if (bakeData != null)
        {
            this.objectToCut = itemFromPlayer;
            this.objectToCut.transform.SetParent(null);
            this.objectToCut.transform.position = spawnTransform.position;
            this.objectToCut.transform.rotation = spawnTransform.rotation;
            
            var rb = this.objectToCut.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            this.originalBowlObject = null;
        }
        else
        {
            if (doughPrefabToSpawn == null) return false;

            GameObject doughObject = Instantiate(
                doughPrefabToSpawn,
                spawnTransform.position,
                spawnTransform.rotation
            );

            this.objectToCut = doughObject;
            this.originalBowlObject = itemFromPlayer;
            this.originalBowlObject.SetActive(false);
        }

        LockPlayer();
        return true;
    }

    // ---------------------------------------------------------
    // CAMBIO PRINCIPAL: Lógica Asíncrona (Corrutina)
    // ---------------------------------------------------------

    [ContextMenu("SliceObject")]
    public void SliceObject()
    {
        // Evitamos llamar múltiples veces mientras ya se está cortando
        if (isSlicing || objectToCut == null) return;
        
        StartCoroutine(SliceObjectRoutine());
    }

    private IEnumerator SliceObjectRoutine()
    {
        isSlicing = true;
        GameObject originalObject = objectToCut;

        // 1. Fusión de mallas
        CombineToppingsIntoMesh(originalObject);
        yield return null; 

        MeshFilter meshFilter = originalObject.GetComponent<MeshFilter>();
        if (meshFilter == null) 
        {
             isSlicing = false;
             yield break;
        }

        Mesh originalMesh = meshFilter.mesh;
        MeshRenderer originalRenderer = meshFilter.GetComponent<MeshRenderer>();
        Material[] originalMaterials = (originalRenderer != null) ? originalRenderer.materials : null;
        Transform meshTransform = meshFilter.transform;
        
        Vector3 worldCutUpAxis = originalObject.transform.TransformDirection(cutUpAxis);
        Vector3 localCutUpAxis = meshTransform.InverseTransformDirection(worldCutUpAxis);
        
        // 2. Calcular cortes
        MeshSlicer slicer = new MeshSlicer(); 
        List<Mesh> sliceMeshes = slicer.Slice(originalMesh, Vector3.zero, localCutUpAxis, sliceCount);
        
        yield return null;

        // 3. Generar objetos en "SECRETO" (Invisibles e Intangibles)
        List<GameObject> newPieces = new List<GameObject>();
        
        float totalDuration = 3.0f; 
        float delayPerPiece = 0f;

        if (sliceMeshes.Count > 0)
        {
            delayPerPiece = totalDuration / sliceMeshes.Count;
        }

        foreach (Mesh sliceMesh in sliceMeshes)
        {
            // Creamos la pieza (el cálculo pesado de física ocurre aquí)
            GameObject newPiece = CreateSliceGameObject(sliceMesh, originalMaterials, meshTransform);
            
            if(newPiece != null)
            {
                // --- TRUCO DE MAGIA ---
                // La desactivamos inmediatamente. 
                // 1. No se ve.
                // 2. No choca con la original.
                // 3. No se cae por la gravedad.
                newPiece.SetActive(false); 
                
                newPieces.Add(newPiece);
            }

            // Seguimos esperando para no congelar la PC, pero el jugador no ve nada raro
            yield return new WaitForSeconds(delayPerPiece);
        }
        
        // 4. EL GRAN REVEAL (Swap)
        
        // A) Desaparecemos la original
        DisableOriginalObject(originalObject);
        
        // B) Aparecemos todas las rebanadas al mismo tiempo
        foreach (var piece in newPieces)
        {
            if (piece != null) piece.SetActive(true);
        }

        // 5. Finalizar lógica de juego
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
        }

        Destroy(originalObject);
        objectToCut = null;

        if (originalBowlObject != null)
        {
            Destroy(originalBowlObject);
            originalBowlObject = null;
        }

        currentItemData = null;
        isSlicing = false; 
    }

    /// <summary>
    /// Apaga renderers y colliders del objeto original.
    /// </summary>
    private void DisableOriginalObject(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
    }

    private void CombineToppingsIntoMesh(GameObject parentObj)
    {
        MeshFilter[] filters = parentObj.GetComponentsInChildren<MeshFilter>();
        MeshFilter parentFilter = parentObj.GetComponent<MeshFilter>();
        
        if (parentFilter == null) return;

        List<CombineInstance> combiners = new List<CombineInstance>();
        List<Material> materials = new List<Material>();

        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            
            // Verificación de seguridad
            if (!mf.sharedMesh.isReadable)
            {
                Debug.LogError($"[SlicingStation] La malla '{mf.name}' no es Read/Write Enabled.");
            }

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null) continue;

            Matrix4x4 transformMatrix = (mf == parentFilter) 
                ? Matrix4x4.identity 
                : parentObj.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;

            for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = mf.sharedMesh;
                ci.subMeshIndex = i;
                ci.transform = transformMatrix;
                combiners.Add(ci);

                if (i < mr.sharedMaterials.Length)
                    materials.Add(mr.sharedMaterials[i]);
                else
                    materials.Add(mr.sharedMaterials[mr.sharedMaterials.Length - 1]);
            }

            if (mf != parentFilter)
            {
                mf.gameObject.SetActive(false);
            }
        }

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
        finalMesh.CombineMeshes(combiners.ToArray(), false, true); 
        
        parentFilter.mesh = finalMesh;
        parentObj.GetComponent<MeshRenderer>().materials = materials.ToArray();
        
        finalMesh.RecalculateBounds();
        finalMesh.RecalculateNormals();
    }

    private GameObject CreateSliceGameObject(Mesh sliceMesh, Material[] materials, Transform originalTransform)
    {
        if (sliceMesh.vertexCount == 0) return null;
        GameObject slice = new GameObject($"Slice");
        
        slice.transform.position = originalTransform.position;
        slice.transform.rotation = originalTransform.rotation;
        slice.transform.localScale = originalTransform.localScale;
        
        slice.AddComponent<MeshFilter>().mesh = sliceMesh;
        slice.AddComponent<MeshRenderer>().materials = materials;
        
        // ATENCIÓN: MeshCollider Convexo es lento. 
        // Si sigue lento, cambia esto por BoxCollider temporalmente.
        var collider = slice.AddComponent<MeshCollider>();
        collider.convex = true; 
        
        var rb = slice.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        
        slice.AddComponent<EnablePhysicsDelay>();
        return slice; 
    }

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

    public void UnlockPlayer()
    {
        if (objectToCut != null)
        {
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