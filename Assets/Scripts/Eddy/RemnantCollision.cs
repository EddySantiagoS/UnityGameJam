using UnityEngine;
using UnityEngine.SceneManagement;

public class RemnantCollision : MonoBehaviour
{
    [Tooltip("Escena a la que se cargará al tocar un remanente")]
    public string sceneToLoad = "Santiago"; // escena destino

    private void OnTriggerEnter(Collider other)
    {
        // asegúrate de que tu jugador tenga el Tag "Player"
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}