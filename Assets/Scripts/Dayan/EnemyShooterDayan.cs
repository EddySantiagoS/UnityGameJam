using UnityEngine;

public class EnemyShooterDayan : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject projectilePrefab;

    [Tooltip("El objetivo al que el enemigo apuntará (El Jugador)")]
    public Transform playerTarget; // Arrastra al jugador aquí

    [Header("Estadísticas de Disparo")]
    public float fireRate = 1f;
    public float projectileSpeed = 10f;

    [Header("IA de Apuntado")]
    [Tooltip("Qué tan rápido el enemigo rota para apuntar al jugador")]
    public float rotationSpeed = 5f;

    private float timer;

    // --- NUEVO: Intentar encontrar al jugador si no está asignado ---
    void Start()
    {
        if (playerTarget == null)
        {
            // Intenta encontrar al jugador por el tag. 
            // Esto es útil para la futura generación procedural.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    void Update()
    {
        // Si el tiempo está detenido, no hacer absolutamente NADA
        if (Time.timeScale == 0) return;

        // --- NUEVO: Lógica de Rotación ---
        if (playerTarget != null)
        {
            // 1. Calcular la dirección hacia el jugador (solo en el plano XZ)
            Vector3 directionToPlayer = playerTarget.position - transform.position;
            directionToPlayer.y = 0; // Ignora la altura

            // 2. Calcular la rotación deseada
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // 3. Aplicar la rotación suavemente
            // La clave está en "Time.deltaTime":
            // Cuando Time.timeScale es 0, Time.deltaTime también es 0.
            // Esto detiene la rotación Slerp automáticamente.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // --- Lógica de Disparo (Sin cambios) ---
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            timer = 0;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 spawnPos = transform.position + transform.forward;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, transform.rotation);

        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            // Ahora 'transform.forward' está apuntando (o intentando apuntar) al jugador
            projRb.linearVelocity = transform.forward * projectileSpeed;
        }
    }
}