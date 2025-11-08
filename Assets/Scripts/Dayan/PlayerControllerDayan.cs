using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))] // Asegura que el LineRenderer esté
public class PlayerControllerDayan : MonoBehaviour
{
    [Header("Dash Básico")]
    public float dashDuration = 0.2f;

    [Header("Carga de Dash (Nuevo)")]
    public float forceMultiplier = 10f; // Multiplica la distancia de arrastre
    public float minDashForce = 5f;
    public float maxDashForce = 20f;

    private Rigidbody rb;
    private PlayerInputActions inputActions;
    private Vector2 mouseScreenPosition;

    // Estado del jugador
    private bool isDashing = false;
    private bool isChargingDash = false; // NUEVO: true cuando mantienes presionado

    // Componentes para el indicador
    private LineRenderer lineRenderer;
    private Camera mainCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main; // Guardamos la cámara para eficiencia

        // --- Configuración del Input ---
        inputActions = new PlayerInputActions();

        // Suscríbete a los eventos de mouse
        inputActions.Player.MousePosition.performed += ctx => mouseScreenPosition = ctx.ReadValue<Vector2>();

        // MODIFICADO: Suscribirse a PRESIONAR y SOLTAR
        inputActions.Player.Dash.performed += OnDashPressed; // Se llama al presionar
        inputActions.Player.Dash.canceled += OnDashReleased; // Se llama al soltar

        // --- Configuración del LineRenderer ---
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // Una línea simple (inicio y fin)
        lineRenderer.enabled = false;   // Oculto por defecto
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // NUEVO: Se llama CADA FRAME
    void Update()
    {
        // Si estamos cargando el dash, dibuja la línea indicadora
        if (isChargingDash)
        {
            DrawChargeIndicator();
        }
    }

    // NUEVO: Se llama al PRESIONAR el mouse
    private void OnDashPressed(InputAction.CallbackContext context)
    {
        if (isDashing) return; // No se puede cargar si ya está en dash

        isChargingDash = true;
        lineRenderer.enabled = true;
    }

    // NUEVO: Se llama al SOLTAR el mouse
    private void OnDashReleased(InputAction.CallbackContext context)
    {
        if (!isChargingDash) return; // Salió de un estado inválido

        isChargingDash = false;
        lineRenderer.enabled = false;

        // Calcular dirección y fuerza
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos == Vector3.positiveInfinity) return; // Fallo del Raycast

        // Vector desde el jugador hasta el ratón
        Vector3 dragVector = mouseWorldPos - transform.position;

        // Dirección (plana, en XZ)
        Vector3 dashDirection = new Vector3(dragVector.x, 0, dragVector.z).normalized;

        // Si la dirección es casi cero (clic sin arrastrar), cancela
        if (dashDirection == Vector3.zero) return;

        // Fuerza (basada en la distancia de arrastre)
        float dragMagnitude = dragVector.magnitude;
        float dashForce = Mathf.Clamp(dragMagnitude * forceMultiplier, minDashForce, maxDashForce);

        // Iniciar el Dash
        StartCoroutine(Dash(dashDirection, dashForce));
    }

    // NUEVO: Dibuja la línea desde el jugador hasta el ratón
    private void DrawChargeIndicator()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        if (mousePos == Vector3.positiveInfinity) return;

        lineRenderer.SetPosition(0, transform.position); // Inicio en el jugador

        // Vector del jugador al ratón
        Vector3 dragVector = mousePos - transform.position;
        float dragDistance = dragVector.magnitude;

        // Limita la longitud visual de la línea al dash máximo
        float clampedDistance = Mathf.Min(dragDistance, maxDashForce / forceMultiplier);
        Vector3 lineEnd = transform.position + (dragVector.normalized * clampedDistance);

        lineRenderer.SetPosition(1, lineEnd); // Fin en la posición del ratón (limitada)
    }

    // MODIFICADO: Ahora acepta dirección y fuerza
    IEnumerator Dash(Vector3 dir, float force)
    {
        isDashing = true;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dir * force;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    // MODIFICADO: Renombrada y optimizada
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);

        // Plano al nivel del jugador (Y=1)
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.positiveInfinity; // Indica un fallo
    }

    // Esta función no cambia, TimeManagerDayan la sigue usando
    public float GetVelocityMagnitude()
    {
        return rb.linearVelocity.magnitude;
    }
}