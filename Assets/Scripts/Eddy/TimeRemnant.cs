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

    private Vector3 baseOffset; // <-- nuevo: diferencia entre spawn y posición original

    public void Initialize(List<PlayerRecorder.FrameData> snapshot, Vector3 positionOffset, float timeOffset = 0f)
    {
        if (snapshot == null || snapshot.Count < 2)
        {
            Destroy(gameObject);
            return;
        }

        // normalizar tiempos relativos (empezando en 0)
        float start = snapshot[0].time;
        for (int i = 0; i < snapshot.Count; i++)
        {
            var f = snapshot[i];
            f.time -= start;
            snapshot[i] = f;
        }

        path = snapshot;
        totalDuration = path[path.Count - 1].time;
        localTimer = Mathf.Clamp(timeOffset, 0f, totalDuration);
        index = 0;

        animator = GetComponent<Animator>();
        if (animator) animator.applyRootMotion = false;

        // --- Nuevo: calculamos un offset entre donde apareció y donde estaba el jugador ---
        baseOffset = transform.position - path[0].position;

        // efecto visual (transparencia)
        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            if (rend.material.HasProperty("_Color"))
            {
                var c = rend.material.color;
                c.a = 0.45f;
                rend.material.color = c;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        lastPos = transform.position;
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

        // avanzar index hasta el frame correcto
        while (index < path.Count - 2 && path[index + 1].time < localTimer)
            index++;

        var a = path[index];
        var b = path[index + 1];
        float t = Mathf.InverseLerp(a.time, b.time, localTimer);

        // --- Aplica el offset relativo ---
        Vector3 pos = Vector3.Lerp(a.position, b.position, t) + baseOffset;
        Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);

        // --- Ajuste al terreno ---
        if (Physics.Raycast(pos + Vector3.up * 1f, Vector3.down, out RaycastHit groundHit, 5f))
        {
            pos.y = groundHit.point.y;
        }

        transform.position = pos;
        transform.rotation = rot;

        // actualizar animador según velocidad local
        if (animator)
        {
            float speed = (transform.position - lastPos).magnitude / Time.deltaTime;
            animator.SetFloat("Speed", speed);
        }

        lastPos = transform.position;
    }
}