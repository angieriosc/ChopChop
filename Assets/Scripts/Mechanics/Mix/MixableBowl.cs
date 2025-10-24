using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa un bowl mezclable que requiere ingredientes específicos.
/// Cambia apariencia y partículas según el estado mezclado.
/// </summary>
[RequireComponent(typeof(InteractableObject))]
public class MixableBowl : MonoBehaviour
{
    [Header("Bowl Configuration")]
    [SerializeField] private string bowlName = "Dough Bowl";
    [SerializeField] private bool isMixed = false;

    [Header("Required Ingredients")]
    [SerializeField] private List<string> requiredIngredients = new() { "Harina", "Agua", "Levadura" };
    [SerializeField] public List<string> currentIngredients = new();

    [Header("Visual States")]
    [SerializeField] private GameObject unmixedModel;
    [SerializeField] private GameObject mixedModel;

    [Header("Color Mode (if no separate models)")]
    [SerializeField] private bool useColorChange = true;
    [SerializeField] private Color unmixedColor = new(0.9f, 0.9f, 0.8f);
    [SerializeField] private Color mixedColor = new(0.95f, 0.85f, 0.7f);

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem bubblesEffect;

    private Renderer objectRenderer;
    private InteractableObject interactable;

    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
        AddDefaultCapabilities();
    }

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        LogInitialization();
        UpdateAppearance();
    }

    #region Public Methods

    /// <summary>
    /// Marca el bowl como mezclado y actualiza su apariencia.
    /// </summary>
    public void MarkAsMixed()
    {
        if (isMixed)
        {
            Debug.LogWarning($"⚠️ '{bowlName}' ya estaba mezclado");
            return;
        }

        isMixed = true;
        RemoveMixableCapability();
        UpdateAppearance();
        Debug.Log($"🔄 '{bowlName}' ahora está mezclado");
    }

    /// <summary>
    /// Verifica si los ingredientes actuales son correctos.
    /// </summary>
    public bool HasCorrectIngredients()
    {
        if (currentIngredients.Count != requiredIngredients.Count)
        {
            Debug.LogWarning($"⚠️ Ingredientes incorrectos: {currentIngredients.Count}/{requiredIngredients.Count}");
            return false;
        }

        foreach (var ingredient in requiredIngredients)
        {
            if (!currentIngredients.Contains(ingredient))
            {
                Debug.LogWarning($"⚠️ Falta ingrediente: {ingredient}");
                return false;
            }
        }

        Debug.Log($"✅ Todos los ingredientes correctos: {string.Join(", ", currentIngredients)}");
        return true;
    }

    /// <summary>
    /// Añade un ingrediente al bowl si no estaba presente.
    /// </summary>
    public void AddIngredient(string ingredient)
    {
        if (!currentIngredients.Contains(ingredient))
        {
            currentIngredients.Add(ingredient);
            Debug.Log($"✅ Ingrediente '{ingredient}' agregado ({currentIngredients.Count}/{requiredIngredients.Count})");
        }
        else
        {
            Debug.LogWarning($"⚠️ Ingrediente '{ingredient}' ya estaba en el bowl");
        }
    }

    /// <summary>
    /// Elimina un ingrediente del bowl.
    /// </summary>
    public void RemoveIngredient(string ingredient)
    {
        if (currentIngredients.Contains(ingredient))
        {
            currentIngredients.Remove(ingredient);
            Debug.Log($"❌ Ingrediente '{ingredient}' removido");
        }
    }

    /// <summary>
    /// Limpia el bowl y restaura la capacidad de mezclar.
    /// </summary>
    public void ClearBowl()
    {
        currentIngredients.Clear();
        isMixed = false;
        AddMixableCapability();
        UpdateAppearance();
        Debug.Log($"🧹 Bowl '{bowlName}' limpiado");
    }

    public bool IsMixed() => isMixed;

    public string GetBowlName() => bowlName;

    public List<string> GetCurrentIngredients() => new(currentIngredients);

    public List<string> GetRequiredIngredients() => new(requiredIngredients);

    #endregion

    #region Private Methods

    private void AddDefaultCapabilities()
    {
        if (!interactable.HasCapability(ObjectCapabilities.Mixable))
        {
            interactable.capabilities |= ObjectCapabilities.Mixable;
            Debug.Log($"✅ Mixable capability añadida a '{bowlName}'");
        }

        if (!interactable.HasCapability(ObjectCapabilities.Grabbable))
            interactable.capabilities |= ObjectCapabilities.Grabbable;

        if (!interactable.HasCapability(ObjectCapabilities.Droppable))
            interactable.capabilities |= ObjectCapabilities.Droppable;
    }

    private void RemoveMixableCapability()
    {
        if (interactable.HasCapability(ObjectCapabilities.Mixable))
        {
            interactable.capabilities &= ~ObjectCapabilities.Mixable;
            Debug.Log("🔒 Mixable capability removida (mezclado)");
        }
    }

    private void AddMixableCapability()
    {
        if (!interactable.HasCapability(ObjectCapabilities.Mixable))
        {
            interactable.capabilities |= ObjectCapabilities.Mixable;
        }
    }

    private void UpdateAppearance()
    {
        // Model switching
        if (unmixedModel != null && mixedModel != null)
        {
            unmixedModel.SetActive(!isMixed);
            mixedModel.SetActive(isMixed);
        }

        // Color change
        if (useColorChange && objectRenderer != null)
        {
            objectRenderer.material.color = isMixed ? mixedColor : unmixedColor;
        }

        // Particle effect
        if (bubblesEffect != null)
        {
            if (isMixed && !bubblesEffect.isPlaying) bubblesEffect.Play();
            else if (!isMixed && bubblesEffect.isPlaying) bubblesEffect.Stop();
        }
    }

    private void LogInitialization()
    {
        Debug.Log($"🥣 MixableBowl '{bowlName}' inicializado");
        Debug.Log($"   Mixed state: {isMixed}");
        Debug.Log($"   Capabilities: {interactable.capabilities}");
        Debug.Log($"   Required ingredients: {string.Join(", ", requiredIngredients)}");
        Debug.Log($"   Current ingredients: {string.Join(", ", currentIngredients)}");

        if (GetComponent<Collider>() == null)
            Debug.LogError($"❌ ERROR: '{bowlName}' no tiene Collider!");
    }

    private void OnValidate()
    {
        if (Application.isPlaying) UpdateAppearance();
    }

    #endregion
}
