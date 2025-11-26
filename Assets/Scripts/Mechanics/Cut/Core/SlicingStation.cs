using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estación de corte Final.
/// - Fusiona toppings.
/// - Usa corrutinas para rendimiento.
/// - Integra nombre de receta activa para el inventario.
/// - Realiza swap visual/físico para evitar bugs.
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
    private bool isSlicing = false; 

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
        // Bloqueamos la salida si estamos en medio del proceso de corte
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

    /// <summary>
    ///  Asigna un objeto a la estación para cortarlo.
    /// </summary>
    /// <param name="itemFromPlayer"></param>
    /// <returns></returns>
    public bool AssignItemToStation(GameObject itemFromPlayer)
    {
        if (objectToCut != null) return false;

        this.currentItemData = itemFromPlayer.GetComponent<CuttableItemData>();
        
        if (this.currentItemData == null || this.currentItemData.sliceResultPrefab == null)
        {
            Debug.LogError($"[SlicingStation] Error: El objeto '{itemFromPlayer.name}' no tiene CuttableItemData o sliceResultPrefab.");
        }

        Transform spawnTransform = (itemSpawnPoint != null) ? itemSpawnPoint : this.transform;
        
        // Detectamos si es Pizza (cocinada) o Masa (cruda)
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

    /// <summary>
    /// Inicia el proceso de corte del objeto asignado.
    /// </summary>
    public void SliceObject()
    {
        if (isSlicing || objectToCut == null) return;
        StartCoroutine(SliceObjectRoutine());
    }

    /// <summary>
    /// Corrutina que maneja el proceso de corte.
    /// </summary>
    private IEnumerator SliceObjectRoutine()
    {
        isSlicing = true;
        GameObject originalObject = objectToCut;

        bool isPizza = originalObject.GetComponent<BakeableIngredient>() != null;

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

        MeshSlicer slicer = new MeshSlicer(); 
        List<Mesh> sliceMeshes = slicer.Slice(originalMesh, Vector3.zero, localCutUpAxis, sliceCount);
        
        yield return null;

        List<GameObject> newPieces = new List<GameObject>();
        float totalDuration = 3.0f;
        float delayPerPiece = 0f;

        if (sliceMeshes.Count > 0) delayPerPiece = totalDuration / sliceMeshes.Count;
        
        foreach (Mesh sliceMesh in sliceMeshes)
        {
            GameObject newPiece = CreateSliceGameObject(sliceMesh, originalMaterials, meshTransform);
            if(newPiece != null)
            {
                newPiece.SetActive(false);
                newPieces.Add(newPiece);
            }
            yield return new WaitForSeconds(delayPerPiece);
        }
        
        DisableOriginalObject(originalObject);

        foreach (var piece in newPieces)
        {
            if (piece != null) piece.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f); 
        string recipeKey = "Unknown";
        
        if (isPizza) // Es Pizza
        {
            CustomerManager customerManager = FindFirstObjectByType<CustomerManager>();
            if (customerManager != null && customerManager.ActiveRecipe != null)
                recipeKey = customerManager.ActiveRecipe.name;
        }
        else if (currentItemData != null && currentItemData.sliceResultPrefab != null)
        {
            // Es Masa (Dough)
            recipeKey = currentItemData.sliceResultPrefab.name;
        }

        if (CuttingInventory.Instance != null)
        {
            CuttingInventory.Instance.AddSlices(recipeKey, null, newPieces.Count);
        }

        PizzaDeliveryManager deliveryManager = PizzaDeliveryManager.Instance;
        
        if (isPizza && deliveryManager != null)
        {
            Debug.Log($"[SlicingStation] Guardando rebanadas de PIZZA '{recipeKey}' en DeliveryManager.");
            foreach (GameObject piece in newPieces)
            {
                deliveryManager.StoreRealSlice(recipeKey, piece);
            }
        }
        else
        {
            Debug.Log("[SlicingStation] Destruyendo rebanadas de MASA (no se guardan).");
            foreach (GameObject piece in newPieces)
            {
                Destroy(piece);
            }
        }

        Destroy(originalObject);
        objectToCut = null;

        if (originalBowlObject != null)
        {
            BowlStation bowlStation = FindFirstObjectByType<BowlStation>();
            if (bowlStation != null) bowlStation.RegisterBowlDestruction();
            Destroy(originalBowlObject);
            originalBowlObject = null;
        }

        currentItemData = null;
        isSlicing = false; 

        UnlockPlayer();
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

    /// <summary>
    /// Fusiona mallas de hijos en el padre. Soporta >65k vértices.
    /// </summary>
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

    /// <summary>
    /// Crea un GameObject a partir de una malla de corte.
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
    /// Bloquea al jugador en la estación de corte.
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
    /// Desbloquea al jugador de la estación de corte.
    /// </summary>
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

    /// <summary>
    /// Devuelve el objeto al jugador en el siguiente frame.
    /// </summary>
    /// <param name="objectToReturn">El objeto a devolver.</param>
    /// <param name="playerPickup">El componente PlayerPickup del jugador.</param>
    /// <returns></returns>
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