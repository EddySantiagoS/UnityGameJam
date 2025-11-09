using UnityEngine;

public class StartGameTrigger : MonoBehaviour
{
    public BoardGenerator boardGenerator; // ← arrastra el objeto del tablero
    public GameObject piso;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Desactivamos el trigger para que no vuelva a activarse
            gameObject.SetActive(false);

            piso.SetActive(false);

            // Iniciamos el juego
            boardGenerator.StartGame();
        }
    }
}
