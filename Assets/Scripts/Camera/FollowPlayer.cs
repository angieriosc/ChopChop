using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla la cámara para que siga al jugador con un offset determinado.
/// </summary>
public class FollowPlayer : MonoBehaviour
{
    // 1. Variables públicas y serializadas
    [SerializeField] private GameObject player;

    // 2. Variables privadas
    private Vector3 _offset = Vector3.zero;

    // 3. Métodos de Unity
    /// <summary>
    /// Se llama antes del primer frame. Calcula el offset inicial entre cámara y jugador.
    /// </summary>
    private void Start()
    {
        if (player != null)
            _offset = transform.position - player.transform.position;
    }

    /// <summary>
    /// Se llama después de todos los métodos Update. Actualiza la posición y rotación de la cámara.
    /// </summary>
    private void LateUpdate()
    {
        if (player == null) return;

        transform.position = player.transform.position + player.transform.TransformDirection(_offset);
        transform.rotation = player.transform.rotation;
    }
}
