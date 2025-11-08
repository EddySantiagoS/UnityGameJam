using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControllerDayan : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float moveSpeed = 5f; // Aunque no se usa para moverse, se puede usar para la velocidad del dash
    public float dashForce = 15f;
    public float dashDuration = 0.2f;

    private bool isDashing = false;
    private Rigidbody rb;
    private PlayerInputActions inputActions; // Variable para las acciones
    private Vector2 mouseScreenPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Inicializa las acciones de entrada
        inputActions = new PlayerInputActions();

        // Suscr�bete a los eventos
        // "Dash.performed" se dispara cuando presionas el bot�n
        inputActions.Player.Dash.performed += _ => OnDash();

        // "MousePosition.performed" actualiza la posici�n del mouse continuamente
        inputActions.Player.MousePosition.performed += ctx => mouseScreenPosition = ctx.ReadValue<Vector2>();
    }

    // Activa las acciones cuando el objeto est� habilitado
    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    // Desactiva las acciones cuando el objeto est� deshabilitado
    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // Esta funci�n es llamada por el evento de entrada
    private void OnDash()
    {
        // El chequeo de 'Update' se mueve aqu�
        if (!isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        Vector3 dir = GetDashDirection();
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dir * dashForce;
            yield return null;
        }
        rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    Vector3 GetDashDirection()
    {
        // Convierte posici�n del mouse en vector en el plano
        // Usamos la variable 'mouseScreenPosition' actualizada por el Input System
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);

        // Asumimos que tienes un suelo o un plano invisible para el raycast
        // Si no, el Raycast puede fallar. Aseg�rate de tener un LayerMask o un plano.
        // Aqu� usar� un Plane en Y=0 para asegurar que siempre golpee.

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Plano en Y=0
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 dir = (hitPoint - transform.position).normalized;
            return new Vector3(dir.x, 0, dir.z); // Asegura que sea en el plano XZ
        }

        // Fallback por si acaso
        return transform.forward;
    }

    // Esta funci�n la llamar� el TimeManagerDayan
    public float GetVelocityMagnitude()
    {
        return rb.linearVelocity.magnitude;
    }
}
