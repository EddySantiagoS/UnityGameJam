using UnityEngine;

public class EnemyShooterDayan : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    public float projectileSpeed = 10f;

    private float timer;

    void Update()
    {
        // Si el tiempo est� detenido, no hace nada
        if (Time.timeScale == 0) return;

        timer += Time.deltaTime; // Time.deltaTime se escala con Time.timeScale

        if (timer >= fireRate)
        {
            timer = 0;
            Shoot();
        }
    }

    void Shoot()
    {
        // Instancia el proyectil un poco adelante para que no colisione con el enemigo
        Vector3 spawnPos = transform.position + transform.forward;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, transform.rotation);

        // Aplica velocidad. Asume que el enemigo "mira" (transform.forward) al jugador
        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.linearVelocity = transform.forward * projectileSpeed;
        }
    }
}
