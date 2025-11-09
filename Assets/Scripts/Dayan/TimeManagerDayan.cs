using System.Collections;
using UnityEngine;

// Ya no necesita lógica en Update
public class TimeManagerDayan : MonoBehaviour
{
    public static TimeManagerDayan Instance;

    [Tooltip("La duración de la transición para un cambio suave.")]
    public float transitionDuration = 0.2f;

    private float currentTimeScaleVelocity = 0f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Método que el PlayerController llamará
    public void SetTimeScaleSmooth(float targetScale)
    {
        // Cancelamos cualquier operación Invoke o corrutina anterior para un control estricto
        if (Time.timeScale == targetScale) return;

        // Si ya hay un cambio de tiempo en curso, simplemente ajustamos el objetivo
        StopAllCoroutines();
        StartCoroutine(TransitionTimeScale(targetScale));
    }

    // Corrutina para una transición suave (opcional, pero mejora la sensación)
    private IEnumerator TransitionTimeScale(float targetScale)
    {
        float startScale = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;

            // Suaviza la escala de tiempo
            Time.timeScale = Mathf.Lerp(startScale, targetScale, t);

            // Asegura que no sea negativo
            Time.timeScale = Mathf.Max(0f, Time.timeScale);

            yield return null;
        }

        Time.timeScale = targetScale;
    }
}