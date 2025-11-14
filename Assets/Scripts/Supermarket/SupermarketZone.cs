using UnityEngine;

/// <summary>
/// Define la zona del supermercado y activa la música al entrar.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SupermarketZone : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool playerInside = false;
    
    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player") || other.GetComponent<PlayerCartController>() != null)
        {
            if (!playerInside)
            {
                playerInside = true;
                
                // Activar música de fondo
                if (SupermarketAudioManager.Instance != null)
                {
                    SupermarketAudioManager.Instance.PlaySupermarketAmbience();
                }
                
                if (showDebugLogs)
                {
                    Debug.Log("🏪 Entraste al supermercado");
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player") || other.GetComponent<PlayerCartController>() != null)
        {
            if (playerInside)
            {
                playerInside = false;
                
                // Detener música de fondo
                if (SupermarketAudioManager.Instance != null)
                {
                    SupermarketAudioManager.Instance.StopSupermarketAmbience();
                }
                
                if (showDebugLogs)
                {
                    Debug.Log("🚪 Saliste del supermercado");
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0.5f, 1f, 0.2f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }
}