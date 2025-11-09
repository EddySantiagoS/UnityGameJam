using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [System.Serializable]
    public class Waypoint
    {
        public Transform point;      // El cubo o vacío que marca la posición destino
        public float moveSpeed = 2f; // Velocidad de movimiento hacia este punto
        public float rotSpeed = 2f;  // Velocidad de rotación hacia este punto
    }

    [Header("Puntos del recorrido (en orden)")]
    public Waypoint[] waypoints;

    [Header("Configuración general")]
    public bool startOnPlay = true;
    public bool loop = false;

    private int currentIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        if (startOnPlay)
            StartMovement();
    }

    void Update()
    {
        if (!isMoving || waypoints.Length == 0)
            return;

        MoveToCurrentWaypoint();
    }

    // ======================================================
    //      INICIAR MOVIMIENTO
    // ======================================================
    public void StartMovement()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No hay waypoints asignados al ObjectMover.");
            return;
        }

        isMoving = true;
        currentIndex = 0;
    }

    // ======================================================
    //      DETENER MOVIMIENTO
    // ======================================================
    public void StopMovement()
    {
        isMoving = false;
    }

    // ======================================================
    //      MOVER HACIA EL PUNTO ACTUAL + ROTAR
    // ======================================================
    void MoveToCurrentWaypoint()
    {
        Transform target = waypoints[currentIndex].point;
        float moveSpeed = waypoints[currentIndex].moveSpeed;
        float rotSpeed = waypoints[currentIndex].rotSpeed;

        if (target == null)
        {
            Debug.LogWarning("Uno de los waypoints no tiene un Transform asignado.");
            NextPoint();
            return;
        }

        // ---------------------------
        // MOVIMIENTO SUAVE
        // ---------------------------
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // ---------------------------
        // ROTACIÓN SUAVE
        // ---------------------------
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target.rotation,
            rotSpeed * Time.deltaTime
        );

        // ---------------------------
        // SI YA LLEGÓ → SIGUIENTE PUNTO
        // ---------------------------
        if (Vector3.Distance(transform.position, target.position) < 0.05f &&
            Quaternion.Angle(transform.rotation, target.rotation) < 1f)
        {
            NextPoint();
        }
    }

    // ======================================================
    //      CAMBIAR AL SIGUIENTE PUNTO
    // ======================================================
    void NextPoint()
    {
        currentIndex++;

        if (currentIndex >= waypoints.Length)
        {
            if (loop)
                currentIndex = 0;
            else
            {
                isMoving = false;
                return;
            }
        }
    }
}
