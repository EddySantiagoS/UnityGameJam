using UnityEngine;

public class ProjectileDayan : MonoBehaviour
{
    public int maxBounces = 3;
    public float maxLifeTime = 8f;
    private int bounces = 0;

    public AudioClip bounceSound;
    private AudioSource audioSource;

    // Asigna la capa del suelo/paredes
    public LayerMask collisionLayers;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            // El sonido de rebote DEBE ralentizarse con el tiempo
            audioSource.ignoreListenerPause = false;
        }
    }

    void Start()
    {
        // Se autodestruirá después de 'maxLifeTime' segundos
        Destroy(gameObject, maxLifeTime);
    }

    void OnCollisionEnter(Collision col)
    {
        // 1. COMPROBAR COLISIÓN CON EL JUGADOR
        if (col.gameObject.CompareTag("Player"))
        {
            // Llama al GameManager para reiniciar (Asumiendo que existe y funciona)
            // GameManagerDayan.Instance.RestartWorld(); 
            Destroy(gameObject); // Destruye el proyectil
            return;
        }

        // 2. COMPROBAR SI LA COLISIÓN ES CON UN MURO/SUELO (Usando la LayerMask)
        // La sintaxis se corrige a 'col.gameObject.layer'
        bool isBouncySurface = ((1 << col.gameObject.layer) & collisionLayers) != 0;

        if (isBouncySurface)
        {
            // Reproducir el sonido
            if (audioSource != null && bounceSound != null)
            {
                // La sintaxis se corrige a 'col.relativeVelocity.magnitude'
                audioSource.PlayOneShot(bounceSound, col.relativeVelocity.magnitude * 0.1f);
            }

            // 3. CONTAR REBOTES Y AUTODESTRUCCIÓN
            bounces++;
            if (bounces > maxBounces)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Si choca con algo que NO es una superficie de rebote (ej: otro enemigo)
            // Puedes decidir destruirlo inmediatamente o no hacer nada.
            // Para mantener la lógica de "maxBounces", debe haber una condición de rebote.

            // Si la superficie no es de rebote, la destruimos (opcional, basado en diseño)
            // Destruye el proyectil si no es una superficie "bouncy" para evitar infinitos rebotes no deseados.
            Destroy(gameObject);
        }
    }
}