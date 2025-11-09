using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// =========================================================================
// ESTRUCTURAS DE CONFIGURACIÓN
// =========================================================================

[System.Serializable]
public class TurretParameters
{

    [Header("Status")]
    [Tooltip("Activate or deactivate the Turret")]
    public bool active = true;
    public bool canFire = true;

    [Header("Shooting")]
    [Tooltip("Fuerza o daño del ataque (usado como velocidad del proyectil)")]
    public float power = 10f;
    [Tooltip("Pausa entre disparos (equivalente a fireRate)")]
    [Range(0.1f, 2)]
    public float ShootingDelay = 0.5f;

    [Tooltip("Radio de la torreta para detectar la entrada del jugador (OnTrigger)")]
    public float detectionRadius = 25f; // Corregido: antes era 'radius'
}

[System.Serializable]
public class TurretFX
{

    [Tooltip("Muzzle transform position")]
    public Transform muzzle;
    [Tooltip("Prefab del Proyectil (objeto físico que viaja)")]
    public GameObject projectilePrefab;
    [Tooltip("Prefab del destello visual de la boca del cañón")]
    public GameObject muzzleFlashFX;
}

[System.Serializable]
public class TurretAudio
{
    public AudioClip shotClip;
}

[System.Serializable]
public class TurretTargeting
{

    [Tooltip("Velocidad de rotación al apuntar")]
    public float aimingSpeed = 1f;
    [Tooltip("Pause before the aiming (no usado en FixedUpdate)")]
    public float aimingDelay;

