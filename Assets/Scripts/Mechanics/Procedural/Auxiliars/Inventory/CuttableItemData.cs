using UnityEngine;

/// <summary>
/// Un script "ayudante" que se coloca en los objetos "cortables" (como el bowl)
/// para decirle a la estación cuál es el prefab de la rebanada resultante.
/// </summary>
public class CuttableItemData : MonoBehaviour
{
    [Tooltip("El prefab que debe guardarse en el inventario (ej. 'DoughSlice').")]
    public GameObject sliceResultPrefab;
}