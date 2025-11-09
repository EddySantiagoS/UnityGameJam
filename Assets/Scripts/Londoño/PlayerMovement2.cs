using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement2 : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 40f;
    public float sprintSpeed = 80f;
    public float jumpHeight = 2f;

    [Header("Gravedad Dinámica")]
    public float gravityNormal = -7f;   // gravedad normal
    public float gravitySprint = -9.8f; // gravedad cuando corre
    private float gravity;              // gravedad actual

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference sprintAction;
    public InputActionReference jumpAction;

    [Header("Animador")]
    public Animator animator;

    // ==============================================
    //     SISTEMA DE VIENTO AVANZADO
    // ==============================================
    [Header("Viento Avanzado")]
    public float baseWindStrength = 10f;        // viento constante suave
    public float turbulenceStrength = 15f;      // oscilación constante
    public float gustChance = 0.12f;            // probabilidad de ráfaga fuerte
    public float gustStrengthMin = 30f;         // fuerza mínima ráfaga
    public float gustStrengthMax = 60f;         // fuerza máxima ráfaga
    public float gustDurationMin = 0.25f;       // duración mínima
    public float gustDurationMax = 0.7f;        // duración máxima
    public float windChangeSpeed = 8f;          // suavizado del viento final

    private Vector3 windBase;
    private Vector3 windTurbulence;
    private Vector3 windGust;
    private Vector3 currentWind;

    private float windTimer;
    private float gustTimer;
    private float gustDuration;

    // =============================================
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpPressed;

    // ======================================================
    // ENABLE / DISABLE
    // ======================================================
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

    // ======================================================
    // START
    // ======================================================
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        gravity = gravityNormal;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // ======================================================
    // UPDATE
    // ======================================================
    void Update()
    {
        // =============================================
        // 1. DETECTAR SUELO
        // =============================================
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // =============================================
        // 2. VIENTO BASE  (dirección lenta y suave)
        // =============================================
        windTimer -= Time.deltaTime;

        if (windTimer <= 0f)
        {
            windBase = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized * baseWindStrength;

            windTimer = Random.Range(1f, 3f);
        }

        // =============================================
        // 3. TURBULENCIA (Perlin Noise)
        // =============================================
        windTurbulence = new Vector3(
            Mathf.PerlinNoise(Time.time * 1.5f, 0f) - 0.5f,
            0,
            Mathf.PerlinNoise(0f, Time.time * 1.5f) - 0.5f
        ).normalized * turbulenceStrength;

        // =============================================
        // 4. RÁFAGAS FUERTES
        // =============================================
        if (gustTimer <= 0f && Random.value < gustChance)
        {
            windGust = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(-1f, 1f)
            ).normalized * Random.Range(gustStrengthMin, gustStrengthMax);

            gustDuration = Random.Range(gustDurationMin, gustDurationMax);
            gustTimer = gustDuration;
        }
        else if (gustTimer > 0f)
        {
            gustTimer -= Time.deltaTime;
        }
        else
        {
            windGust = Vector3.zero;
        }

        // =============================================
        // 5. MEZCLA TOTAL DEL VIENTO
        // =============================================
        Vector3 totalWind = windBase + windTurbulence + windGust;

        currentWind = Vector3.Lerp(currentWind, totalWind, Time.deltaTime * windChangeSpeed);

        // =============================================
        // 6. MOVIMIENTO PLAYER + VIENTO
        // =============================================
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y)
                        * (isSprinting ? sprintSpeed : walkSpeed);

        Vector3 finalMovement = move + currentWind;

        controller.Move(finalMovement * Time.deltaTime);

        // =============================================
        // 7. GRAVEDAD DINÁMICA
        // =============================================
        gravity = isSprinting ? gravitySprint : gravityNormal;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // =============================================
        // 8. ROTAR HACIA DONDE MIRA CÁMARA
        // =============================================
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        if (isGrounded && !wasGrounded)
            jumpPressed = false;

        // =============================================
        // 9. ANIMACIONES
        // =============================================
        if (animator != null)
        {
            animator.SetBool("IsSprinting", isSprinting);
        }
    }

    // ======================================================
    // INPUT CALLBACKS
    // ======================================================
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

    // ======================================================
    // RESET GENERAL (se usa en la muerte)
    // ======================================================
    public void ResetMovementState()
    {
        velocity = Vector3.zero;
        currentWind = Vector3.zero;
        windBase = Vector3.zero;
        windTurbulence = Vector3.zero;
        windGust = Vector3.zero;

        gravity = gravityNormal;
    }
}
