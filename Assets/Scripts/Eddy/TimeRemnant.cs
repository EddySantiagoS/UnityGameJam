using System.Collections.Generic;
using UnityEngine;

public class TimeRemnant : MonoBehaviour
{
    private List<PlayerRecorder.FrameData> path;
    private float localTimer;
    private int index;

    public float playbackSpeed = 1f;
    private float totalDuration;
    private Animator animator;
    private Vector3 lastPos;

    public void Initialize(List<PlayerRecorder.FrameData> snapshot)
    {
        float startTime = snapshot[0].time;
        for (int i = 0; i < snapshot.Count; i++)
        {
            var f = snapshot[i];
            f.time -= startTime;
            snapshot[i] = f;
        }

        path = snapshot;
        totalDuration = snapshot[snapshot.Count - 1].time;
        localTimer = 0f;
        index = 0;

        animator = GetComponent<Animator>();
        if (animator)
            animator.applyRootMotion = false;

        lastPos = transform.position;

        // Opcional: efecto visual transparente
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            if (rend.material.HasProperty("_Color"))
            {
                var c = rend.material.color;
                c.a = 0.4f;
                rend.material.color = c;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }

    void Update()
    {
        if (path == null || path.Count < 2)
        {
            Destroy(gameObject);
            return;
        }

        localTimer += Time.deltaTime * playbackSpeed;
        if (localTimer >= totalDuration)
        {
            Destroy(gameObject);
            return;
        }

        while (index < path.Count - 2 && path[index + 1].time < localTimer)
            index++;

        var a = path[index];
        var b = path[index + 1];
        float t = Mathf.InverseLerp(a.time, b.time, localTimer);

        transform.position = Vector3.Lerp(a.position, b.position, t);
        transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);

        // --- NUEVO: Actualizar animación según velocidad ---
        if (animator)
        {
            float speed = (transform.position - lastPos).magnitude / Time.deltaTime;
            animator.SetFloat("Speed", speed);
        }

        lastPos = transform.position;
    }
}