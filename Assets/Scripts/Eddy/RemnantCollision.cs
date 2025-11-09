using UnityEngine;
using UnityEngine.SceneManagement;

public class RemnantCollision : MonoBehaviour
{
    [Tooltip("Nombre de la escena a cargar cuando el jugador toque este remanente")]
    public string sceneToLoad = "NextScene"; // cámbialo por el nombre real

    private void OnTriggerEnter(Collider other)
    {
        // Puedes ajustar la comparación según el tag o el componente del jugador
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}