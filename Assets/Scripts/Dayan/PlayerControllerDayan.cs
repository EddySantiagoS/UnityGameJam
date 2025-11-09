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

    // --- ROTACIÓN & AUDIO ---
    [Header("Visuales y Rotación")]
    public float headingRotationSpeed = 10f;
    [Tooltip("Capas que deben activar el sonido de choque (Muros, Suelo, etc.).")]
    public LayerMask collisionLayersForHitSound; // Nueva variable pública

    [Header("Audio Sources (Asignar en Inspector)")]
    public AudioSource normalTimeAudioSource; // Dash (Ignore Listener Pause = True)
    public AudioSource slowMoAudioSource;   // Choque (Ignore Listener Pause = False)
    public AudioClip wallHitSound;

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

    // --- VARIABLES PRIVADAS Y DE ESTADO ---
    private Rigidbody rb;
    private PlayerInputActions inputActions;
    private Vector2 mouseScreenPosition;
    private GameObject currentArrowHead;
    private GameObject currentClickEffect;
    private bool isDashing = false;
    private bool isChargingDash = false;
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private Coroutine dashCoroutine;

    // VARIABLES DE CÁLCULO DE DASH (Para corregir errores de contexto)
    private Vector3 lastDashDirection = Vector3.forward;
    private Vector3 calculatedDashDirection;
    private float calculatedDashForce;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        // Ya no necesitamos GetComponent<AudioSource>() si usamos las referencias públicas.

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
            DrawChargeIndicator(); // Corregido: Ahora existe
        }

        HandleRotations();
    }

    // --- LÓGICA DE CHOQUE Y SONIDO (CORREGIDO) ---
    void OnCollisionEnter(Collision collision)
    {
        // 1. Comprobar si golpea una superficie de choque (Muro, Suelo, etc.)
        bool isWallOrGround = ((1 << collision.gameObject.layer) & collisionLayersForHitSound) != 0;

        if (isWallOrGround)
        {
            // Usamos el AudioSource que se ralentiza (slowMoAudioSource)
            if (slowMoAudioSource != null && wallHitSound != null)
            {
                float hitVolume = collision.relativeVelocity.magnitude * 0.05f;

                // Solo reproducir si el impacto es significativo
                if (hitVolume > 0.1f)
                {
                    hitVolume = Mathf.Clamp(hitVolume, 0.1f, 0.8f);
                    slowMoAudioSource.PlayOneShot(wallHitSound, hitVolume);
                }
            }
        }
    }

    // --- ROTACIONES (Solo calcula la dirección) ---
    private void HandleRotations()
    {
        if (rb == null) return;

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
        // NOTA: La rotación visual la aplica el script VisualsFollowerDayan.cs
    }


    // --- EVENTOS DE INPUT (CORREGIDOS) ---
    private void OnDashPressed(InputAction.CallbackContext context)
    {
        // CORRECCIÓN 1: Detenemos cualquier Dash/Gracia en curso para permitir el re-click rápido.
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            isDashing = false;
            dashCoroutine = null;
        }

        isChargingDash = true;

        // ACTIVACIÓN: El tiempo se vuelve normal (1f) durante la carga.
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

        // CORRECCIÓN 2: Almacenamos la dirección y fuerza en variables de clase
        calculatedDashDirection = new Vector3(dragVector.x, 0, dragVector.z).normalized;

        if (calculatedDashDirection == Vector3.zero) return;

        lastDashDirection = calculatedDashDirection;

        float dragMagnitude = dragVector.magnitude;
        calculatedDashForce = Mathf.Clamp(dragMagnitude * forceMultiplier, minDashForce, maxDashForce);

        dashCoroutine = StartCoroutine(DashFlow(calculatedDashDirection, calculatedDashForce));
    }

    // --- CORUTINAS DE MECÁNICA ---
    IEnumerator DashFlow(Vector3 dir, float force)
    {
        // Esto gestiona la referencia de la corrutina.
        yield return StartCoroutine(DashExecution(dir, force));
        dashCoroutine = null;
    }

    IEnumerator DashExecution(Vector3 dir, float force)
    {
        // 1. FASE DE DASH (Propulsión)
        isDashing = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dir * force;
            yield return null;
        }

        // 2. FINAL DEL DASH Y GRACIA
        rb.linearVelocity = Vector3.zero;
        isDashing = false;

        if (slowMoDurationAfterDash > 0)
        {
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

    // --- AYUDAS (CORREGIDO: El método DrawChargeIndicator) ---
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

    // --- GETTER PARA ROTACIÓN VISUAL ---
    public Vector3 GetLastDashDirection()
    {
        return lastDashDirection;
    }

    public float GetVelocityMagnitude()
    {
        return rb.linearVelocity.magnitude;
    }
}