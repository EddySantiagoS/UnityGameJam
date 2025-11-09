using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerControllerDayan : MonoBehaviour
{
    [Header("Dash Básico")]
    public float dashDuration = 0.2f;

    [Header("Carga de Dash (Nuevo)")]
    public float forceMultiplier = 10f;
    public float minDashForce = 5f;
    public float maxDashForce = 20f;

    [Header("Dash Line (LineRenderer)")]
    [Tooltip("La longitud que tendrá la línea de dash.")]
    public float dashLineLength = 10f;

    [Header("Punta de Flecha (¡NUEVO!)")]
    [Tooltip("Arrastra aquí el Prefab de tu punta de flecha")]
    public GameObject arrowHeadPrefab;
    [Tooltip("Qué tan lejos del final de la línea se posiciona la punta (ajusta para que se vea bien)")]
    public float arrowHeadOffset = -0.5f;

    [Header("Efectos Visuales (¡NUEVO!)")]
    [Tooltip("El Prefab del efecto de 'clic' que se instanciará")]
    public GameObject clickEffectPrefab;
    [Tooltip("La capa (Layer) que representa el suelo clickeable")]
    public LayerMask groundLayerMask;

    private GameObject currentClickEffect; // Para controlar el spam

    // --- (Variables privadas sin cambios, excepto 'lineRenderer') ---
    private Rigidbody rb;
    private PlayerInputActions inputActions;
    private Vector2 mouseScreenPosition;
    private GameObject currentArrowHead; // La flecha instanciada
    private bool isDashing = false;
    private bool isChargingDash = false;
    private LineRenderer lineRenderer; // Quitamos la variable duplicada 'dashLine'
    private Camera mainCamera;

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

        // --- ¡NUEVO! ---
        if (arrowHeadPrefab != null)
        {
            currentArrowHead = Instantiate(arrowHeadPrefab);
            currentArrowHead.SetActive(false);
        }
        // --- ---
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
    }

    private void OnDashPressed(InputAction.CallbackContext context)
    {
        if (isDashing) return;

        isChargingDash = true;

        if (clickEffectPrefab != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayerMask))
            {
                if (currentClickEffect != null)
                {
                    Destroy(currentClickEffect);
                }

                // 1. Calculamos la posición correcta
                Vector3 spawnPos = hit.point + (Vector3.up * 0.1f);

                // 2. ¡Usamos 'spawnPos' aquí!
                currentClickEffect = Instantiate(clickEffectPrefab, spawnPos, Quaternion.Euler(-90, 0, 0));
            }
        }
    }

    private void OnDashReleased(InputAction.CallbackContext context)
    {
        if (!isChargingDash) return;

        isChargingDash = false;
        lineRenderer.enabled = false; // Ocultar línea

        if (currentArrowHead != null)
        {
            currentArrowHead.SetActive(false);
        }
        // --- ---

        //if (currentClickEffect != null)
        //{
        //    Destroy(currentClickEffect);
        //    currentClickEffect = null;
        //}

        // Calcular dirección y fuerza (sin cambios)
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos == Vector3.positiveInfinity) return;
        Vector3 dragVector = mouseWorldPos - transform.position;
        Vector3 dashDirection = new Vector3(dragVector.x, 0, dragVector.z).normalized;
        if (dashDirection == Vector3.zero) return;
        float dragMagnitude = dragVector.magnitude;
        float dashForce = Mathf.Clamp(dragMagnitude * forceMultiplier, minDashForce, maxDashForce);
        StartCoroutine(Dash(dashDirection, dashForce));
    }

    // --- ¡FUNCIÓN COMPLETAMENTE NUEVA! ---
    // REEMPLAZA ESTA FUNCIÓN COMPLETA
    private void DrawChargeIndicator()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        if (mousePos == Vector3.positiveInfinity) return;

        // 1. Calcular el vector 2D (plano)
        Vector3 dragVector = mousePos - transform.position;
        dragVector.y = 0; // Aplanar
        Vector3 direction = dragVector.normalized;
        float dragDistance = dragVector.magnitude; // Distancia real del arrastre

        // 2. Comprobar si estamos apuntando (evitar spam)
        if (dragDistance < 0.1f)
        {
            lineRenderer.enabled = false;
            if (currentArrowHead != null) currentArrowHead.SetActive(false);
            return;
        }

        // 3. Asegurarse de que todo esté visible
        lineRenderer.enabled = true;
        if (currentArrowHead != null) currentArrowHead.SetActive(true);

        // 4. Calcular la longitud visual CLAMPED (Limitada)
        // Esta es la lógica clave que refleja tu min/max dash force.
        // (Usamos la lógica de tu OnDashReleased para que coincida)
        float clampedDistance = Mathf.Clamp(
            dragDistance,
            minDashForce / forceMultiplier,
            maxDashForce / forceMultiplier
        );

        // 5. Definir el punto final de la línea
        Vector3 lineEnd = transform.position + (direction * clampedDistance);

        // 6. Aplicar al LineRenderer (¡AHORA SE ESTIRA!)
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, lineEnd);

        // 7. Aplicar a la punta de la flecha
        if (currentArrowHead != null)
        {
            // Posicionamos la flecha en el punto final + offset
            currentArrowHead.transform.position = lineEnd + (direction * arrowHeadOffset);

            // Rotamos la flecha
            currentArrowHead.transform.rotation = Quaternion.LookRotation(direction);
        }
    }


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