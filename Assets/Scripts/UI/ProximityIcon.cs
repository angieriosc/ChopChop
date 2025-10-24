using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProximityInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform player;              // The player transform
    public Transform targetObject;        // The object the player approaches
    public GameObject icon;               // Icon that appears near the object
    public Animator iconAnimator;         // Animator for icon animation
    public Button iconButton;             // UI Button for interaction
    public Text uiText;                   // Optional: regular UI text
    public TextMeshProUGUI tmpText;       // Optional: TextMeshPro text

    [Header("Settings")]
    public float detectionRadius = 3f;    // How close the player must be
    [TextArea]
    public string messageWhenNear = "Press E to Interact";
    public string messageWhenFar = "";

    private bool isNear = false;
    private bool isHiding = false;        // Prevents double hide calls during animation

    private void Start()
    {
        if (icon != null)
            icon.SetActive(false);

        if (iconButton != null)
            iconButton.interactable = false;

        if (uiText != null) uiText.text = messageWhenFar;
        if (tmpText != null) tmpText.text = messageWhenFar;
    }

    private void Update()
    {
        if (player == null || targetObject == null)
            return;

        float distance = Vector3.Distance(player.position, targetObject.position);
        bool nowNear = distance <= detectionRadius;

        // Detect if the proximity state changed
        if (nowNear != isNear)
        {
            isNear = nowNear;

            if (isNear)
                OnPlayerEnterRange();
            else
                OnPlayerExitRange();
        }

        // Press E when near
        if (isNear && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }

    private void OnPlayerEnterRange()
    {
        if (icon != null)
            icon.SetActive(true);

        if (iconButton != null)
            iconButton.interactable = true;

        if (uiText != null)
            uiText.text = messageWhenNear;

        if (tmpText != null)
            tmpText.text = messageWhenNear;

        isHiding = false;
    }

    private void OnPlayerExitRange()
    {
        if (icon != null)
        {
            icon.SetActive(false);
        }

        if (iconButton != null)
            iconButton.interactable = false;

        if (uiText != null)
            uiText.text = messageWhenFar;

        if (tmpText != null)
            tmpText.text = messageWhenFar;
    }

    private void OnInteract()
    {
        // Trigger the icon's selected animation
        if (iconAnimator != null)
        {
            iconAnimator.SetTrigger("Pressed"); // Make sure you have a trigger parameter called "Pressed"
        }

        Debug.Log("Interacted with object: " + targetObject.name);
    }

    private System.Collections.IEnumerator DeactivateIconAfterAnimation()
    {
        if (isHiding) yield break; // Prevent overlapping calls
        isHiding = true;

        // Wait for the hide animation to finish (assuming 0.2s duration)
        yield return new WaitForSeconds(0.2f);

        if (!isNear && icon != null)
            icon.SetActive(false);

        isHiding = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetObject.position, detectionRadius);
        }
    }
}