using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Spawn Aleatorio")]
    public bool useRandomSpawn = true;
    public Vector3 spawnCenter = Vector3.zero;
    public Vector3 spawnRange = new Vector3(10f, 0f, 10f);

    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Rotación con mouse")]
    public float mouseSensitivity = 100f;

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpPressed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;

        if (useRandomSpawn)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRange.x, spawnRange.x),
                Random.Range(-spawnRange.y, spawnRange.y),
                Random.Range(-spawnRange.z, spawnRange.z)
            );

            Vector3 spawnPos = spawnCenter + randomOffset;

            // Opcional: asegurarse de que esté sobre el suelo
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                spawnPos.y = hit.point.y + 0.5f; // justo sobre el suelo
            }

            controller.enabled = false; // importante antes de mover un CharacterController
            transform.position = spawnPos;
            controller.enabled = true;
        }
    }

    void Update()
    {
        // Detección de suelo
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movimiento
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // --- Animaciones ---
        // La animación se basa solo en la velocidad (caminar/correr)
        float speedPercent = move.magnitude * (isSprinting ? 1f : 0.5f);
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);

        // Pausar animación si está en el aire
        if (!isGrounded)
            animator.speed = 0f;
        else
            animator.speed = 1f;

        // --- Salto ---
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;

            // --- Detectar si hay un objeto bajo el jugador ---
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 1f))
            {
                var rotator = hit.collider.GetComponent<RotateOnPlayerJump>();
                if (rotator != null)
                {
                    rotator.RotateRandom();
                }
            }
        }

        // --- Gravedad ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Rotación del personaje según la cámara ---
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Reset de salto
        if (isGrounded && !wasGrounded)
            jumpPressed = false;
    }

    // --- Input System Callbacks ---
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Control de cámara separado
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
            jumpPressed = true;
    }
}