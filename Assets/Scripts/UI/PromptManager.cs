using System.Collections;
using TMPro;
using UnityEngine;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }

    [SerializeField] private CanvasGroup      canvasGroup;
    [SerializeField] private TextMeshProUGUI  promptText;
    [SerializeField] private float            fadeDuration = 0.4f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        canvasGroup.alpha = 0f;
    }

    public void Show(string text)
    {
        promptText.text = text;
        Fade(1f);
    }

    public void Hide()
    {
        Fade(0f);
    }

    private void Fade(float target)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start   = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            canvasGroup.alpha  = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
