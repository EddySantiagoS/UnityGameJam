using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    }

    // --- NUEVO: Iniciar el primer nivel al arrancar ---
    void Start()
    {
        if (levelGenerator == null)
        {
            levelGenerator = LevelGeneratorDayan.Instance;
        }
        // Genera el nivel inicial
        StartCoroutine(levelGenerator.GenerateNewLevel());
    }

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

        yield return StartCoroutine(levelGenerator.GenerateNewLevel());

        // ¡LA LÍNEA CLAVE! Llama al generador
        if (levelGenerator != null)
        {
            levelGenerator.GenerateNewLevel();
        }

        // El tiempo se reiniciará automáticamente cuando el jugador se mueva
        // (controlado por TimeManagerDayan)
    }

    public void LoadNextScene(string sceneName)
    {
        // 1. Reseteamos el tiempo a la normalidad
        // (¡Crítico! si no, la nueva escena cargará en cámara lenta)
        Time.timeScale = 1f;

        // 2. Cargamos la nueva escena por su nombre
        SceneManager.LoadScene(sceneName);
    }
}