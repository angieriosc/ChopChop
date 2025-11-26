using UnityEngine;

/// <summary>
/// Representa una línea de diálogo cinemático.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    public string actorName;
    public string text;
    public float autoDuration = 3f;

    public Transform focusTarget;

    public Animator actorAnimator;
    public string talkingBoolName = "IsTalking";
}
