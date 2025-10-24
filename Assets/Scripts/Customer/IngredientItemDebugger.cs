using UnityEngine;
using UnityEngine.UI;

public class IngredientItemDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔍 === INSPECCIONANDO INGREDIENT ITEM ===");
        Debug.Log("Nombre: " + gameObject.name);
        Debug.Log("Activo: " + gameObject.activeSelf);
        
        // Verificar RectTransform
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log("RectTransform: " + rt.rect.width + "x" + rt.rect.height);
            Debug.Log("Posición: " + rt.anchoredPosition);
        }
        
        // Buscar Icon
        Transform iconT = transform.Find("Icon");
        if (iconT != null)
        {
            Debug.Log("✅ Icon encontrado - Activo: " + iconT.gameObject.activeSelf);
            Image img = iconT.GetComponent<Image>();
            if (img != null)
            {
                Debug.Log("  Image enabled: " + img.enabled);
                Debug.Log("  Sprite: " + (img.sprite != null ? img.sprite.name : "NULL"));
                Debug.Log("  Color: " + img.color);
            }
            RectTransform iconRT = iconT.GetComponent<RectTransform>();
            if (iconRT != null)
            {
                Debug.Log("  Tamaño: " + iconRT.rect.width + "x" + iconRT.rect.height);
                Debug.Log("  Posición: " + iconRT.anchoredPosition);
            }
        }
        else
        {
            Debug.LogError("❌ Icon NO encontrado!");
        }
        
        // Buscar Name
        Transform nameT = transform.Find("Name");
        if (nameT != null)
        {
            Debug.Log("✅ Name encontrado - Activo: " + nameT.gameObject.activeSelf);
            Text txt = nameT.GetComponent<Text>();
            TMPro.TextMeshProUGUI tmp = nameT.GetComponent<TMPro.TextMeshProUGUI>();
            
            if (txt != null)
            {
                Debug.Log("  Text encontrado - Enabled: " + txt.enabled);
                Debug.Log("  Texto: " + txt.text);
                Debug.Log("  Color: " + txt.color);
                Debug.Log("  Font Size: " + txt.fontSize);
            }
            else if (tmp != null)
            {
                Debug.Log("  TextMeshPro encontrado - Enabled: " + tmp.enabled);
                Debug.Log("  Texto: " + tmp.text);
                Debug.Log("  Color: " + tmp.color);
                Debug.Log("  Font Size: " + tmp.fontSize);
            }
            else
            {
                Debug.LogError("  ❌ No tiene Text ni TextMeshPro!");
            }
        }
        else
        {
            Debug.LogError("❌ Name NO encontrado!");
        }
    }
}