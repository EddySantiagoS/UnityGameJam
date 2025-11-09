using UnityEngine;

public class EnemyDamageTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemigo tocó al jugador → muerte");

            // Llamamos al respawn usando el PlayerCollisionHandler
            PlayerCollisionHandler handler = other.GetComponent<PlayerCollisionHandler>();

            if (handler != null)
            {
                handler.ForceDeath();
            }
        }
    }
}
