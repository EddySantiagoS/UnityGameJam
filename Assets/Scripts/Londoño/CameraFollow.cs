using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target a seguir")]
    public Transform player;

    [Header("Offsets")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);

    [Header("Suavizado")]
    public float smoothSpeed = 0.15f;

    private void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("No se asignó el player en la cámara.");
            return;
        }

        // Posición objetivo
        Vector3 desiredPosition = player.position + offset;

        // Suavizado
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        // Siempre mirar al player
        transform.LookAt(player);
    }
}
