using UnityEngine;

/// <summary>
/// Controla el movimiento del jugador, incluyendo caminar y saltar.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private void Start() => controller = GetComponent<CharacterController>();

    private void Update()
    {
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    /// <summary>
    /// Gestiona el movimiento horizontal del jugador.
    /// </summary>
    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        controller.Move(move * speed * Time.deltaTime);
    }

    /// <summary>
    /// Gestiona el salto del jugador.
    /// </summary>
    private void HandleJump()
    {
        if (!Input.GetButtonDown("Jump") || !isGrounded) return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    /// <summary>
    /// Aplica gravedad al jugador.
    /// </summary>
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}