using System.Collections.Generic;
using UnityEngine;

public class TimeRemnantManager : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject remnantPrefab;

    [Tooltip("Cantidad mínima de remanentes activos al mismo tiempo")]
    public int minRemnants = 3;

    [Tooltip("Cantidad máxima de remanentes activos al mismo tiempo")]
    public int maxRemnants = 6;

    [Tooltip("Cada cuántos segundos intenta generar un nuevo remanente")]
    public float spawnInterval = 3f;

    [Header("Área de aparición")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(50f, 0f, 50f);

    private PlayerRecorder recorder;
    private float timer = 0f;
    private List<GameObject> activeRemnants = new List<GameObject>();

    void Start()
    {
        recorder = FindFirstObjectByType<PlayerRecorder>();
    }

    void Update()
    {
        if (recorder == null) return;

        // Limpia los que fueron destruidos
        activeRemnants.RemoveAll(r => r == null);

        // Garantiza que haya al menos minRemnants
        while (activeRemnants.Count < minRemnants)
        {
            TrySpawnRemnant();
        }

        // Cada cierto tiempo, intenta crear otro remanente
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnRemnant();
        }

        // Si supera el máximo, elimina el más antiguo
        while (activeRemnants.Count > maxRemnants)
        {
            Destroy(activeRemnants[0]);
            activeRemnants.RemoveAt(0);
        }
    }

    void TrySpawnRemnant()
    {
        if (activeRemnants.Count >= maxRemnants) return;
        if (recorder == null) return;

        // --- posición base: alrededor del jugador ---
        Vector3 playerPos = recorder.transform.position;
        float spawnRadius = 25f; // radio máximo desde el jugador

        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            10f,
            Random.Range(-spawnRadius, spawnRadius)
        );

        Vector3 spawnPos = playerPos + randomOffset;

        // --- Ajustar a la superficie ---
        if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 50f))
            spawnPos = hit.point;
        else
            spawnPos.y = playerPos.y;

        // --- Instanciar remanente ---
        GameObject remnant = Instantiate(remnantPrefab, spawnPos, Quaternion.identity);

        var ghost = remnant.GetComponent<TimeRemnant>();
        if (ghost != null)
        {
            float randomOffsetTime = Random.Range(-1.5f, 1.5f);
            ghost.Initialize(recorder.GetSnapshot(), Vector3.zero, randomOffsetTime);
        }

        activeRemnants.Add(remnant);

        if (activeRemnants.Count > maxRemnants)
        {
            Destroy(activeRemnants[0]);
            activeRemnants.RemoveAt(0);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}