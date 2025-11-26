using UnityEngine;

public static class InputCooldown
{
    public static bool BlockNextE = false;

    private static float cooldownTimer = 0f;

    /// <summary>
    /// Activa un bloqueo por tiempo (ej. 0.5 segundos).
    /// </summary>
    public static void TriggerCooldown(float duration)
    {
        cooldownTimer = Time.time + duration;
    }

    /// <summary>
    /// Revisa si todavía estamos en tiempo de espera.
    /// </summary>
    public static bool IsOnCooldown()
    {
        return Time.time < cooldownTimer;
    }
}