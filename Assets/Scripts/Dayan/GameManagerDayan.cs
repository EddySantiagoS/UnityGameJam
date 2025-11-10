using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManagerDayan : MonoBehaviour
{
    public static GameManagerDayan Instance;

    [Tooltip("Referencia al generador de niveles")]
    public LevelGeneratorDayan levelGenerator; // Arrastra el objeto LevelGenerator aquí

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        UnlockAndShowCursor();
    }

    // --- NUEVO: Iniciar el primer nivel al arrancar ---
    void Start()
    {
        if (levelGenerator == null)
        {
            // Asumiendo que LevelGenerator también usa un patrón Singleton
            levelGenerator = LevelGeneratorDayan.Instance;
        }

        // Genera el nivel inicial y bloquea el cursor.
        StartCoroutine(InitialSetupRoutine());
    }

    // --- NUEVA RUTINA DE CONFIGURACIÓN INICIAL ---
    IEnumerator InitialSetupRoutine()
    {
        // 1. Generar el nivel (asumiendo que esto es una corrutina en LevelGeneratorDayan)
        yield return StartCoroutine(levelGenerator.GenerateNewLevel());
    }
    // ---------------------------------------------

    public void RestartWorld()
    {
        // Evita que se llame múltiples veces
        if (Time.timeScale == 0f) return;

        StartCoroutine(RestartRoutine());
    }

    // --- MODIFICADO: Esta rutina ahora llama al generador ---
    IEnumerator RestartRoutine()
    {
        Debug.Log("¡Mundo reiniciándose proceduralmente!");
        Time.timeScale = 0f; // Detiene todo

        // Efecto visual (ej. un fade a negro) iría aquí
        yield return new WaitForSecondsRealtime(0.5f);

        // Llama al generador (asumiendo que GenerateNewLevel es una Corrutina)
        yield return StartCoroutine(levelGenerator.GenerateNewLevel());

        // Bloquea el cursor nuevamente para el gameplay después de la generación.
        UnlockAndShowCursor();

        // El tiempo se reiniciará automáticamente cuando el jugador se mueva.
    }

    public void LoadNextScene(string sceneName)
    {

        // 2. Reseteamos el tiempo a la normalidad
        Time.timeScale = 1f;

        // 3. Cargamos la nueva escena por su nombre
        SceneManager.LoadScene(sceneName);
    }

    // --- MÉTODOS DE CONTROL DE CURSOR ---

    public void UnlockAndShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor Desbloqueado y Visible.");
    }
    // ------------------------------------
}