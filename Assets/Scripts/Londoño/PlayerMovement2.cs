using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement2 : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 40f;
    public float sprintSpeed = 80f;
    public float jumpHeight = 2f;
    public float gravity = -14f;

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference sprintAction;
    public InputActionReference jumpAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool jumpPressed;

    void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        jumpAction.action.Enable();

        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;

        sprintAction.action.performed += OnSprint;
        sprintAction.action.canceled += OnSprint;

        jumpAction.action.started += OnJump;
    }

    void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;

        sprintAction.action.performed -= OnSprint;
        sprintAction.action.canceled -= OnSprint;

        jumpAction.action.started -= OnJump;

        moveAction.action.Disable();
        sprintAction.action.Disable();
        jumpAction.action.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movimiento
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Salto
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Rotación hacia donde mira la cámara
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        if (isGrounded && !wasGrounded)
            jumpPressed = false;
    }

    // ==================
    // INPUT CALLBACKS
    // ==================

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = ctx.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started && isGrounded)
            jumpPressed = true;
    }
}
