using UnityEngine;

public class VisualsFollowerDayan : MonoBehaviour
{
    // Arrastra el objeto Player (la esfera con el Rigidbody) aquí
    public Transform playerTarget;

    // Arrastra el script PlayerControllerDayan aquí para obtener la dirección de giro.
    public PlayerControllerDayan playerController;

    public float rotationSpeed = 15f;

    void LateUpdate()
    {
        if (playerTarget == null || playerController == null) return;

        // 1. Seguimiento de Posición
        transform.position = playerTarget.position;

        // 2. Seguimiento de Rotación (Solo Y)
        Vector3 targetDirection = playerController.GetLastDashDirection();

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Aplicar la rotación suavemente (solo el eje Y está siendo afectado por LookRotation)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.unscaledDeltaTime
            );
        }
    }
}