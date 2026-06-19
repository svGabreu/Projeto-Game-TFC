using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;

    [Header("Puppet da Cutscene")]
    [SerializeField] private GameObject cutscenePuppet;

    [Header("Prompt pós-cutscene (opcional)")]
    [SerializeField] private string postCutscenePrompt;

    [Header("Narrador pós-cutscene (opcional)")]
    [SerializeField] private NarratorCard       narratorCard;
    [SerializeField] private string             narratorMessage;

    [Header("Próxima cutscene (opcional)")]
    [SerializeField] private CutsceneController nextCutscene;

    [Header("Diálogo pós-cutscene (opcional)")]
    [SerializeField] private DialogueData nextDialogue;

    [Header("Cena pós-diálogo (opcional — substitui StartGameplay)")]
    [SerializeField] private string nextScene;

    [Header("Componentes do Jogador")]
    [SerializeField] private Player            playerScript;
    [SerializeField] private CameraControl     cameraControl;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Início")]
    [SerializeField] private bool autoStart = true;

    private Rigidbody playerRb;

    private void Start()
    {
        if (director != null) director.stopped += OnDirectorStopped;
        if (autoStart) StartCutscene();
    }

    private void ResolvePlayer(bool force = false)
    {
        bool needsResolve = force;
        if (!needsResolve)
        {
            try   { needsResolve = (playerScript == null); }
            catch { needsResolve = true; }
        }

        if (needsResolve)
        {
            Debug.Log("[CutsceneController] Buscando Player por tag...");
            var go = GameObject.FindWithTag("Player");
            if (go != null)
            {
                playerScript      = go.GetComponent<Player>();
                playerInteraction = go.GetComponent<PlayerInteraction>();
                cameraControl     = go.GetComponentInChildren<CameraControl>()
                                 ?? FindFirstObjectByType<CameraControl>();
                Debug.Log($"[CutsceneController] Player encontrado: {go.name}");
            }
            else Debug.LogWarning("[CutsceneController] Nenhum Player encontrado com tag 'Player'.");
        }

        try   { playerRb = playerScript != null ? playerScript.GetComponent<Rigidbody>() : null; }
        catch { playerRb = null; }
    }

    private void OnDestroy()
    {
        if (director != null) director.stopped -= OnDirectorStopped;
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen())
            StartCoroutine(WaitForDialogueThenStartGameplay());
        else
            AfterCutscene();
    }

    private void AfterCutscene()
    {
        if (narratorCard != null && !string.IsNullOrEmpty(narratorMessage))
        {
            narratorCard.Show(narratorMessage, OnNarratorDone);
            return;
        }
        OnNarratorDone();
    }

    private void OnNarratorDone()
    {
        if (nextCutscene != null)
        {
            nextCutscene.StartCutscene();
            return;
        }
        if (nextDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(nextDialogue, null);
            StartCoroutine(WaitForDialogueThenFinish());
            return;
        }
        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneTransitionManager.Instance?.GoToScene(nextScene);
            return;
        }
        StartGameplay();
    }

    private IEnumerator WaitForDialogueThenFinish()
    {
        yield return new WaitUntil(() =>
            DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen());

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneTransitionManager.Instance?.GoToScene(nextScene);
            yield break;
        }
        StartGameplay();
    }

    private IEnumerator WaitForDialogueThenStartGameplay()
    {
        yield return new WaitUntil(() =>
            DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen());
        AfterCutscene();
    }

    public void StartCutscene()
    {
        ResolvePlayer();

        SafeSetActive(cutscenePuppet, true, "cutscenePuppet");
        HidePlayer();

        if (playerRb          != null) playerRb.isKinematic      = true;
        if (cameraControl     != null) cameraControl.enabled     = false;
        if (playerInteraction != null) playerInteraction.enabled = false;

        if (director != null) director.Play();
    }

    private void HidePlayer()
    {
        if (playerScript == null) { Debug.LogWarning("[CutsceneController] playerScript null — player não ocultado."); return; }
        try   { playerScript.gameObject.SetActive(false); }
        catch { Debug.LogWarning("[CutsceneController] playerScript.gameObject destruído — buscando novamente.");
                ResolvePlayer(force: true);
                if (playerScript != null) playerScript.gameObject.SetActive(false); }
    }

    private static void SafeSetActive(GameObject go, bool active, string label)
    {
        if (go == null) { Debug.LogWarning($"[CutsceneController] {label} null — ignorado."); return; }
        try { go.SetActive(active); }
        catch (System.Exception e) { Debug.LogError($"[CutsceneController] {label}.SetActive({active}) falhou: {e.Message}"); }
    }

    public void StartGameplay()
    {
        if (director != null) director.Stop();

        SafeSetActive(cutscenePuppet, false, "cutscenePuppet");
        if (playerScript != null)
        {
            try { playerScript.gameObject.SetActive(true); }
            catch { Debug.LogWarning("[CutsceneController] playerScript destruído em StartGameplay."); }
        }

        if (playerRb          != null) playerRb.isKinematic      = false;
        if (playerScript      != null) playerScript.enabled      = true;
        if (playerInteraction != null) playerInteraction.enabled = true;

        if (cameraControl != null)
        {
            cameraControl.enabled = true;
            cameraControl.SnapCameraToPlayer();
        }

        if (!string.IsNullOrEmpty(postCutscenePrompt))
            PromptManager.Instance?.Show(postCutscenePrompt);
    }
}
