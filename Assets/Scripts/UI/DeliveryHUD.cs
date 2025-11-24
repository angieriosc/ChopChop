using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DeliveryHUD : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        [Tooltip("Índice de la rebanada en el inventario (0,1,2...)")]
        public int sliceIndex;

        [Tooltip("Texto que mostrará la cantidad disponible")]
        public TMP_Text quantityText;
    }

    [Header("Referencias")]
    [SerializeField] private PizzaDeliveryManager deliveryManager;
    [SerializeField] private GameObject deliveryCanvas;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _stationCamera;

    [Header("Slots visuales del HUD")]
    [SerializeField] private List<SlotUI> slots = new();

    [Header("Botón para salir")]
    [SerializeField] private Button exitButton;

    private void Start()
    {
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitMenu);
    }

    private void OnEnable()
    {
        if (deliveryManager != null)
            deliveryManager.OnInventoryChanged += RefreshHUD;

        RefreshHUD();
    }

    private void OnDisable()
    {
        if (deliveryManager != null)
            deliveryManager.OnInventoryChanged -= RefreshHUD;
    }

    /// <summary>
    /// Refresca los contadores visibles del inventario de rebanadas.
    /// </summary>
    private void RefreshHUD()
    {
        if (deliveryManager == null)
            return;

        foreach (SlotUI slot in slots)
        {
            int qty = deliveryManager.GetInventoryQuantity(slot.sliceIndex);
            slot.quantityText.text = qty.ToString();
        }
    }

    /// <summary>
    /// Salir del menú de entrega.
    /// </summary>
    private void ExitMenu()
    {
        if (exitButton != null)
        {
            Animator anim = exitButton.GetComponent<Animator>();
            anim.SetBool("isPressed", false);
            anim.SetTrigger("Normal");
            anim.SetInteger("state", 0);
        }

        deliveryCanvas.SetActive(false);

        _playerCamera.gameObject.SetActive(true);
        _stationCamera.gameObject.SetActive(false);

        // desbloquear movimiento si lo estás usando
        ToppingLock.IsLocked = false;

        // Desactivar entregas
        deliveryManager.SetEnabled(false);
    }
}
