using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Cargar la escena del nivel 1
    public void PlayGame()
    {
        SceneManager.LoadScene("Eddy"); // Cambia "Nivel1" por el nombre real de tu escena
    }

    // Salir del juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        // Si estás en el editor, esto permite probar la salida
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}