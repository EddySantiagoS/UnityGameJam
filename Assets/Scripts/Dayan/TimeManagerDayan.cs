using UnityEngine;

public class TimeManagerDayan : MonoBehaviour
{
public static TimeManagerDayan Instance;
    
    [Tooltip("El objeto del jugador que tiene el script PlayerControllerDayan")]
    public PlayerControllerDayan player;

    public float targetTimeScale = 1f;
    public float smoothSpeed = 5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        float desired = PlayerIsMoving() ? targetTimeScale : 0f;
        
        // Usamos Time.unscaledDeltaTime para que el Lerp funcione incluso cuando Time.timeScale es 0
        Time.timeScale = Mathf.Lerp(Time.timeScale, desired, Time.unscaledDeltaTime * smoothSpeed);
    }

    bool PlayerIsMoving()
    {
        return player != null && player.GetVelocityMagnitude() > 0.1f;
    }
}
