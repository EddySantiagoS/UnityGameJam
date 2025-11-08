using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerMovement2 movementScript;   // para desactivar controles
    private bool isDead = false;
    private bool hasWon = false;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement2>();

        if (movementScript == null)
            Debug.LogWarning("No se encontró PlayerMovement2 en el jugador.");
    }

    // ESTE MÉTODO FUNCIONA PARA CharacterController (usa Triggers)
    private void OnTriggerEnter(Collider other)
    {
        // ======= MUERTE =======
        if (other.CompareTag("Muerte") && !isDead)
        {
            isDead = true;
            Debug.Log("Has muerto uwu");

            // Apagar controles
            if (movementScript != null)
                movementScript.enabled = false;

            // Puedes destruir el player:
            Destroy(gameObject, 0.2f);

            // O puedes desactivarlo:
            // gameObject.SetActive(false);

            // Llamar un futuro menú de muerte...
            // UIManager.Instance.ShowDeathScreen();
        }

        // ======= META / GANAR =======
        if (other.CompareTag("Meta") && !hasWon)
        {
            hasWon = true;
            Debug.Log("¡Has ganado! Inicia cinemática");

            // Desactivar movimiento
            if (movementScript != null)
                movementScript.enabled = false;

            // También puedes bloquear la cámara
            // Camera.main.GetComponent<ThirdPersonCamera2>().enabled = false;

            // Iniciar una cinemática (timeline)
            // FindObjectOfType<GameManager>().StartCinematic();

            // O cargar escena, mostrar UI, etc.
        }
    }
}
