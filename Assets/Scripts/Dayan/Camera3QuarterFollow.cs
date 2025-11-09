using UnityEngine;

public class Camera3QuarterFollow : MonoBehaviour
{
    public Transform target; // Arrastra tu personaje aquí en el Inspector
    public Vector3 offset = new Vector3(8f, 10f, -8f); // Ajusta estos valores para el ángulo 3/4
    public float smoothSpeed = 2f; // Controla la suavidad del seguimiento

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcular la posición deseada de la cámara
        Vector3 desiredPosition = target.position + offset;

        // 2. Interpolar suavemente (Lerp) hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 3. Asegurarse de que la cámara esté mirando al personaje
        transform.LookAt(target);
    }
}