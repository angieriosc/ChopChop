using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Muestra un texto flotante con el precio del item y se desvanece.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class FloatingPriceText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;
    
    private TextMeshPro textMesh;
    private Color originalColor;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
        }
    }
    
    /// <summary>
    /// Inicializa el texto flotante con un precio.
    /// </summary>
    public void Initialize(float price, Vector3 position)
    {
        transform.position = position;
        
        // Configurar el texto
        textMesh.text = $"-${price:F2}";
        textMesh.fontSize = 8;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = new Color(1f, 0.2f, 0.2f, 1f); // Rojo brillante
        
        // Configurar el rectángulo
        textMesh.rectTransform.sizeDelta = new Vector2(5, 2);
        
        // Asegurar que el sorting layer sea visible
        textMesh.sortingOrder = 100;
        
        originalColor = textMesh.color;
        
        Debug.Log($"💰 Texto flotante creado: {textMesh.text} en posición {position}");
        
        StartCoroutine(FloatAndFade());
    }
    
    /// <summary>
    /// Hace que el texto flote hacia arriba y se desvanezca.
    /// </summary>
    private IEnumerator FloatAndFade()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            
            // Flotar hacia arriba
            transform.position = startPos + Vector3.up * (floatSpeed * elapsed);
            
            // Hacer que siempre mire a la cámara
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                                Camera.main.transform.rotation * Vector3.up);
            }
            
            // Fade out
            float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}