    [Tooltip("Tags con las que se puede disparar. Debe ser 'Player'")]
    public string[] tagsToFire = { "Player" };
    public List<Collider> targets = new List<Collider>();
    public Collider target;
}


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class TurretShooterDayan : MonoBehaviour
{

    public TurretParameters parameters;
    public TurretTargeting targeting;
    public TurretFX VFX;
    public TurretAudio SFX;

    // --- ¡VARIABLES INTEGRALES DE LA IA DAYAN! ---
    [Header("IA DAYAN: Límites y Obstáculos")]
    [Tooltip("La capa que contiene tus muros (para el Raycast de LOS)")]
    public LayerMask obstacleLayerMask;
    [Tooltip("Distancia mínima al player que la torreta puede disparar")]
    public float minShootingDistance = 2f;
    // --- ---

    private void Awake()
    {

        // Configuramos el Rigidbody 
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;

        // Configuramos la esfera de detección
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = parameters.detectionRadius; // Usamos el radio corregido

        // Asignar el objetivo inicial (el jugador)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targeting.target = player.GetComponent<Collider>();
        }
    }

    private void FixedUpdate()
    {

        if (parameters.active == false)
        {
            return;
        }

        // Si no hay target, no hacemos nada.
        if (targeting.target == null)
        {
            ClearTargets();
            return;
        }

        // --- ¡LÓGICA DE DETECCIÓN Y BLOQUEO DE GIRO AÑADIDA! ---
        if (CanShootAtTarget())
        {

            Aiming(); // Solo gira si puede disparar

            // Solo disparamos si podemos ver al jugador.
            // Usamos Invoke para mantener la cadencia de disparo (ShootingDelay).
            if (!IsInvoking("Shooting"))
            {
                Invoke("Shooting", parameters.ShootingDelay);
            }
        }
       
    }

    // Comprueba si puede disparar (Rango y LOS) ---
    private bool CanShootAtTarget()
    {
        Transform playerTargetTransform = targeting.target.transform;

        Vector3 rayOrigin = VFX.muzzle.position;
        Vector3 targetPosition = playerTargetTransform.position;

        Vector3 directionToPlayer = (targetPosition - rayOrigin).normalized;
        float distanceToPlayer = Vector3.Distance(rayOrigin, targetPosition);

        // 1. Comprobación de RANGO MÍNIMO
        if (distanceToPlayer < minShootingDistance)
        {
            return false;
        }

        // 2. Comprobación de LÍNEA DE VISIÓN (LOS)
        if (Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstacleLayerMask))
        {
            return false; // Hay una pared (Obstacle)
        }

        return true; // Puede disparar
    }

    #region Aiming and Shooting

    private void Shot()
    {
        Debug.Log("Intentando disparar sonido.");
        if (SFX.shotClip != null) GetComponent<AudioSource>().PlayOneShot(SFX.shotClip, Random.Range(0.75f, 1));
        GetComponent<Animator>().SetTrigger("Shot");

        // 1. Instanciar el Muzzle Flash FX (Destello Visual)
        if (VFX.muzzleFlashFX != null)
        {
            // Creamos el flash como hijo del muzzle (para que se mueva con él)
            GameObject flash = Instantiate(VFX.muzzleFlashFX, VFX.muzzle.position, VFX.muzzle.rotation, VFX.muzzle);
            // Lo destruimos rápidamente (el flash es temporal)
            Destroy(flash, 0.5f);
        }

        // 2. Instanciar el Proyectil (Bala Física)
        if (VFX.projectilePrefab != null)
        {
            GameObject newProjectile = Instantiate(VFX.projectilePrefab, VFX.muzzle.position, VFX.muzzle.rotation);
            Rigidbody projRb = newProjectile.GetComponent<Rigidbody>();
            if (projRb != null)
            {
                projRb.linearVelocity = VFX.muzzle.forward * parameters.power;
            }
        }
    }

    private void Shooting()
    {

        // --- SOLUCIÓN: LIMPIEZA INMEDIATA DE INVOCACIÓN PENDIENTE ---
        // Esto evita que la torreta se "atasque" si la comprobación de pared falla (return;).
        CancelInvoke("Shooting");
        // --- FIN DE SOLUCIÓN ---

        if (targeting.target == null || parameters.canFire == false)
        {
            return;
        }

        Transform playerTargetTransform = targeting.target.transform;

        // --- BLOQUE DE CÁLCULO DE RAYCAST AJUSTADO ---

        // Posición de destino (pecho del jugador, para más precisión)
        Vector3 targetPosition = playerTargetTransform.position + Vector3.up * 0.5f;

        // Origen base (el Muzzle)
        Vector3 originBase = VFX.muzzle.position;

        // Dirección sin ajustar
        Vector3 directionRaw = (targetPosition - originBase).normalized;

        // Origen AJUSTADO: Empujamos 10cm hacia adelante para evitar el clipping del cañón.
        Vector3 rayOrigin = originBase + (directionRaw * 0.1f);

        // Recalculamos la dirección final
        Vector3 directionToPlayer = (targetPosition - rayOrigin).normalized;

        // Distancia: Se mide desde el origen ajustado hasta el objetivo.
        float distanceToPlayer = Vector3.Distance(rayOrigin, targetPosition);

        // --- FIN DEL BLOQUE DE CÁLCULO ---

        // 1. COMPROBACIÓN DE RANGO MÍNIMO
        if (distanceToPlayer < minShootingDistance)
        {
            return;
        }

        // 2. COMPROBACIÓN DE LÍNEA DE VISIÓN (LOS)
        if (Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstacleLayerMask))
        {
            // Debug.Log("Torreta bloqueada por muro."); // Log de depuración
            return; // Hay una pared, el proyectil no se dispara.
        }

        // --- SI PASA LA PRUEBA, CONTINÚA CON EL RAYCAST FINAL ---

        // Lógica final de Raycast (Comprobamos que estamos apuntando al Player)
        RaycastHit hit;

        // Usamos la dirección de disparo de la torreta (transform.forward) para la precisión final
        if (Physics.Raycast(VFX.muzzle.position, VFX.muzzle.transform.forward, out hit, distanceToPlayer))
        {

            // Comprobamos que el rayo haya impactado al objetivo (Player)
            if (CheckTags(hit.collider) == true)
            {
                Shot(); // ¡Disparo exitoso!
            }
        }
    }
    public void Aiming()
    {

        if (targeting.target == null)
        {
            return;
        }

        // Usamos la posición del muzzle para apuntar de forma más precisa
        Vector3 muzzlePos = VFX.muzzle.position;
        Vector3 delta = targeting.target.transform.position - muzzlePos;

        // Normalizamos la altura del vector para evitar que la torreta se incline excesivamente
        delta.y = 0;

        float angle = Vector3.Angle(transform.forward, delta);
        Vector3 cross = Vector3.Cross(transform.forward, delta);

        // Aplicamos la rotación
        GetComponent<Rigidbody>().AddTorque(cross * angle * targeting.aimingSpeed);
    }

    #endregion

    #region Targeting (Lógica de Asset Store)

    private void OnTriggerEnter(Collider other)
    {

        if (parameters.active == false)
        {
            return;
        }

        if (CheckTags(other) == true)
        {
            if (targeting.targets.Count == 0)
            {
                targeting.target = other.GetComponent<Collider>();
            }
            targeting.targets.Add(other.GetComponent<Collider>());
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (parameters.active == false)
        {
            return;
        }

        if (CheckTags(other) == true)
        {
            targeting.targets.Remove(other.GetComponent<Collider>());
            if (targeting.targets.Count != 0)
            {
                targeting.target = targeting.targets.First();
            }
            else
            {
                targeting.target = null;
            }
        }
    }

    private bool CheckTags(Collider toMatch)
    {

        bool Match = false;

        for (int i = 0; i < targeting.tagsToFire.Length; i++)
        {
            if (toMatch.tag == targeting.tagsToFire[i])
            {
                Match = true;
            }
        }

        return (Match);
    }

    private void ClearTargets()
    {

        if (targeting.target != null)
        {
            if (targeting.target.GetComponent<Collider>().enabled == false)
            {
                targeting.targets.Remove(targeting.target);
            }
        }

        foreach (Collider target in targeting.targets.ToList())
        {

            if (target == null)
            {
                targeting.targets.Remove(target);
            }

            if (targeting.targets.Count != 0)
            {
                targeting.target = targeting.targets.First();
            }
            else
            {
                targeting.target = null;
            }
        }
    }

    #endregion
}