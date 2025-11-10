using UnityEngine;
using UnityEngine.InputSystem;

public class TimeFreezeOnJump : MonoBehaviour
{
    [Header("Configuración de congelamiento")]
    [Tooltip("Escala de tiempo cuando se congela (0 detiene todo)")]
    public float frozenTimeScale = 0f;
    [Tooltip("Velocidad de transición al congelar/descongelar")]
    public float transitionSpeed = 10f;

    [Header("Audio")]
    [Tooltip("Velocidad de transición del volumen (igual o menor a transitionSpeed para más suavidad)")]
    public float audioFadeSpeed = 5f;

    private PlayerMovement playerMovement;
    private bool freezeHeld = false;
    private float targetTimeScale = 1f;
    private Animator animator;

    private AudioSource[] allAudioSources;
    private bool isFrozen = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        // Buscar todos los audios activos (puede ser música y efectos)
        allAudioSources = FindObjectsOfType<AudioSource>();
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

        // Animación
        if (animator != null)
            animator.speed = (Time.timeScale < 0.01f) ? 0f : 1f;

        // Transición de volumen sincronizada
        float targetVolume = Mathf.InverseLerp(0f, 1f, Time.timeScale); // 0 cuando está congelado, 1 cuando está normal
        foreach (var audio in allAudioSources)
        {
            if (audio != null)
            {
                audio.volume = Mathf.Lerp(audio.volume, targetVolume, Time.unscaledDeltaTime * audioFadeSpeed);
            }
        }

        // Pausa total solo cuando el tiempo está casi detenido
        if (Time.timeScale < 0.01f && !isFrozen)
        {
            foreach (var audio in allAudioSources)
                if (audio != null) audio.Pause();
            isFrozen = true;
        }
        else if (Time.timeScale >= 0.01f && isFrozen)
        {
            foreach (var audio in allAudioSources)
                if (audio != null) audio.UnPause();
            isFrozen = false;
        }
    }

    public void OnFreeze(InputAction.CallbackContext context)
    {
        if (context.started)
            freezeHeld = true;
        else if (context.canceled)
            freezeHeld = false;
    }
}