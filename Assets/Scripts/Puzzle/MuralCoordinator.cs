// MuralCoordinator.cs
// Escuta os dois MuralPuzzles (A e B) e libera o Amuleto da Palavra quando ambos forem concluídos.
// Coloque num GameObject na cena Casa_Hieroglifos.

using System.Collections;
using UnityEngine;

public class MuralCoordinator : MonoBehaviour
{
    [Header("Murais")]
    public MuralPuzzle muralA;
    public MuralPuzzle muralB;

    [Header("Recompensa")]
    public GameObject rewardWorldObject;    // WorldClue com Amuleto da Palavra — começa desativado
    public DialogueData completionDialogue; // opcional — Anjinho comenta ao concluir

    [Header("Puppet")]
    [SerializeField] private GameObject puppetAnjinho; // puppet 3D na cena — começa desativado

    private bool muralADone = false;
    private bool muralBDone = false;
    private bool rewardGiven = false;

    private const string KEY_REWARD = "mural.reward.given";
    private GameStateManager GSM => GameStateManager.Instance;

    // Awake registra os listeners antes de qualquer Start() rodar.
    // Assim, quando MuralPuzzle.Start() restaurar o estado e disparar OnMuralCompleted,
    // este coordenador já está ouvindo.
    private void Awake()
    {
        if (muralA != null) muralA.OnMuralCompleted.AddListener(OnMuralACompleted);
        if (muralB != null) muralB.OnMuralCompleted.AddListener(OnMuralBCompleted);
    }

    private void Start()
    {
        if (GSM != null && GSM.GetBool(KEY_REWARD))
        {
            rewardGiven = true;
            // Só reativa se o item ainda não foi coletado pelo jogador
            if (rewardWorldObject != null && !IsRewardCollected())
                rewardWorldObject.SetActive(true);
            return;
        }

        // MuralPuzzle vive dentro de um painel inativo — Start() só dispara ao abrir o painel.
        // Lê o estado salvo diretamente do GSM para não depender de IsDone neste momento.
        if (muralA != null && GSM != null)
            muralADone = GSM.GetBool("mural." + muralA.muralID + ".done");
        if (muralB != null && GSM != null)
            muralBDone = GSM.GetBool("mural." + muralB.muralID + ".done");
        CheckBothDone();
    }

    // Retorna true se o item da recompensa já foi coletado para o inventário
    private bool IsRewardCollected()
    {
        if (rewardWorldObject == null) return false;
        var wc = rewardWorldObject.GetComponent<WorldClue>();
        return wc != null && wc.itemToGive != null
               && InventoryManager.Instance != null
               && InventoryManager.Instance.HasItem(wc.itemToGive.itemID);
    }

    private void OnMuralACompleted() { muralADone = true; CheckBothDone(); }
    private void OnMuralBCompleted() { muralBDone = true; CheckBothDone(); }

    private void CheckBothDone()
    {
        if (rewardGiven || !muralADone || !muralBDone) return;

        GSM?.SetBool(KEY_REWARD, true);
        ActivateReward();
    }

    private void ActivateReward()
    {
        if (rewardGiven) return;
        rewardGiven = true;

        if (rewardWorldObject != null)
        {
            if (!IsRewardCollected())
            {
                rewardWorldObject.SetActive(true);
                Debug.Log("[MuralCoordinator] Amuleto da Palavra ativado!");
            }
            else
            {
                Debug.Log("[MuralCoordinator] Amuleto já coletado — não reativado.");
            }
        }

        if (completionDialogue != null && DialogueManager.Instance != null)
        {
            SetPlayerLocked(true);
            StartCoroutine(IniciarCutsceneAnjinho());
        }
    }

    private IEnumerator IniciarCutsceneAnjinho()
    {
        // Aguarda 1.5s com o painel de conclusão visível antes de iniciar o diálogo
        yield return new WaitForSecondsRealtime(1.5f);

        // Fecha qualquer mural aberto (restaura Time.timeScale = 1)
        var murals = FindObjectsByType<MuralUI>(FindObjectsSortMode.None);
        foreach (var m in murals) if (m.IsOpen()) m.CloseMural();

        // Exibe o puppet do Anjinho/Leuviah na cena
        if (puppetAnjinho != null) puppetAnjinho.SetActive(true);

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[MuralCoordinator] DialogueManager não encontrado na cena.");
            if (puppetAnjinho != null) puppetAnjinho.SetActive(false);
            SetPlayerLocked(false);
            yield break;
        }

        // Inicia o diálogo; ao final, oculta o puppet e destrava o jogador
        DialogueManager.Instance.StartDialogue(completionDialogue, null, () =>
        {
            if (puppetAnjinho != null) puppetAnjinho.SetActive(false);
            SetPlayerLocked(false);
        });
    }

    private void SetPlayerLocked(bool locked)
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null) return;
        var pScript  = playerGO.GetComponent<Player>();
        var pInteract = playerGO.GetComponent<PlayerInteraction>();
        var rb        = playerGO.GetComponent<Rigidbody>();
        if (pScript   != null) pScript.enabled   = !locked;
        if (pInteract != null) pInteract.enabled = !locked;
        if (rb        != null) rb.isKinematic    = locked;
    }

    private void OnDestroy()
    {
        if (muralA != null) muralA.OnMuralCompleted.RemoveListener(OnMuralACompleted);
        if (muralB != null) muralB.OnMuralCompleted.RemoveListener(OnMuralBCompleted);
    }
}
