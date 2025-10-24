using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UISpriteAnimation : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Plays the animation by setting animator speed to 1.
    /// </summary>
    public void PlayAnimation()
    {
        if (animator != null)
            animator.speed = 1f;
    }

    /// <summary>
    /// Pauses the animation by setting animator speed to 0.
    /// </summary>
    public void PauseAnimation()
    {
        if (animator != null)
            animator.speed = 0f;
    }

    /// <summary>
    /// Resets animation to start frame and pauses it.
    /// </summary>
    public void ResetAndPause()
    {
        if (animator != null)
        {
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
            animator.speed = 0f;
        }
    }
}
