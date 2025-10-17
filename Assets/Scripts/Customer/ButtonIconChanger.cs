using UnityEngine;
using UnityEngine.UI;

public class ButtonIconChanger : MonoBehaviour
{
    [Header("Button Icons")]
    public Sprite expandedIcon; // Flecha hacia abajo ▼
    public Sprite collapsedIcon; // Flecha hacia arriba ▲
    
    private Image buttonImage;
    
    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogWarning("⚠️ No se encontró componente Image en el botón");
        }
    }
    
    public void SetExpanded(bool isExpanded)
    {
        if (buttonImage == null) return;
        
        if (isExpanded && expandedIcon != null)
        {
            buttonImage.sprite = expandedIcon;
            Debug.Log("🔽 Ícono cambiado a EXTENDIDO");
        }
        else if (!isExpanded && collapsedIcon != null)
        {
            buttonImage.sprite = collapsedIcon;
            Debug.Log("🔼 Ícono cambiado a ENROLLADO");
        }
    }
}