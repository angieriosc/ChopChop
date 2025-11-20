using UnityEngine;

public class CubeMover : MonoBehaviour
{
    public GameObject target;
    public float speed = 3f;

    public System.Action OnReachedTarget;

    private bool moving = false;
    private Animator animator;

    private int speedHash;

    void Awake()
    {
        animator = GetComponent<Animator>();
        speedHash = Animator.StringToHash("Speed");
    }

    public void MoveTo()
    {
        if (target == null)
        {
            Debug.LogWarning("CubeMover: No se asignó un target.");
            return;
        }

        moving = true;

        // Animación tipo PlayerMovement → Speed = 1
        if (animator)
            animator.SetFloat(speedHash, 1f);
    }

    void Update()
    {
        if (!moving || target == null) return;

        Vector3 targetPos = target.transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        // Si llegó al destino
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            moving = false;

            // Speed = 0 → Idle
            if (animator)
                animator.SetFloat(speedHash, 0f);

            OnReachedTarget?.Invoke();
        }
    }
}
