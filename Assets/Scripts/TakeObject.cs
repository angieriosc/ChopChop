using UnityEngine;

public class TakeObject : MonoBehaviour
{
    [Header("Configuración")]
    public Animator animator;                 // Asigna el Animator del Player
    public Transform customMount;             // Opcional: un Empty en la mano (HoldPoint)

    [Header("Ajustes en la mano")]
    public Vector3 localPositionOffset;       // Ajusta después de probar
    public Vector3 localEulerOffset;          // Ajusta después de probar

    [HideInInspector] public GameObject currentItem;

    Transform GetMountPoint()
    {
        if (customMount != null) return customMount;
        if (animator != null && animator.avatar && animator.avatar.isHuman)
            return animator.GetBoneTransform(HumanBodyBones.RightHand);
        return transform;
    }

    /// <summary>
    /// Intenta tomar un objeto si es Grabbable.
    /// </summary>
    public void TryGrab(GameObject target)
    {
        if (target == null) return;

        InteractableObject interactable = target.GetComponent<InteractableObject>();

        if (interactable == null)
        {
            Debug.LogWarning($"El objeto {target.name} no tiene componente InteractableObject.");
            return;
        }

        // Verifica si el objeto es grabbable
        if (!interactable.HasCapability(ObjectCapabilities.Grabbable))
        {
            Debug.Log($"El objeto {target.name} no se puede agarrar (no es Grabbable).");
            return;
        }

        // Si ya hay algo en la mano, no permite agarrar otro
        if (currentItem != null)
        {
            Debug.Log("Ya tienes un objeto en la mano. Suéltalo antes de agarrar otro.");
            return;
        }

        Attach(target);
        Debug.Log($"Has agarrado: {target.name}");
    }

    /// <summary>
    /// Intenta soltar el objeto actual si es Droppable.
    /// </summary>
    public void TryDrop()
    {
        if (currentItem == null)
        {
            Debug.Log("No tienes ningún objeto en la mano para soltar.");
            return;
        }

        InteractableObject interactable = currentItem.GetComponent<InteractableObject>();

        if (interactable == null || !interactable.HasCapability(ObjectCapabilities.Droppable))
        {
            Debug.Log($"El objeto {currentItem.name} no se puede soltar (no es Droppable).");
            return;
        }

        Detach();
        Debug.Log("Has soltado el objeto.");
    }

    /// <summary>
    /// Monta un objeto en la mano.
    /// </summary>
    public void Attach(GameObject prefabOrInstance)
    {
        if (prefabOrInstance == null) return;

        if (currentItem != null) Destroy(currentItem);

        Transform mount = GetMountPoint();

        GameObject go;

        // Si el objeto ya está en la escena, lo reubica; si es un prefab, lo instancia
        if (prefabOrInstance.scene.IsValid())
        {
            go = prefabOrInstance;
            go.transform.SetParent(mount, worldPositionStays: false);
        }
        else
        {
            go = Instantiate(prefabOrInstance, mount);
        }

        // Ajustes de posición y orientación
        go.transform.localPosition = localPositionOffset;
        go.transform.localEulerAngles = localEulerOffset;
        go.transform.localScale = Vector3.one;

        // Desactivar física y colisiones
        foreach (var rb in go.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
        foreach (var col in go.GetComponentsInChildren<Collider>()) col.enabled = false;

        currentItem = go;
    }

    /// <summary>
    /// Suelta (desmonta) el objeto actual.
    /// </summary>
    public void Detach()
    {
        if (currentItem == null) return;

        currentItem.transform.SetParent(null);
        foreach (var col in currentItem.GetComponentsInChildren<Collider>()) col.enabled = true;

        currentItem = null;
    }
}
