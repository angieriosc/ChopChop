using UnityEngine;

/// <summary>
/// Representa una tarea de corte en el juego, indicando el ingrediente a cortar
/// y la cantidad de cortes necesarios para completarla.
/// </summary>
[System.Serializable]
public class SliceTask
{
    /// <summary>
    /// Nombre de la tarea o del ingrediente.
    /// </summary>
    public string taskName;

    /// <summary>
    /// Prefab del ingrediente que será cortado.
    /// </summary>
    public GameObject ingredientPrefab;

    /// <summary>
    /// Número de cortes requeridos para completar la tarea.
    /// </summary>
    public int requiredSlices;

    /// <summary>
    /// Textura representativa de la tarea o ingrediente.
    /// </summary>
    public Texture taskTexture;
}
