using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

/// <summary>
/// Contiene los datos de autenticación (nombre de usuario y contraseña) en texto plano.
/// Esta clase se serializa a JSON antes de ser encriptada.
/// </summary>
[System.Serializable]
public class AuthPayload
{
    public string username;
    public string password;

    public AuthPayload(string u, string p)
    {
        username = u;
        password = p;
    }
}

/// <summary>
/// Contiene la carga final de datos encriptados que se enviará al backend.
/// </summary>
[System.Serializable]
public class EncryptedPayload
{
    /// <summary>
    /// La clave AES encriptada con la clave pública RSA, en formato Base64.
    /// </summary>
    public string key;
    /// <summary>
    /// El AuthPayload encriptado con AES, en formato Base64.
    /// </summary>
    public string data;
}

/// <summary>
/// Gestiona la lógica de registro e inicio de sesión, manejando la UI y la comunicación
/// segura con el backend mediante encriptación híbrida (AES + RSA).
/// </summary>
public class AuthManager : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _registerButton;
    [SerializeField] private TextMeshProUGUI _feedbackText;

    [Header("Backend Settings")]
    [SerializeField] private string _backendUrl = "http://localhost:4000/auth";

    [Header("Security Settings")]
    [TextArea(8, 20)]
    [SerializeField] private string _rsaPublicKeyXml; // Pegar aquí la clave pública XML del backend

    // 2. Variables privadas (No hay en este caso)

    // 3. Métodos de Unity
    private void OnEnable()
    {
        // Asegurarse de que la clave pública esté configurada
        if (string.IsNullOrEmpty(_rsaPublicKeyXml))
        {
            Debug.LogError("La clave pública RSA no está configurada en el componente AuthManager!");
            if (_feedbackText != null) _feedbackText.text = "Error de Configuración: Falta la clave pública RSA.";
            if (_loginButton != null) _loginButton.interactable = false;
            if (_registerButton != null) _registerButton.interactable = false;
            return;
        }

        // Añadir listeners cuando el objeto se active
        _loginButton.onClick.AddListener(OnLoginPressed);
        _registerButton.onClick.AddListener(OnRegisterPressed);
    }

    private void OnDisable()
    {
        // IMPORTANTE: Remover listeners cuando el objeto se desactive para prevenir duplicados
        _loginButton.onClick.RemoveListener(OnLoginPressed);
        _registerButton.onClick.RemoveListener(OnRegisterPressed);
    }

    // 4. Métodos de Photon (No aplica en esta clase)

    // 5. Métodos públicos
    /// <summary>
    /// Invocado al presionar el botón de Login. Inicia el proceso de envío de credenciales.
    /// </summary>
    public void OnLoginPressed()
    {
        string username = _usernameInput.text;
        string password = _passwordInput.text;
        StartCoroutine(SendEncryptedRequest("/login", username, password));
    }

    /// <summary>
    /// Invocado al presionar el botón de Registro. Inicia el proceso de registro de un nuevo usuario.
    /// </summary>
    public void OnRegisterPressed()
    {
        string username = _usernameInput.text;
        string password = _passwordInput.text;
        StartCoroutine(SendEncryptedRequest("/register", username, password));
    }

    // 6. Métodos privados auxiliares
    /// <summary>
    /// Corrutina que encripta y envía las credenciales del usuario al endpoint especificado.
    /// </summary>
    /// <param name="endpoint">El endpoint del API al que se enviará la petición (ej. "/login").</param>
    /// <param name="username">El nombre de usuario a enviar.</param>
    /// <param name="password">La contraseña a enviar.</param>
    private IEnumerator SendEncryptedRequest(string endpoint, string username, string password)
    {
        // 1. Generar una clave AES de un solo uso para esta petición
        using Aes aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateKey();
        aes.GenerateIV();
        byte[] aesKey = aes.Key;
        byte[] iv = aes.IV;

        // 2. Encriptar el payload (usuario/contraseña) con AES
        AuthPayload payload = new AuthPayload(username, password);
        string jsonPayload = JsonUtility.ToJson(payload);
        byte[] plainBytes = Encoding.UTF8.GetBytes(jsonPayload);
        
        byte[] cipherBytes;
        using (ICryptoTransform encryptor = aes.CreateEncryptor(aesKey, iv))
        {
            cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        byte[] combinedIvAndCipher = new byte[iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, combinedIvAndCipher, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, combinedIvAndCipher, iv.Length, cipherBytes.Length);
        
        string encryptedData = Convert.ToBase64String(combinedIvAndCipher);

        // 3. Encriptar la clave AES con la clave pública RSA
        string encryptedAesKey;
        try
        {
            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(_rsaPublicKeyXml);
            // 'true' especifica padding OAEP (con SHA-1), compatible con implementaciones más antiguas
            byte[] encryptedKeyBytes = rsa.Encrypt(aesKey, true);
            encryptedAesKey = Convert.ToBase64String(encryptedKeyBytes);
        }
        catch (Exception e)
        {
            Debug.LogError("Error en la encriptación RSA: " + e.Message);
            if (_feedbackText != null) _feedbackText.text = "Error: Falló la encriptación RSA. Verifique el formato de la clave.";
            yield break;
        }

        // 4. Crear el payload final y enviarlo
        EncryptedPayload finalPayload = new EncryptedPayload { key = encryptedAesKey, data = encryptedData };
        string finalJson = JsonUtility.ToJson(finalPayload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(finalJson);

        using (UnityWebRequest request = new UnityWebRequest(_backendUrl + endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Enviando petición encriptada a: " + request.url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Respuesta: " + request.downloadHandler.text);
                if (_feedbackText != null)
                    _feedbackText.text = "Éxito! " + request.downloadHandler.text;
            }
            else
            {
                // Este bloque captura errores de conexión o del backend (ej. fallo en desencriptación)
                string errorMsg = $"Error: {request.error} | Código: {request.responseCode} | Cuerpo: {request.downloadHandler.text}";
                Debug.LogError(errorMsg);
                if (_feedbackText != null)
                    _feedbackText.text = errorMsg;
            }
        }
    }
}
