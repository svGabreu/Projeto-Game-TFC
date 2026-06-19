using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class NarratorCard : MonoBehaviour
{
    [SerializeField] private CanvasGroup     canvasGroup;
    [SerializeField] private TextMeshProUGUI narratorText;
    [SerializeField] private float           fadeDuration    = 1f;
    [SerializeField] private float           displayDuration = 0f; // 0 = espera input do jogador

    private System.Action    onComplete;
    private bool             waitingForInput;
    private PlayableDirector director;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(string text, System.Action onComplete = null)
    {
        this.onComplete   = onComplete;
        this.director     = null;
        narratorText.text = text;
        gameObject.SetActive(true);
        StartCoroutine(ShowRoutine());
    }

    // Usa dentro de uma Timeline: pausa o director, mostra o texto, retoma ao fechar
    public void ShowAndPauseDirector(string text, PlayableDirector dir, System.Action onComplete = null)
    {
        this.director   = dir;
        this.onComplete = onComplete;
        if (dir != null) dir.Pause();
        narratorText.text = text;
        gameObject.SetActive(true);
        StartCoroutine(ShowRoutine());
    }

    private void Update()
    {
        if (!waitingForInput) return;

        var kb    = Keyboard.current;
        var mouse = Mouse.current;

        bool pressed = (kb    != null && (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame))
                    || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (pressed) StartCoroutine(HideAndComplete());
    }

    private IEnumerator ShowRoutine()
    {
        yield return FadeRoutine(0f, 1f);

        if (displayDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(displayDuration);
            yield return HideAndComplete();
        }
        else
        {
            waitingForInput = true;
        }
    }

    private IEnumerator HideAndComplete()
    {
        waitingForInput = false;
        yield return FadeRoutine(1f, 0f);
        gameObject.SetActive(false);
        if (director != null) { director.Resume(); director = null; }
        onComplete?.Invoke();
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed           += Time.unscaledDeltaTime;
            canvasGroup.alpha  = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
