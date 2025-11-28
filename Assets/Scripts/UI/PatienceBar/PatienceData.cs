using UnityEngine;

[CreateAssetMenu(fileName = "PatienceConfig", menuName = "Game/PatienceConfig")]
public class PatienceData : ScriptableObject
{
    [Header("Configuración Global")]
    public float maxPatience = 100f;
    
    [Tooltip("Puntos que baja por segundo automáticamente")]
    public float passiveDecayRate = 0.5f;

    [Header("Tabla de Penalizaciones")]
    public float burntPizzaPenalty = 15f;
    public float undercookedPenalty = 20f;
    public float wrongCutPenalty = 10f;
    public float wrongPourPenalty = 5f;
    public float genericPenalty = 5f;
    public float incompletePenalty = 5f;

    /// <summary>
    /// Traduce el tipo de error a un valor numérico
    /// </summary>
    public float GetDamageAmount(PenaltyType type)
    {
        switch (type)
        {
            case PenaltyType.BurntPizza: return burntPizzaPenalty;
            case PenaltyType.WrongCut: return wrongCutPenalty;
            case PenaltyType.WrongPour: return wrongPourPenalty;
            case PenaltyType.Undercooked: return undercookedPenalty;
            case PenaltyType.Incomplete: return genericPenalty;
            default: return genericPenalty;
        }
    }
}