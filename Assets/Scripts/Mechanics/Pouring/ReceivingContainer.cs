using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Recipiente destino que recibe líquidos de PouringContainer.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class ReceivingContainer : MonoBehaviour
{
    [Header("Capacity (ml)")]
    [SerializeField] private float capacityML = 1000f;

    [Header("UI Text (Optional)")]
    [SerializeField] private TextMeshProUGUI amountText;

    private Dictionary<string, float> ingredients = new Dictionary<string, float>();
    private InteractableObject interactable;

    private float TotalML
    {
        get
        {
            float total = 0f;
            foreach (var ing in ingredients.Values) total += ing;
            return total;
        }
    }

    private void Start()
    {
        interactable = GetComponent<InteractableObject>();

        if (!interactable.HasCapability(ObjectCapabilities.Mixable))
            interactable.capabilities |= ObjectCapabilities.Mixable;

        UpdateVisual();
    }

    public void AddLiquid(string ingredient, float amount)
    {
        if (TotalML + amount > capacityML)
            amount = capacityML - TotalML;

        if (!ingredients.ContainsKey(ingredient)) ingredients[ingredient] = 0f;
        ingredients[ingredient] += amount;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (amountText == null) return;

        string text = $"Total: {TotalML:F0} ml\n";
        foreach (var kvp in ingredients)
            text += $"{kvp.Key}: {kvp.Value:F0} ml\n";

        amountText.text = text;
    }
}
