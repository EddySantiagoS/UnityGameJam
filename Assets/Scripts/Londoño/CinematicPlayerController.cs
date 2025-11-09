using UnityEngine;

public class CinematicPlayerController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Nombres de animaciones en orden")]
    public string anim1 = "Anim1";
    public string anim2 = "Anim2";
    public string anim3 = "Anim3";
    public string anim4 = "Anim4";
    public string animFinal = "AnimFinal";

    [Header("Duraciones (segundos) de cada animación")]
    public float time1 = 5f;
    public float time2 = 5f;
    public float time3 = 5f;
    public float time4 = 5f;
    public float timeFinal = 5f;

    [Header("Duración de transición suave entre animaciones")]
    public float fadeDuration = 0.3f;  // ← Ajustable en el inspector

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        StartCoroutine(PlaySequence());
    }

    private System.Collections.IEnumerator PlaySequence()
    {
        // Animación 1
        animator.CrossFade(anim1, fadeDuration);
        yield return new WaitForSeconds(time1);

        // Animación 2
        animator.CrossFade(anim2, fadeDuration);
        yield return new WaitForSeconds(time2);

        // Animación 3
        animator.CrossFade(anim3, fadeDuration);
        yield return new WaitForSeconds(time3);

        // Animación 4
        animator.CrossFade(anim4, fadeDuration);
        yield return new WaitForSeconds(time4);

        // Animación Final (no loop)
        animator.CrossFade(animFinal, fadeDuration);
        yield return new WaitForSeconds(timeFinal);

        // Ocultar personaje al terminar todo
        gameObject.SetActive(false);
    }
}
