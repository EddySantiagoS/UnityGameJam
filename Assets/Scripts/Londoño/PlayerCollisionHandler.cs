using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerMovement2 movementScript;

    private bool isDead = false;
    private bool hasWon = false;

    [Header("Respawn")]
    public Transform respawnPoint;

    [Header("Escena de victoria")]
    public string winSceneName = "NombreDeLaEscena";

    [Header("Audio del Player")]
    public AudioClip fallLoopSound;   // sonido en loop
    public AudioClip deathSound;      // sonido completo de muerte

    private AudioSource fallAudioSource;  // solo para el sonido en loop
    private CharacterController controller;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement2>();
        controller = GetComponent<CharacterController>();

        // Audio source para loop de caída
        fallAudioSource = gameObject.AddComponent<AudioSource>();
        fallAudioSource.loop = true;
        fallAudioSource.volume = 0.75f;

        if (fallLoopSound != null)
        {
            fallAudioSource.clip = fallLoopSound;
            fallAudioSource.Play();
        }

        if (respawnPoint == null)
        {
            GameObject rp = new GameObject("RespawnPoint");
            rp.transform.position = transform.position;
            respawnPoint = rp.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Muerte") && !isDead)
        {
            isDead = true;

            if (movementScript != null)
                movementScript.enabled = false;

            movementScript.ResetMovementState();

            PlayDeathSoundFull();  // ✅ reproducir sonido de muerte completo

            StartCoroutine(RespawnPlayer());
            return;
        }

        if (other.CompareTag("Meta") && !hasWon)
        {
            hasWon = true;

            if (movementScript != null)
                movementScript.enabled = false;

            SceneManager.LoadScene(winSceneName);
        }
    }

    private System.Collections.IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(0.2f);

        controller.enabled = false;
        transform.position = respawnPoint.position;
        controller.enabled = true;

        if (movementScript != null)
            movementScript.enabled = true;

        // Volver a activar loop de caída
        RestartFallLoop();

        isDead = false;
    }

    public void ForceDeath()
    {
        if (!isDead)
        {
            isDead = true;

            if (movementScript != null)
                movementScript.enabled = false;

            movementScript.ResetMovementState();

            PlayDeathSoundFull();

            StartCoroutine(RespawnPlayer());
        }
    }

    // =====================================================
    //  AUDIO MEJORADO
    // =====================================================

    // ✅ Reproduce el sonido de muerte sin interrupciones
    void PlayDeathSoundFull()
    {
        if (deathSound == null) return;

        // Detener el loop del personaje
        fallAudioSource.Stop();

        // Crear un AudioSource temporal SOLO PARA ESTE SONIDO
        GameObject tempAudio = new GameObject("DeathSound");
        AudioSource source = tempAudio.AddComponent<AudioSource>();

        source.clip = deathSound;
        source.volume = 1f;
        source.loop = false;

        source.Play();

        // Destruir audio cuando termine
        Destroy(tempAudio, deathSound.length);
    }

    void RestartFallLoop()
    {
        if (fallLoopSound != null)
        {
            fallAudioSource.Stop();
            fallAudioSource.clip = fallLoopSound;
            fallAudioSource.loop = true;
            fallAudioSource.Play();
        }
    }
}
