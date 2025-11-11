using UnityEngine;

/// <summary>
/// A helper script placed on "container" objects (like a bowl with dough).
/// It tells the SlicingStation which "ingredient-only" prefab to spawn 
/// when this container is placed on it.
/// </summary>
public class DoughContainer : MonoBehaviour
{
    [Tooltip("The 'dough-only' prefab to spawn when placed in the station")]
    public GameObject doughPrefabToSpawn;
}