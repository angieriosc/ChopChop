using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estación de corte Final.
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
    [SerializeField] private SliceControlsUI sliceControlsUI; 
    
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
        if (sliceControlsUI == null) sliceControlsUI = FindFirstObjectByType<SliceControlsUI>();
    }

    /// <summary>
    /// Actualiza cada frame para detectar la entrada del jugador para salir de la estación.
    /// </summary>
    private void Update()
    {
        if (playerLocked && !isSlicing && Input.GetKeyDown(exitKey))
        {
            UnlockPlayer();
        }
    }

    /// <summary>
    /// Verifica si la estación de corte está disponible para un nuevo objeto.
    /// </summary>
    public bool IsAvailable() => objectToCut == null;

    /// <summary>
    /// Maneja la entrada del jugador para entrar en la estación de corte.
    /// </summary>
    public void EnterStation()
    {
        if (IsAvailable()) LockPlayer();
    }

    /// <summary>
    /// Asigna un objeto a la estación de corte para ser cortado.
    /// </summary>
    public bool AssignItemToStation(GameObject itemFromPlayer)
    {
        if (objectToCut != null) return false;

        this.currentItemData = itemFromPlayer.GetComponent<CuttableItemData>();
        
        if (this.currentItemData == null)
        {
            Debug.LogError($"[SlicingStation] Error: '{itemFromPlayer.name}' no tiene CuttableItemData.");
            return false;
        }

        Transform spawnTransform = (itemSpawnPoint != null) ? itemSpawnPoint : this.transform;
        
        bool isBowl = itemFromPlayer.CompareTag("Bowl"); 

        if (isBowl)
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
        else
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
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            this.originalBowlObject = null;
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
    /// Rutina para cortar el objeto asignado.
    /// </summary>
    private IEnumerator SliceObjectRoutine()
    {
        isSlicing = true;
        GameObject originalObject = objectToCut;

        if (originalObject == null)
        {
            isSlicing = false;
            UnlockPlayer();
            yield break;
        }

        bool isPizza = originalObject.GetComponent<BakeableIngredient>() != null;
        CustomerManager customerManager = FindFirstObjectByType<CustomerManager>();

        if (isPizza && customerManager != null && customerManager.ActiveRecipe != null)
        {
            int targetSlices = customerManager.ActiveRecipe.pizzaSlices; 

            if (this.sliceCount != targetSlices)
            {
                Debug.Log($"<color=red>¡ERROR DE CORTE!</color> Pizza cortada en {sliceCount}, la receta pedía {targetSlices}.");
                
                if (PatienceManager.Instance != null)
                {
                    PatienceManager.Instance.ApplyPenalty(PenaltyType.WrongCut);
                }
            }
            else
            {
                Debug.Log("<color=green>¡Corte de Pizza Perfecto!</color>");
            }
        }

        if (isPizza) CombineToppingsIntoMesh(originalObject);
        yield return null; 

        MeshFilter meshFilter = originalObject.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null) 
        {
             Debug.LogError($"[SlicingStation] MeshFilter no encontrado en {originalObject.name}.");
             isSlicing = false;
             yield break;
        }

        Mesh originalMesh = meshFilter.mesh;
        MeshRenderer originalRenderer = meshFilter.GetComponent<MeshRenderer>();
        if(originalRenderer == null) originalRenderer = meshFilter.GetComponent<MeshRenderer>();

        Material[] originalMaterials = (originalRenderer != null) ? originalRenderer.materials : null;
        Transform meshTransform = meshFilter.transform;
        
        Vector3 worldCutUpAxis = originalObject.transform.TransformDirection(cutUpAxis);
        Vector3 localCutUpAxis = meshTransform.InverseTransformDirection(worldCutUpAxis);

        MeshSlicer slicer = new MeshSlicer(); 
        List<Mesh> sliceMeshes = slicer.Slice(originalMesh, Vector3.zero, localCutUpAxis, sliceCount);
        
        yield return null;

        List<GameObject> newPieces = new List<GameObject>();
        float totalDuration = 3.0f;
        float delayPerPiece = totalDuration / (sliceMeshes.Count > 0 ? sliceMeshes.Count : 1);
        
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

        float forceForLowCuts = 130f; 
        float forceForHighCuts = 50f; 
        float t = Mathf.InverseLerp(2f, 12f, (float)sliceMeshes.Count);
        float separationPower = Mathf.Lerp(forceForLowCuts, forceForHighCuts, t);
        float nudgeDistance = 0.08f; 
        Vector3 explosionCenter = originalObject.transform.position;

        foreach (var piece in newPieces)
        {
            if (piece == null) continue;
            piece.SetActive(true);
            Collider col = piece.GetComponent<Collider>();
            Vector3 pieceCenter = col.bounds.center;
            Vector3 direction = (pieceCenter - explosionCenter).normalized;
            if (direction == Vector3.zero) direction = Vector3.up;
            piece.transform.position += direction * nudgeDistance;
        }

        Physics.SyncTransforms(); 

        foreach (var piece in newPieces)
        {
            if (piece == null) continue;
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearDamping = 8f; 
            rb.AddExplosionForce(separationPower, explosionCenter, 3f, 0f);
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.1f);

        foreach (var piece in newPieces)
        {
            if (piece != null)
            {
                Rigidbody rb = piece.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero; 
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;          
            }
        }

        yield return new WaitForSeconds(1.5f); 
        
        string recipeKey = "Unknown";
        if (isPizza) 
        {
            if (customerManager != null && customerManager.ActiveRecipe != null)
                recipeKey = customerManager.ActiveRecipe.name;
        }
        else if (currentItemData != null && currentItemData.sliceResultPrefab != null)
        {
            recipeKey = currentItemData.sliceResultPrefab.name;
        }

        if (CuttingInventory.Instance != null)
        {
            CuttingInventory.Instance.AddSlices(recipeKey, null, newPieces.Count);
        }

        PizzaDeliveryManager deliveryManager = PizzaDeliveryManager.Instance;
        
        if (isPizza && deliveryManager != null)
        {
            foreach (GameObject piece in newPieces) deliveryManager.StoreRealSlice(recipeKey, piece);
        }
        else
        {
            foreach (GameObject piece in newPieces) Destroy(piece);
        }

        Destroy(originalObject);
        objectToCut = null;

        if (originalBowlObject != null)
        {
            BowlStation bowlStation = FindFirstObjectByType<BowlStation>();
            if (bowlStation != null) bowlStation.RegisterBowlDestruction();
        }

        currentItemData = null;
        isSlicing = false; 

        if (sliceControlsUI != null)
        {
            sliceControlsUI.ResetControls();
        }

        UnlockPlayer();
    }

    /// <summary>
    /// Desactiva el objeto original después de cortarlo.
    /// </summary>
    private void DisableOriginalObject(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
    }

    /// <summary>
    /// Combina los meshes de los toppings en un solo mesh para optimizar el corte.
    /// 
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
    /// Crea un GameObject para una rebanada con el mesh y materiales dados.
    /// </summary>
    private GameObject CreateSliceGameObject(Mesh sliceMesh, Material[] materials, Transform originalTransform)
    {
        if (sliceMesh.vertexCount == 0) return null;
        GameObject slice = new GameObject($"Slice");
        
        slice.transform.position = originalTransform.position;
        slice.transform.rotation = originalTransform.rotation;
        slice.transform.localScale = originalTransform.lossyScale;
        
        slice.AddComponent<MeshFilter>().mesh = sliceMesh;
        slice.AddComponent<MeshRenderer>().materials = materials;
        
        var collider = slice.AddComponent<MeshCollider>();
        collider.convex = true; 
        
        var rb = slice.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        return slice; 
    }

    /// <summary>
    /// Bloquea al jugador en la estación de corte.
    /// </summary>
    public void LockPlayer()
    {
        if (playerLocked) return;
        playerLocked = true;

        if (sliceControlsUI != null) sliceControlsUI.gameObject.SetActive(true);
        
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

        // CAMBIO 3: Usamos la referencia al script para desactivar su GameObject
        if (sliceControlsUI != null) sliceControlsUI.gameObject.SetActive(false);
        
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