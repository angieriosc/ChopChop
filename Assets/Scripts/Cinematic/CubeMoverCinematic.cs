using UnityEngine;
using System.Collections.Generic;

public class CubeMoverCinematic : MonoBehaviour
{
[Header("Waypoints")]
    public List<Transform> waypoints;       // ← Lista de puntos a recorrer
    public float speed = 3f;
    public Transform cameraTransform;       // Para mirar a la cámara al final

    [Header("Callbacks")]
    public System.Action<int> OnReachedPoint;   // Se llama en cada punto
    public System.Action OnRouteFinished;       // Se llama cuando termina la ruta

    private Animator animator;
    private int speedHash;
    private int currentIndex = 0;
    private bool moving = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        speedHash = Animator.StringToHash("Speed");
    }

    void Start()
    {
        StartRoute();
    }

    /// <summary>
    /// Inicia el movimiento por los waypoints desde el primero.
    /// </summary>
    public void StartRoute()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("CubeMover: No hay waypoints asignados.");
            return;
        }

        currentIndex = 0;
        moving = true;

        if (animator)
        {
            animator.CrossFade("Running", 0.15f);
            animator.SetFloat(speedHash, 1f);
        }
    }

    void Update()
    {
        if (!moving) return;
        if (currentIndex >= waypoints.Count) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        Vector3 targetPos = target.position;

        // --- ROTACIÓN ---
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, look, 10f * Time.deltaTime);
        }

        // --- MOVIMIENTO ---
        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, speed * Time.deltaTime);

        // --- LLEGÓ AL PUNTO ---
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            OnReachedPoint?.Invoke(currentIndex);
            currentIndex++;

            // Si quedan más puntos: seguir corriendo
            if (currentIndex < waypoints.Count)
            {
                if (animator)
                    animator.SetFloat(speedHash, 1f);
                return;
            }

            // --- TERMINÓ LA RUTA ---
            moving = false;

            if (animator)
                animator.SetFloat(speedHash, 0);

            // Mirar a la cámara al final
            if (cameraTransform)
            {
                Vector3 lookDir = cameraTransform.position - transform.position;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            OnRouteFinished?.Invoke();
        }
    }
}
