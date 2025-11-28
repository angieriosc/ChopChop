using UnityEngine;

[System.Serializable]
public struct GamePenalty
{
    public string nombreDelError;
    [Range(0, 100)]
    public float cantidadDeDaño;
}
