using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator;
    public float animationSpeed = 1.0f; // Default speed

    void Start()
    {
        // Get the Animator component if not assigned in Inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        
        animationSpeed += 0.5f;
        animator.SetFloat("SpeedMultiplier", animationSpeed);
    }
}