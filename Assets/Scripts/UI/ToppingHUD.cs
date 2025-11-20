using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Actualiza el HUD del inventario/toppings según los límites de la receta.
/// Muestra cuántos toppings quedan disponibles para colocar.
/// </summary>
public class ToppingHUD : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        [Tooltip("ID del topping que este slot representa (ej: 'salsa', 'queso', 'pimiento')")]
        public string toppingId;

        [Tooltip("Referencia al texto que mostrará la cantidad disponible")]
        public TMP_Text quantityText;
    }

    [Header("Referencias")]
    [SerializeField] private PizzaToppingManager toppingManager;

    [Header("Slots visuales del HUD")]
    [SerializeField] private List<SlotUI> slots = new();


    private void OnEnable()
    {
        if (toppingManager != null)
            toppingManager.OnToppingCountsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (toppingManager != null)
            toppingManager.OnToppingCountsChanged -= Refresh;
    }

    /// <summary>
    /// Refresca los contadores visibles de cada topping en la UI.
    /// </summary>
    public void Refresh()
    {
        if (toppingManager == null)
            return;

        foreach (SlotUI slot in slots)
        {
            int used = toppingManager.GetPlacedForTopping(slot.toppingId);
            int max = toppingManager.GetMaxForTopping(slot.toppingId);

            if (max <= 0)
            {
                slot.quantityText.text = "0";
            }
            else
            {
                slot.quantityText.text = $"{max - used}";
            }
        }
    }
}
