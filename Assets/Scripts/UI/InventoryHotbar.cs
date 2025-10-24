using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryHotbarAnimator : MonoBehaviour
{
    [Header("Inventory Slot Animators (Assign 6)")]
    public Animator[] slotAnimators = new Animator[6];

    [Header("Animator State Names")]
    public string normalState = "Normal";    // The idle/default state name
    public string selectedTrigger = "Select"; // Trigger to play selected animation

    private int currentSlot = -1;

    void Update()
    {
        // Keyboard input 1–6
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }

        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast from camera through mouse pointer
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object has an InventorySlot component
                SlotIndex slot = hit.collider.GetComponent<SlotIndex>();
                if (slot != null)
                {
                    SelectSlot(slot.slotIndex);
                }
            }
        }
    }

    void SelectSlot(int index)
    {
        if (index < 0 || index >= slotAnimators.Length)
            return;

        // Revert previous slot to Normal
        if (currentSlot >= 0 && currentSlot < slotAnimators.Length && slotAnimators[currentSlot] != null)
        {
            slotAnimators[currentSlot].Play(normalState, 0, 0f);
        }

        // Play selected animation on the new slot
        if (slotAnimators[index] != null)
        {
            slotAnimators[index].ResetTrigger(selectedTrigger);
            slotAnimators[index].SetTrigger(selectedTrigger);
        }

        currentSlot = index;
        Debug.Log($"Selected slot: {index + 1}");
    }
}