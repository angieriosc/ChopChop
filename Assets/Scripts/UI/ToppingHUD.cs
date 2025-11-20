using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

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
    [SerializeField] private GameObject toppingCanvas;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _stationCamera;

    [Header("Slots visuales del HUD")]
    [SerializeField] private List<SlotUI> slots = new();

    [Header("Botón de salida de la estación")]
    [SerializeField] private Button exitButton;


    private void Start()
        {
            // Listerner del botón de salir
            if (exitButton != null) {
                exitButton.onClick.AddListener(ExitMenu);
            }
        }
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
            if (slot.toppingId == toppingManager.BaseDoughInventoryKey)
            {
                int doughQty = CuttingInventory.Instance != null 
                    ? CuttingInventory.Instance.GetQuantity(slot.toppingId)
                    : 0;

                slot.quantityText.text = doughQty.ToString();
                continue;
            }

            // 2) Ingredientes normales → usar límites de la receta
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

    /// <summary>
    /// Cierra el menú y desbloquea jugador y cámara.
    /// </summary>
private void ExitMenu()
{
    Animator anim = exitButton.GetComponent<Animator>();

    anim.SetBool("isPressed", false);
    anim.SetTrigger("Normal");
    anim.SetInteger("state", 0);

    toppingCanvas.SetActive(false);
    _playerCamera.gameObject.SetActive(true);
    _stationCamera.gameObject.SetActive(false);
    ToppingLock.IsLocked = false;
}

}
