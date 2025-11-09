using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerControllerDayan : MonoBehaviour
{
    // --- NUEVAS VARIABLES DE TIEMPO ---
    [Header("Control de Tiempo")]
    [Tooltip("Tiempo en segundos que durará el tiempo normal después del Dash.")]
    public float slowMoDurationAfterDash = 0.5f;
    [Tooltip("El factor de tiempo (e.g., 0.2 = 20% de velocidad).")]
    public float slowMoFactor = 0.2f;

    // --- VARIABLES DE ROTACIÓN ---
    [Header("Visuales y Rotación")]
    public Transform visualsContainer;
    public float headingRotationSpeed = 10f;

    // --- VARIABLES DE DASH Y EFECTOS ---
    [Header("Dash Básico")]
    public float dashDuration = 0.2f;

    [Header("Carga de Dash")]
    public float forceMultiplier = 10f;
    public float minDashForce = 5f;
    public float maxDashForce = 20f;

    [Header("Dash Line")]
    public float dashLineLength = 10f;

    [Header("Punta de Flecha")]
    public GameObject arrowHeadPrefab;
    public float arrowHeadOffset = -0.5f;

    [Header("Efectos Visuales")]
    public GameObject clickEffectPrefab;
    public LayerMask groundLayerMask;

    // --- VARIABLES PRIVADAS ---
    private Rigidbody rb;
    private PlayerInputActions inputActions;
    private Vector2 mouseScreenPosition;
    private GameObject currentArrowHead;
    private GameObject currentClickEffect;
    private bool isDashing = false;
    private bool isChargingDash = false;
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private Vector3 lastDashDirection = Vector3.forward;

    // --- SETUP ---

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        inputActions = new PlayerInputActions();
        inputActions.Player.MousePosition.performed += ctx => mouseScreenPosition = ctx.ReadValue<Vector2>();
        inputActions.Player.Dash.performed += OnDashPressed;
        inputActions.Player.Dash.canceled += OnDashReleased;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        if (arrowHeadPrefab != null)
        {
            currentArrowHead = Instantiate(arrowHeadPrefab);
            currentArrowHead.SetActive(false);
        }
    }

    void Start()
    {
        // En lugar de Time.timeScale = slowMoFactor;
        if (TimeManagerDayan.Instance != null)
        {
            TimeManagerDayan.Instance.SetTimeScaleSmooth(slowMoFactor);
        }
        else
        {
            Time.timeScale = slowMoFactor;
        }
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        if (isChargingDash)
        {
            DrawChargeIndicator();
        }

        HandleRotations();
    }

    // --- ROTACIONES (Sin Cambios) ---
    private void HandleRotations()
    {
        if (rb == null || visualsContainer == null) return;

        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0;

        if (currentVelocity.magnitude > 0.1f)
        {
            lastDashDirection = currentVelocity.normalized;
        }
        else if (isChargingDash)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            if (mousePos != Vector3.positiveInfinity)
            {
                Vector3 dragVector = mousePos - transform.position;
                dragVector.y = 0;
                if (dragVector.magnitude > 0.1f)
                {
                    lastDashDirection = dragVector.normalized;
                }
            }
        }

        if (lastDashDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastDashDirection);
            visualsContainer.rotation = Quaternion.Slerp(
                visualsContainer.rotation,
                targetRotation,
                headingRotationSpeed * Time.deltaTime
            );
        }
    }

    // --- EVENTOS DE INPUT ---
    private void OnDashPressed(InputAction.CallbackContext context)
    {
        if (isDashing) return;

        isChargingDash = true;

        // 1. ACTIVACIÓN: El tiempo se vuelve normal (1f) durante la carga y el Dash.
        // Usamos el TimeManager para una transición suave.
        if (TimeManagerDayan.Instance != null)
        {
            TimeManagerDayan.Instance.SetTimeScaleSmooth(1f);
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (clickEffectPrefab != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayerMask))
            {
                if (currentClickEffect != null) Destroy(currentClickEffect);
                Vector3 spawnPos = hit.point + (Vector3.up * 0.1f);
                currentClickEffect = Instantiate(clickEffectPrefab, spawnPos, Quaternion.Euler(-90, 0, 0));
            }
        }
    }

    private void OnDashReleased(InputAction.CallbackContext context)
    {
        if (!isChargingDash) return;

        isChargingDash = false;
        lineRenderer.enabled = false;
        if (currentArrowHead != null) currentArrowHead.SetActive(false);

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos == Vector3.positiveInfinity) return;

        Vector3 dragVector = mouseWorldPos - transform.position;
        Vector3 dashDirection = new Vector3(dragVector.x, 0, dragVector.z).normalized;
        if (dashDirection == Vector3.zero) return;

        lastDashDirection = dashDirection;

        float dragMagnitude = dragVector.magnitude;
        float dashForce = Mathf.Clamp(dragMagnitude * forceMultiplier, minDashForce, maxDashForce);
        StartCoroutine(Dash(dashDirection, dashForce));
    }

    // --- CORUTINAS DE MECÁNICA ---

    IEnumerator Dash(Vector3 dir, float force)
    {
        // 1. FASE DE DASH (Propulsión)
        isDashing = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dir * force;
            yield return null;
        }

        // 2. FINAL DEL DASH Y GRACIA: Mantenemos el tiempo en 1f (Normal).
        rb.linearVelocity = Vector3.zero;
        isDashing = false;

        if (slowMoDurationAfterDash > 0)
        {
            // Esperar el periodo de gracia en tiempo normal
            yield return new WaitForSecondsRealtime(slowMoDurationAfterDash);

            // 3. VUELTA AL ESTADO POR DEFECTO: SLOW MOTION
            if (TimeManagerDayan.Instance != null)
            {
                TimeManagerDayan.Instance.SetTimeScaleSmooth(slowMoFactor);
            }
            else
            {
                Time.timeScale = slowMoFactor;
            }
        }
    }

    // --- AYUDAS (Sin Cambios) ---

    private void DrawChargeIndicator()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        if (mousePos == Vector3.positiveInfinity) return;

        Vector3 dragVector = mousePos - transform.position;
        dragVector.y = 0;
        Vector3 direction = dragVector.normalized;
        float dragDistance = dragVector.magnitude;

        if (dragDistance < 0.1f)
        {
            lineRenderer.enabled = false;
            if (currentArrowHead != null) currentArrowHead.SetActive(false);
            return;
        }

        lineRenderer.enabled = true;
        if (currentArrowHead != null) currentArrowHead.SetActive(true);

        float clampedDistance = Mathf.Clamp(
            dragDistance,
            minDashForce / forceMultiplier,
            maxDashForce / forceMultiplier
        );

        Vector3 lineEnd = transform.position + (direction * clampedDistance);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, lineEnd);

        if (currentArrowHead != null)
        {
            currentArrowHead.transform.position = lineEnd + (direction * arrowHeadOffset);
            currentArrowHead.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.positiveInfinity;
    }

    public float GetVelocityMagnitude()
    {
        return rb.linearVelocity.magnitude;
    }
}