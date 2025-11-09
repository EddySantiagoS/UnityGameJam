using UnityEngine;

public class SafeZoneDayan : MonoBehaviour
{
    // Asegúrate de que este objeto tenga un Collider y que 'Is Trigger' esté marcado.
    // También, el Player debe tener un Rigidbody para que OnTriggerEnter funcione.

    [Tooltip("El nombre exacto de la escena a cargar")]
    public string nextSceneName;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            // ¡Nivel completado! Llama al reinicio (o futuro reshuffle)
            Debug.Log("¡Zona segura alcanzada!");
            GameManagerDayan.Instance.LoadNextScene(nextSceneName);
        }
    }
}
