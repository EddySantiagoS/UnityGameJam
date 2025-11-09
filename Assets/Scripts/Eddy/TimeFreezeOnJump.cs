using UnityEngine;
using UnityEngine.InputSystem;

public class TimeFreezeOnJump : MonoBehaviour
{
    [Header("Configuración de congelamiento")]
    [Tooltip("Escala de tiempo cuando se congela (0 detiene todo)")]
    public float frozenTimeScale = 0f;
    [Tooltip("Velocidad de transición al congelar/descongelar")]
    public float transitionSpeed = 10f;

    private PlayerMovement playerMovement;
    private bool freezeHeld = false;
    private float targetTimeScale = 1f;
    private Animator animator;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerMovement == null) return;

        bool groundedNow = Physics.CheckSphere(
            playerMovement.groundCheck.position,
            playerMovement.groundDistance,
            playerMovement.groundMask
        );

        // Solo permitir congelar si está en el aire
        if (!groundedNow && freezeHeld)
            targetTimeScale = frozenTimeScale;
        else
            targetTimeScale = 1f;

        // Transición suave del tiempo
        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * transitionSpeed);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Detener animaciones al congelar
        if (animator != null)
            animator.speed = (Time.timeScale < 0.01f) ? 0f : 1f;
    }

    // Este método único se conecta al evento "Freeze" del Player Input
    public void OnFreeze(InputAction.CallbackContext context)
    {
        // Detectar si el botón está presionado o soltado
        if (context.started)
            freezeHeld = true;
        else if (context.canceled)
            freezeHeld = false;
    }
}