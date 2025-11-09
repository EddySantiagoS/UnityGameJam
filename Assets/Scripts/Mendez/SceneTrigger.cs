using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra tiene la etiqueta "Player"
        if (other.CompareTag("Player"))
        {
            // Carga la escena indicada
            SceneManager.LoadScene(sceneName);
        }
    }
}
