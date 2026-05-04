using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Fade UI")]
    public Canvas transitionCanvas;
    public Image fadeImage;

    [Header("Timing")]
    public float fadeOutDuration = 0.5f;
    public float fadeInDuration = 0.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (transitionCanvas != null)
        {
            DontDestroyOnLoad(transitionCanvas.gameObject);
        }
    }

    private void Start()
    {
        SetFadeAlpha(0f);
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndSwitchScene(sceneName));
    }

    private IEnumerator FadeAndSwitchScene(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return null;

        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float a = Mathf.Lerp(startAlpha, endAlpha, t);
            SetFadeAlpha(a);
            yield return null;
        }

        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}