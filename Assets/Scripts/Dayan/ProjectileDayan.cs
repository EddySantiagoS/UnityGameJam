using UnityEngine;

public class ProjectileDayan : MonoBehaviour
{
    public int maxBounces = 3;
    public float maxLifeTime = 8f;
    private int bounces = 0;

    void Start()
    {
        // Se autodestruirá después de 'maxLifeTime' segundos
        Destroy(gameObject, maxLifeTime);
    }

    void OnCollisionEnter(Collision col)
    {
        // Comprueba si golpea al jugador
        if (col.gameObject.CompareTag("Player"))
        {
            // Llama al GameManager para reiniciar
            GameManagerDayan.Instance.RestartWorld();
            Destroy(gameObject); // Destruye el proyectil
            return; // Sale de la función
        }

        // Si no es el jugador, cuenta el rebote
        bounces++;
        if (bounces > maxBounces)
        {
            Destroy(gameObject);
        }
    }
}
