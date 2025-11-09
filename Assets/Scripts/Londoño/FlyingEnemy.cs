using UnityEngine;
using System;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 85f;

    [Header("Vida del misil")]
    public float lifetime = 10f;

    [Header("Referencia al objeto Frente (punta del misil)")]
    public Transform frente; // ← Arrastra aquí la esfera "Frente"

    public Action onDestroyed;

    private Vector3 moveDir;

    void Start()
    {
        // Si no asignaste la punta "Frente" en el prefab, la busca automáticamente
        if (frente == null)
        {
            Transform f = transform.Find("Frente");
            if (f != null)
                frente = f;
            else
                Debug.LogError("NO SE ENCONTRÓ EL OBJETO 'Frente' en el prefab del misil.");
        }

        // Generar rotación aleatoria en Y
        transform.rotation = Quaternion.Euler(
            0f,
            UnityEngine.Random.Range(0f, 360f),
            0f
        );

        // Calcular dirección verdadera hacia donde apunta el modelado
        UpdateForwardDirection();

        // Destruir después de X tiempo
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Mover en la dirección de la punta
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    void UpdateForwardDirection()
    {
        // Dirección REAL del misil basada en la esfera Frente
        moveDir = (frente.position - transform.position).normalized;

        if (moveDir == Vector3.zero)
        {
            moveDir = transform.forward; // fallback
        }
    }

    void OnDestroy()
    {
        onDestroyed?.Invoke();
    }
}
