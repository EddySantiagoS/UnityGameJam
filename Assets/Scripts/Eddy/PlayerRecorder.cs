using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    [System.Serializable]
    public struct FrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
    }

    [Header("Grabación")]
    public float recordDuration = 5f;
    public float recordInterval = 0.05f;

    private List<FrameData> frames = new List<FrameData>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recordInterval)
        {
            timer = 0f;
            RecordFrame();
        }
    }

    void RecordFrame()
    {
        frames.Add(new FrameData
        {
            position = transform.position,
            rotation = transform.rotation,
            time = Time.time
        });

        // mantener solo los últimos X segundos
        while (frames.Count > 1 && Time.time - frames[0].time > recordDuration)
            frames.RemoveAt(0);
    }

    public List<FrameData> GetSnapshot()
    {
        return new List<FrameData>(frames);
    }
}