using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Clase estática de utilidad para encriptar y desencriptar texto utilizando el algoritmo AES.
/// Proporciona métodos seguros para manejar la confidencialidad de datos sensibles.
/// </summary>
public static class AESCryptography
{
    // TODO: Mover la clave y el IV a un backend seguro o derivarlos de una contraseña en lugar de almacenarlos en el código.
    private const string AES_KEY = "LLhcAOYPZ0ASa3CSaQLCATquUdRl3bHj"; // Debe tener 32 caracteres para AES-256
    private const string AES_IV = "EQMtM6u1djuJK3Qx";  // Debe tener 16 caracteres para AES

    /// <summary>
    /// Encripta una cadena de texto plano usando AES con el modo CBC y padding PKCS7.
    /// </summary>
    /// <param name="plainText">El texto plano que se va a encriptar.</param>
    /// <returns>Una cadena en formato Base64 que representa el texto encriptado.</returns>
    public static string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(AES_KEY);
            aes.IV = Encoding.UTF8.GetBytes(AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream())
            {
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    /// <summary>
    /// Desencripta una cadena de texto cifrado en Base64.
    /// </summary>
    /// <param name="cipherText">La cadena encriptada en formato Base64.</param>
    /// <returns>La cadena de texto plano original.</returns>
    public static string DecryptString(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(AES_KEY);
            aes.IV = Encoding.UTF8.GetBytes(AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
