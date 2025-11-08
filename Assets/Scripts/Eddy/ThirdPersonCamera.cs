using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;          // El jugador
    public Transform camTransform;    // La cámara (MainCamera)

    [Header("Ajustes")]
    public float distance = 3f;       // Distancia detrás del jugador
    public float height = 1.5f;       // Altura sobre el jugador
    public float sensitivity = 100f;
    public float minPitch = -20f;
    public float maxPitch = 45f;

    private Vector2 lookInput;
    private float yaw;   // Rotación horizontal
    private float pitch; // Rotación vertical

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target == null)
            Debug.LogWarning("Asigna el jugador en el campo Target del ThirdPersonCamera");
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Actualizar rotación de cámara
        yaw += lookInput.x * sensitivity * Time.deltaTime;
        pitch -= lookInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Calcular posición y rotación
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);

        // Aplicar al CameraRig
        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * height);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}