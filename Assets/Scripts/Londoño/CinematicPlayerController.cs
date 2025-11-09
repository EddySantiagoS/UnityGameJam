using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;

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

    [Header("Duraciones de cada animación")]
    public float time1 = 5f;
    public float time2 = 5f;
    public float time3 = 5f;
    public float time4 = 5f;
    public float timeFinal = 5f;

    [Header("Duración de transición entre animaciones")]
    public float fadeDuration = 0.3f;

    [Header("Flash Panel (con componente Image)")]
    public GameObject panelFlash;
    private Image flashImage;

    [Header("Sonido del flash (se reproduce una vez)")]
    public AudioSource flashSource;  // ← AudioSource dedicado
    public AudioClip flashSound;     // ← Clip del sonido

    [Header("Objetos a activar al final")]
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;
    public GameObject object4;

    [Header("Escena final")]
    public string nextSceneName = "NuevaEscena";
    public float delayBeforeSceneChange = 4f;

    [Header("Ocultar Player (solo modelos, no el objeto principal)")]
    public GameObject[] objectsToHide;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (panelFlash != null)
        {
            flashImage = panelFlash.GetComponent<Image>();
            panelFlash.SetActive(false);
        }

        if (object1 != null) object1.SetActive(false);
        if (object2 != null) object2.SetActive(false);
        if (object3 != null) object3.SetActive(false);
        if (object4 != null) object4.SetActive(false);

        StartCoroutine(PlaySequence());
    }

    private System.Collections.IEnumerator PlaySequence()
    {
        animator.CrossFade(anim1, fadeDuration);
        yield return new WaitForSeconds(time1);

        animator.CrossFade(anim2, fadeDuration);
        yield return new WaitForSeconds(time2);

        animator.CrossFade(anim3, fadeDuration);
        yield return new WaitForSeconds(time3);

        animator.CrossFade(anim4, fadeDuration);
        yield return new WaitForSeconds(time4);

        animator.CrossFade(animFinal, fadeDuration);
        yield return new WaitForSeconds(timeFinal);

        HidePlayerVisual();

        // FLASH CON SONIDO
        if (panelFlash != null)
            StartCoroutine(FlashEffect());

        object1?.SetActive(true);
        object2?.SetActive(true);
        object3?.SetActive(true);
        object4?.SetActive(true);

        yield return new WaitForSeconds(delayBeforeSceneChange);
        SceneManager.LoadScene(nextSceneName);
    }

    void HidePlayerVisual()
    {
        if (objectsToHide == null) return;

        foreach (var obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        panelFlash.SetActive(true);

        // ✅ Reproducir sonido UNA SOLA VEZ
        if (flashSource != null && flashSound != null)
            flashSource.PlayOneShot(flashSound);

        yield return new WaitForSeconds(1f);

        panelFlash.SetActive(false);
    }
}
