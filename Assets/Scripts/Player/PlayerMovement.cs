using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;           // Velocidad de movimiento
    [SerializeField] private float rotationSpeed = 120f; // Velocidad de rotación (grados/segundo)
    [SerializeField] private float gravity = -9.81f;     // Gravedad
    [SerializeField] private float jumpHeight = 1.5f;    // Altura de salto

    [Header("Animation")]
    [SerializeField] private Animator animator;          // Asignar en Inspector
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string groundedParam = "Grounded";
    [SerializeField] private string jumpTrigger = "Jump";

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private int speedHash;
    private int groundedHash;
    private int jumpHash;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        speedHash = Animator.StringToHash(speedParam);
        groundedHash = Animator.StringToHash(groundedParam);
        jumpHash = Animator.StringToHash(jumpTrigger);
    }

    private void Update()
    {
        if (DialogueLock.IsLocked)
            return;

        if (ToppingLock.IsLocked)
            return;
        if (DeliveryLock.IsLocked)
            return;
            
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        // Comprobamos si está en el suelo
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movimiento hacia adelante y atrás (W/S)
        float moveZ = Input.GetAxis("Vertical");  // W/S
        Vector3 move = transform.forward * moveZ;

        controller.Move(move * speed * Time.deltaTime);

        // Rotación con A/D
        float rotate = Input.GetAxis("Horizontal"); // A/D
        transform.Rotate(Vector3.up * rotate * rotationSpeed * Time.deltaTime);

        // Animaciones
        if (animator)
        {
            animator.SetFloat(speedHash, Mathf.Abs(moveZ), 0.1f, Time.deltaTime);
            animator.SetBool(groundedHash, isGrounded);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator && !string.IsNullOrEmpty(jumpTrigger))
                animator.SetTrigger(jumpHash);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}