using System.Collections.Generic;
using UnityEngine;

public class TimeRemnantManager : MonoBehaviour
{
    public GameObject remnantPrefab;
    public float spawnInterval = 6f;
    public int maxRemnants = 4;

    private PlayerRecorder recorder;
    private float timer;
    private Queue<GameObject> remnants = new Queue<GameObject>();

    void Start()
    {
        recorder = FindFirstObjectByType<PlayerRecorder>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRemnant();
        }
    }

    void SpawnRemnant()
    {
        if (recorder == null) return;

        var snapshot = recorder.GetSnapshot();
        if (snapshot.Count < 2) return;

        GameObject remnant = Instantiate(remnantPrefab, snapshot[0].position, snapshot[0].rotation);
        var ghost = remnant.GetComponent<TimeRemnant>();
        if (ghost != null)
            ghost.Initialize(snapshot);

        remnants.Enqueue(remnant);
        if (remnants.Count > maxRemnants)
            Destroy(remnants.Dequeue());
    }
}