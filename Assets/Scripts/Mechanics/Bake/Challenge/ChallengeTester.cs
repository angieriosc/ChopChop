using UnityEngine;

/// <summary>
/// Script de ayuda para probar el sistema de reto sin necesitar el horno.
/// Adjunta este script a un GameObject y presiona T para iniciar el reto.
/// SOLO PARA TESTING - REMOVER EN BUILD FINAL.
/// </summary>
public class ChallengeTester : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DivisionChallenge _divisionChallenge;
    
    [Header("Configuración de Testing")]
    [SerializeField] private KeyCode _testKey = KeyCode.T;
    [SerializeField] private bool _enableDebugLogs = true;
    
    private void Awake()
    {
        if (_divisionChallenge == null)
        {
            _divisionChallenge = FindObjectOfType<DivisionChallenge>();
            
            if (_divisionChallenge == null)
            {
                Debug.LogError("ChallengeTester: No se encontró DivisionChallenge en la escena!");
            }
        }
        
        // Suscribirse a eventos para debugging
        if (_divisionChallenge != null)
        {
            _divisionChallenge.OnChallengeCompleted += OnTestChallengeCompleted;
            _divisionChallenge.OnChallengeFailed += OnTestChallengeFailed;
        }
    }
    
    private void OnDestroy()
    {
        if (_divisionChallenge != null)
        {
            _divisionChallenge.OnChallengeCompleted -= OnTestChallengeCompleted;
            _divisionChallenge.OnChallengeFailed -= OnTestChallengeFailed;
        }
    }
    
    private void Update()
    {
        // Presionar tecla de test para iniciar el reto
        if (Input.GetKeyDown(_testKey))
        {
            StartTestChallenge();
        }
        
        // Presionar ESC para cancelar (útil para testing)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelChallenge();
        }
    }
    
    /// <summary>
    /// Inicia el reto en modo de prueba.
    /// </summary>
    private void StartTestChallenge()
    {
        if (_divisionChallenge == null)
        {
            Debug.LogError("No hay DivisionChallenge asignado!");
            return;
        }
        
        if (_enableDebugLogs)
        {
            Debug.Log($"[TEST] Iniciando reto de prueba... Presiona {_testKey} para iniciar, ESC para cancelar.");
        }
        
        _divisionChallenge.StartChallenge();
    }
    
    /// <summary>
    /// Cancela el reto actual (útil para testing).
    /// </summary>
    private void CancelChallenge()
    {
        // Aquí podrías agregar lógica para cancelar el reto si lo implementas
        if (_enableDebugLogs)
        {
            Debug.Log("[TEST] Reto cancelado (recargar escena para resetear).");
        }
    }
    
    /// <summary>
    /// Callback cuando el reto se completa.
    /// </summary>
    private void OnTestChallengeCompleted(int bonusMultiplier)
    {
        if (_enableDebugLogs)
        {
            Debug.Log($"[TEST] ¡Reto completado! Bonus: x{bonusMultiplier}");
            Debug.Log($"[TEST] Con este bonus, un horno de 10s cocinaría en {10f / bonusMultiplier:F1}s");
        }
    }
    
    /// <summary>
    /// Callback cuando el reto falla.
    /// </summary>
    private void OnTestChallengeFailed()
    {
        if (_enableDebugLogs)
        {
            Debug.Log("[TEST] Reto fallido.");
        }
    }
    
    // Método para dibujar instrucciones en pantalla
    private void OnGUI()
    {
        if (_enableDebugLogs)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            
            string instructions = $"[TESTING MODE]\nPresiona '{_testKey}' para iniciar el reto\nPresiona 'ESC' para salir";
            GUI.Label(new Rect(10, 10, 400, 100), instructions, style);
        }
    }
}