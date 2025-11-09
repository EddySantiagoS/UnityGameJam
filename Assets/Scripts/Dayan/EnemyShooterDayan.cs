using UnityEngine;

public class EnemyShooterDayan : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject projectilePrefab;
    public Transform playerTarget;

    [Header("Estadísticas de Disparo")]
    public float fireRate = 1f;
    public float projectileSpeed = 10f;

    [Header("IA de Apuntado")]
    public float rotationSpeed = 5f;


    [Header("IA de Detección")]
    [Tooltip("Distancia máxima a la que el enemigo te detectará y disparará")]
    public float shootingRange = 20f;

    [Tooltip("La capa que contiene tus muros (para el Raycast)")]
    public LayerMask obstacleLayerMask;


    private float timer;

    void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    void Update()
    {
        // Si el tiempo está detenido (o muy lento), no hacer nada
        if (Time.timeScale == 0) return;

        if (playerTarget == null) return;

        // --- ¡LÓGICA DE IA ACTUALIZADA! ---

        // --- INICIO DE LA MODIFICACIÓN (Pecho a Pecho) ---

        // 1. Definir los puntos de origen y destino del rayo (más robusto)
        // Asumimos que "pecho" está 0.5m por encima del pivote
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 targetChest = playerTarget.position + Vector3.up * 0.5f;

        // 2. Calcular la distancia y dirección REALES del rayo
        float distanceToPlayer = Vector3.Distance(rayOrigin, targetChest);
        Vector3 directionToPlayer = (targetChest - rayOrigin).normalized;

        // --- FIN DE LA MODIFICACIÓN ---


        // 3. COMPROBACIÓN DE RANGO
        if (distanceToPlayer > shootingRange)
        {
            // El jugador está muy lejos. No hacer NADA (ni rotar, ni disparar).
            return;
        }


        // Dibuja el rayo en la vista de Escena (¡muy útil!)
        // Verde si está en rango, Rojo si choca con algo
        Color rayColor = Color.green;
        // --- ---

        // 4. COMPROBACIÓN DE LÍNEA DE VISIÓN (LOS)
        // (Usamos los nuevos 'rayOrigin', 'directionToPlayer' y 'distanceToPlayer')
        if (Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstacleLayerMask))
        {
            // Hay una pared (Obstacle) entre el enemigo y el jugador.
            // No hacer NADA (ni rotar, ni disparar).

            rayColor = Color.red; // (Para depuración)

            // Dibuja el rayo de depuración (puedes borrar esto después)
            Debug.DrawRay(rayOrigin, directionToPlayer * distanceToPlayer, rayColor);
            return;
        }

        // Dibuja el rayo de depuración (puedes borrar esto después)
        Debug.DrawRay(rayOrigin, directionToPlayer * distanceToPlayer, rayColor);

        Vector3 directionToLook = directionToPlayer; 
        directionToLook.y = 0; 
        Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);


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
            projRb.linearVelocity = transform.forward * projectileSpeed;
        }
    }
}