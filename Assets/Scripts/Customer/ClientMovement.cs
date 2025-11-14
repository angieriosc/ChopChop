using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class ClientMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float rotationSpeed = 10f; // How quickly the player turns

    [Header("Animation")]
    [SerializeField] private Animator animator; // Assign in Inspector

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;

        // Move the player
        if (move.magnitude >= 0.1f)
        {
            // Rotate toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Move forward in facing direction
            controller.Move(move * speed * Time.deltaTime);
        }

        // Update animator Speed parameter
        float animationSpeed = move.magnitude;
        animator.SetFloat("Speed", animationSpeed);
    }

    private void HandleJump()
    {
        if (!Input.GetButtonDown("Jump") || !isGrounded)
            return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}