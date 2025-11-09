using UnityEngine;

public class CameraFollowDayan : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    [Header("Límites del Mapa")]
    public bool clampCamera = false; // Desactivada por defecto
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Compensación de Aspecto")]
    public float baseAspectRatio = 1.777778f; // 16/9

    private Vector3 originalOffset;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        originalOffset = offset;

        // Forzamos la primera posición para asegurar que la cámara
        // esté centrada en el jugador antes de que empiece el juego.
        SetCameraPosition();
    }

    void Update()
    {
        AdjustCameraForAspectRatio();
    }

    void AdjustCameraForAspectRatio()
    {
        if (mainCamera == null) return;

        float currentAspect = mainCamera.aspect;
        float compensationFactor = baseAspectRatio / currentAspect;
        Vector3 newOffset = originalOffset;

        // Compensamos la altura (Y) y profundidad (Z)
        newOffset.y = originalOffset.y * compensationFactor;
        newOffset.z = originalOffset.z * compensationFactor;

        offset = newOffset;
    }

    void LateUpdate()
    {
        SetCameraPosition();
    }

    // Función central para calcular y asignar la posición
    void SetCameraPosition()
    {
        if (target == null) return;

        // --- FÓRMULA CLAVE: Jugador + Offset ROTADO ---
        Vector3 desiredPosition = target.position + transform.rotation * offset;
        // ----------------------------------------------

        if (clampCamera)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
        }

        transform.position = desiredPosition;
    }

    // ----------------------------------------------------
    // --- FUNCIÓN TEMPORAL PARA CALCULAR EL OFFSET IDEAL ---
    // ----------------------------------------------------
    public void CalculateIdealOffset()
    {
        // Estos valores vienen de tu sesión de depuración:
        Vector3 playerPos = new Vector3(5.22f, 1.636275f, 5.21f);
        Vector3 idealCamPos = new Vector3(-3f, 23f, -3f); // Tu posición manual de trabajo

        // 1. Calculamos el vector de desplazamiento en el Mundo (D = C - P)
        Vector3 worldDisplacement = idealCamPos - playerPos;

        // 2. Transformamos el vector de desplazamiento (D) al espacio local de la cámara (R^-1 * D)
        Quaternion inverseCamRotation = Quaternion.Inverse(transform.rotation);
        Vector3 idealOffsetVector = inverseCamRotation * worldDisplacement;

        Debug.Log("--- CÁLCULO DE OFFSET IDEAL ---");
        Debug.Log($"El OFFSET ideal para el Inspector es: {idealOffsetVector}");
        Debug.Log("--- COPIA ESTOS VALORES AL CAMPO 'Offset' ---");
    }
}