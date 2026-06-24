// MuralCoordinator.cs
// Escuta os dois MuralPuzzles (A e B) e libera o Amuleto da Palavra quando ambos forem concluídos.
// Coloque num GameObject na cena Casa_Hieroglifos.

using UnityEngine;

public class MuralCoordinator : MonoBehaviour
{
    [Header("Murais")]
    public MuralPuzzle muralA;
    public MuralPuzzle muralB;

    [Header("Recompensa")]
    public GameObject rewardWorldObject;    // WorldClue com Amuleto da Palavra — começa desativado
    public DialogueData completionDialogue; // opcional — Anjinho comenta ao concluir

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
        // Recompensa já liberada em sessão anterior
        if (GSM != null && GSM.GetBool(KEY_REWARD))
        {
            ActivateReward();
            return;
        }

        // Fallback: murais já restaurados mas eventos disparados antes do Awake
        if (muralA != null && muralA.IsDone) muralADone = true;
        if (muralB != null && muralB.IsDone) muralBDone = true;
        CheckBothDone();
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
            rewardWorldObject.SetActive(true);
            Debug.Log("[MuralCoordinator] Amuleto da Palavra ativado!");
        }

        if (completionDialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(completionDialogue, null);
    }

    private void OnDestroy()
    {
        if (muralA != null) muralA.OnMuralCompleted.RemoveListener(OnMuralACompleted);
        if (muralB != null) muralB.OnMuralCompleted.RemoveListener(OnMuralBCompleted);
    }
}
