using UnityEngine;

public class TimeManagerDayan : MonoBehaviour
{
    public static TimeManagerDayan Instance;

    public PlayerControllerDayan player;

    public float targetTimeScale = 1f;
    public float slowTimeScale = 0.1f;

    // --- MODIFICADO ---
    [Tooltip("El tiempo (en segundos) que tarda en completarse la transición. Un valor pequeño (ej: 0.2) es más rápido.")]
    public float transitionDuration = 0.2f;

    // Ya no necesitamos 'smoothSpeed'
    // --- ---

    // --- ¡NUEVA VARIABLE! ---
    // Variable interna que SmoothDamp necesita para funcionar
    private float currentTimeScaleVelocity = 0f;
    // --- ---

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        float desired = PlayerIsMoving() ? targetTimeScale : slowTimeScale;

        // --- ¡LÍNEA CLAVE MODIFICADA! ---
        // Antes usábamos: Mathf.Lerp(...)

        // Ahora usamos SmoothDamp para una transición suave y estable
        Time.timeScale = Mathf.SmoothDamp(
            Time.timeScale,             // Valor actual
            desired,                    // Valor objetivo
            ref currentTimeScaleVelocity, // Referencia a nuestra variable de velocidad
            transitionDuration,         // El tiempo que tardará en llegar
            Mathf.Infinity,             // (Velocidad máxima, no nos importa)
            Time.unscaledDeltaTime      // ¡Crítico! Usar tiempo NO escalado
        );
        // --- ---
        Time.timeScale = Mathf.Max(0f, Time.timeScale);
    }

    bool PlayerIsMoving()
    {
        return player != null && player.GetVelocityMagnitude() > 0.1f;
    }
}