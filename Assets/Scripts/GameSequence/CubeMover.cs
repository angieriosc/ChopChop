using UnityEngine;

public class CubeMover : MonoBehaviour
{
    public GameObject target;
    public float speed = 3f;
    public Transform cameraTransform;   // ← asigna la MainCamera aquí

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

        // Forzar la animación correcta de correr
        if (animator)
        {
            animator.CrossFade("Running", 0.15f);
            animator.SetFloat(speedHash, 1f);
        }
    }


    void Update()
    {
        if (!moving || target == null) return;

        Vector3 targetPos = target.transform.position;

        // --- ROTACIÓN HACIA EL DESTINO ---
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0; // evitar inclinación

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                10f * Time.deltaTime
            );
        }

        // --- MOVIMIENTO ---
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        // Llegó al destino
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            moving = false;

            // Idle
            if (animator)
                animator.SetFloat(speedHash, 0f);

            // --- GIRAR HACIA LA CÁMARA ---
            if (cameraTransform != null)
            {
                Vector3 lookDir = (cameraTransform.position - transform.position);
                lookDir.y = 0;

                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            OnReachedTarget?.Invoke();
        }
    }
}
