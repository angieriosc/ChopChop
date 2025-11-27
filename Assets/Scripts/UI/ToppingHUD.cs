using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ToppingHUD : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        [Tooltip("ID del topping (ej: '1', '2', '3') o la key directa si es la masa")]
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

    [Header("Botón de salida")]
    [SerializeField] private Button exitButton;

    private void Start()
    {
        if (exitButton != null) exitButton.onClick.AddListener(ExitMenu);
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
    /// Refresca los contadores buscando la cantidad REAL en el inventario.
    /// </summary>
    public void Refresh()
    {
        if (toppingManager == null) return;

        foreach (SlotUI slot in slots)
        {
            string finalKey = "";

            if (slot.toppingId == toppingManager.BaseDoughInventoryKey)
            {
                finalKey = toppingManager.BaseDoughInventoryKey;
            }

            else
            {
                finalKey = toppingManager.GetInventoryKeyById(slot.toppingId);
            }

            if (CuttingInventory.Instance != null && !string.IsNullOrEmpty(finalKey))
            {
                int qty = CuttingInventory.Instance.GetQuantity(finalKey);
                
                if (slot.quantityText != null) slot.quantityText.text = qty.ToString();
            }
            else
            {
                // Si no hay key (es salsa infinita) o no hay inventario, ponemos guión para marcar infinito
                if (slot.quantityText != null) slot.quantityText.text = "-";
            }
        }
    }

    private void ExitMenu()
    {
        if (exitButton != null)
        {
            Animator anim = exitButton.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("isPressed", false);
                anim.SetTrigger("Normal");
                anim.SetInteger("state", 0);
            }
        }

        if(toppingCanvas != null) toppingCanvas.SetActive(false);
        if(_playerCamera != null) _playerCamera.gameObject.SetActive(true);
        if(_stationCamera != null) _stationCamera.gameObject.SetActive(false);
        ToppingLock.IsLocked = false;
    }
}