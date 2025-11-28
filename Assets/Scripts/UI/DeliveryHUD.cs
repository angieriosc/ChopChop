using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DeliveryHUD : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        [Tooltip("Índice de la rebanada en el inventario (0, 1, 2...)")]
        public int sliceIndex;

        [Tooltip("Texto que mostrará la cantidad")]
        public TMP_Text quantityText;

        [Tooltip("El botón visual en la UI que el jugador presiona")]
        public Button selectButton;
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

        foreach (var slot in slots)
        {
            if (slot.selectButton != null)
            {
                // Limpiamos listeners viejos para no duplicar
                slot.selectButton.onClick.RemoveAllListeners(); 

                // Creamos una copia local del índice para evitar errores de clausura en el loop
                int indexToSelect = slot.sliceIndex; 

                // Le decimos al botón: "Cuando te piquen, avísale al Manager"
                slot.selectButton.onClick.AddListener(() => SelectPizzaOption(indexToSelect));
            }
        }
    }

    private void SelectPizzaOption(int index)
    {
        if (deliveryManager != null)
        {
            Debug.Log($"[DeliveryHUD] Botón de UI presionado para índice {index}");
            deliveryManager.SelectSlice(index);
        }
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

    private void RefreshHUD()
    {
        if (deliveryManager == null) return;

        foreach (SlotUI slot in slots)
        {
            if (slot.quantityText == null) continue;
            int qty = deliveryManager.GetInventoryQuantity(slot.sliceIndex);
            slot.quantityText.text = qty.ToString();
        }
    }

    private void ExitMenu()
    {
        DeliveryLock.IsLocked = false;

        if (exitButton != null)
        {
            Animator anim = exitButton.GetComponent<Animator>();
            if(anim != null)
            {
                anim.SetBool("isPressed", false);
                anim.SetTrigger("Normal");
                anim.SetInteger("state", 0);
            }
        }

        deliveryCanvas.SetActive(false);
        if (_playerCamera != null) _playerCamera.gameObject.SetActive(true);
        if (_stationCamera != null) _stationCamera.gameObject.SetActive(false);

        // Si usas el singleton del manager, también podrías acceder por ahí
        if(deliveryManager != null) deliveryManager.SetEnabled(false);
    }
}