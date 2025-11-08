using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerDayan : MonoBehaviour
{
    public static GameManagerDayan Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RestartWorld()
    {
        // Evita que se llame múltiples veces si varios proyectiles golpean a la vez
        if (Time.timeScale == 0f) return;

        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        Debug.Log("¡Mundo reiniciándose!");

        // Ponemos el tiempo a 0 inmediatamente para detener todo
        Time.timeScale = 0f;

        // Espera en tiempo real, ya que el timeScale es 0
        yield return new WaitForSecondsRealtime(0.5f);

        // Recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
