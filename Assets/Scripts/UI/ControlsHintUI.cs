using System.Collections;
using UnityEngine;

public class ControlsHintUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float exibirPor   = 8f;
    [SerializeField] private float fadeDuracao = 1.5f;

    private void Start() => StartCoroutine(ExibirEFadeOut());

    private IEnumerator ExibirEFadeOut()
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(exibirPor);

        float t = 0f;
        while (t < fadeDuracao)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuracao);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    // Chamado pelo Player ao abrir o pause — oculta o hint imediatamente
    public void OcultarImediato()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
