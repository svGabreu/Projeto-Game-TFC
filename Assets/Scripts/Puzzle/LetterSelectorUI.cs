// LetterSelectorUI.cs
// Coloque em: Assets/Scripts/Puzzle/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterSelectorUI : MonoBehaviour
{
    [Header("Referências")]
    public GameObject panelRoot;
    public Transform buttonContainer;
    public GameObject letterButtonPrefab;

    private MuralSlotPairUI currentPair;
    private MuralUI callerMuralUI;
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private bool buttonsGenerated = false;

    private void Awake()
    {
        // Não gera botões no Awake — gera na primeira abertura
        // para garantir que o prefab está carregado
        ForceClose();
    }

    // --------------------------------------------------------
    // Gera os 26 botões — chamado uma vez na primeira abertura
    // --------------------------------------------------------
    private void GenerateLetterButtons()
    {
        if (buttonsGenerated) return;
        if (letterButtonPrefab == null || buttonContainer == null) return;

        foreach (char letter in alphabet)
        {
            GameObject btn = Instantiate(letterButtonPrefab, buttonContainer);
            string letterStr = letter.ToString();

            TextMeshProUGUI label =
                btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = letterStr;

            Button button = btn.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnLetterSelected(letterStr));
        }

        buttonsGenerated = true;
    }

    // --------------------------------------------------------
    // Abre o seletor — MuralUI já escondeu o painelMiniInventario
    // --------------------------------------------------------
    public void OpenForMuralPair(MuralSlotPairUI pair, MuralUI caller)
    {
        GenerateLetterButtons();

        currentPair   = pair;
        callerMuralUI = caller;
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    // --------------------------------------------------------
    // Fecha o seletor e pede ao MuralUI para restaurar o mini-inventário
    // --------------------------------------------------------
    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        if (callerMuralUI != null) callerMuralUI.ShowMiniInventory();

        currentPair   = null;
        callerMuralUI = null;
    }

    // Fecha sem notificar o MuralUI — usado internamente
    public void ForceClose()
    {
        currentPair   = null;
        callerMuralUI = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public bool IsOpen() => panelRoot != null && panelRoot.activeSelf;

    private void OnLetterSelected(string letter)
    {
        if (currentPair != null)
            currentPair.SetChosenLetter(letter);
        Close();
    }
}