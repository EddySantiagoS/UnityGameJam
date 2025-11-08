using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera2 : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target;          // El jugador
    public Transform camTransform;    // La cámara (MainCamera)

    [Header("Ajustes")]
    public float distance = 2f;       // Distancia detrás del jugador
    public float height = 1f;       // Altura sobre el jugador
    public float sensitivity = 10f;
    public float minPitch = 80f;
    public float maxPitch = 150f;

    [Header("Input")]
    public InputActionReference lookAction; // arrastra aquí Gameplay/Look

    private Vector2 lookInput;
    private float yaw;
    private float pitch;

    void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
        if (lookAction != null) lookAction.action.performed += OnLook;
        if (lookAction != null) lookAction.action.canceled += OnLook;
    }

    void OnDisable()
    {
        if (lookAction != null)
        {
            lookAction.action.performed -= OnLook;
            lookAction.action.canceled -= OnLook;
            lookAction.action.Disable();
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (target == null) Debug.LogWarning("Asigna el jugador en el campo Target del ThirdPersonCamera2");
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += lookInput.x * sensitivity * Time.deltaTime;
        pitch += lookInput.y * sensitivity * Time.deltaTime; // tu preferencia de invertir o no
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * height);
    }

    // ahora es un callback para InputAction
    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
        // Debug.Log($"OnLook (ref) {lookInput}");
    }
}
