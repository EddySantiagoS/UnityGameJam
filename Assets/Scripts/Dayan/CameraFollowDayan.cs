using UnityEngine;

public class CameraFollowDayan : MonoBehaviour
{
    [Tooltip("El objetivo que la cámara debe seguir (El jugador)")]
    public Transform target;

    // --- ELIMINADAS ---
    // Ya no necesitamos 'smoothSpeed' ni 'velocity'
    // --- ---

    [Tooltip("La distancia y ángulo desde el jugador (Ej: (0, 20, 0) para vista cenital)")]
    public Vector3 offset;

    [Header("Límites del Mapa")]
    public bool clampCamera = true;
    public Vector2 minBounds; // Esquina inferior-izquierda
    public Vector2 maxBounds; // Esquina superior-derecha

    // Usamos LateUpdate para un seguimiento "duro" y sin vibraciones.
    // Se ejecuta después de que el jugador se ha movido en Update/FixedUpdate.
    void LateUpdate()
    {
        if (target == null) return;

        // 1. Posición deseada (la del jugador + el offset)
        Vector3 desiredPosition = target.position + offset;

        // 2. Aplicar los límites si están activados
        if (clampCamera)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            // OJO: En vista cenital, el "Y" del mapa es el "Z" del mundo
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        // 3. Asignar la posición directamente (sin suavizado)
        transform.position = desiredPosition;
    }

    // (Opcional) Dibuja los límites en el editor para que puedas verlos
    // Esta función no cambia
    void OnDrawGizmosSelected()
    {
        if (clampCamera)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, transform.position.y - offset.y, (minBounds.y + maxBounds.y) / 2);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, 1, maxBounds.y - minBounds.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
}