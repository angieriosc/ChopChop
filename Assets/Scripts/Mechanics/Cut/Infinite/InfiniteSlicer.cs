using UnityEngine;
using UnityEngine.InputSystem;

public class InfiniteSlicer : MonoBehaviour
{
    [Header("Object References")]
    public GameObject mainObject;
    public GameObject slicePrefab;
    public Transform spawnPoint;

    [Header("Cut Settings")]
    [Tooltip("How far the main object moves forward after a cut. This should ideally match the thickness of a slice.")]
    public float mainObjectMoveDistance = 0.1f;

    [Tooltip("An extra gap to add between the spawn point and the new slice, pushing it forward.")]
    public float sliceSpacing = 0.02f; // ADDED: Variable for slice separation.

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Cut();
        }
    }

    public void Cut()
    {
        if (mainObject == null || slicePrefab == null || spawnPoint == null)
        {
            Debug.LogError("Slicer references are not set in the Inspector!");
            return;
        }

        // ADDED: Calculate the new slice's position with the spacing offset.
        // It takes the spawn point's position and adds a small forward vector.
        Vector3 sliceSpawnPosition = spawnPoint.position + (spawnPoint.forward * sliceSpacing);

        // CHANGED: The rotation now uses the slice prefab's original rotation instead of the spawn point's.
        Instantiate(slicePrefab, sliceSpawnPosition, slicePrefab.transform.rotation);
        
        // Use the dedicated variable to move the main object.
        mainObject.transform.Translate(Vector3.forward * mainObjectMoveDistance, Space.Self);
    }
}