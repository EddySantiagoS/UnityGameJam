using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    [Header("Configuración del área de spawn")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(50f, 0f, 50f);
    public float spawnHeight = 40f;

    [Header("Meteoritos")]
    public GameObject meteorPrefab;
    public float spawnInterval = 2f;
    public int maxMeteors = 20;

    [Header("Fuerza y efectos")]
    public float fallTorque = 100f;
    public float randomForce = 50f;
    public float meteorLifetime = 8f;

    private int currentMeteorCount = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentMeteorCount < maxMeteors)
            {
                SpawnMeteor();
            }
        }
    }

    void SpawnMeteor()
    {
        // Generar posición aleatoria dentro del área
        Vector3 randomPos = new Vector3(
            Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            spawnHeight,
            Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
        );

        Vector3 spawnPos = areaCenter + randomPos;

        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Random.rotation);
        Rigidbody rb = meteor.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Agregar rotación aleatoria
            rb.AddTorque(Random.insideUnitSphere * fallTorque);
            // Pequeña fuerza lateral
            rb.AddForce(Random.insideUnitSphere * randomForce);
        }

        currentMeteorCount++;
        Destroy(meteor, meteorLifetime); // se destruye después de caer
        StartCoroutine(DecreaseCountAfter(meteorLifetime));
    }

    IEnumerator DecreaseCountAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentMeteorCount--;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(areaCenter + Vector3.up * spawnHeight, new Vector3(areaSize.x, 1f, areaSize.z));
    }
}