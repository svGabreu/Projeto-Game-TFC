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
    // Abre o seletor e esconde o inventário
    // --------------------------------------------------------
    public void OpenForMuralPair(MuralSlotPairUI pair)
    {
        GenerateLetterButtons();

        currentPair = pair;
        if (panelRoot != null) panelRoot.SetActive(true);

        // Esconde o PainelInventario para o seletor ocupar o espaço
        MuralUI muralUI = FindObjectOfType<MuralUI>();
        if (muralUI != null)
        {
            GameObject inv = muralUI.GetPainelInventario();
            if (inv != null) inv.SetActive(false);
        }
    }

    // --------------------------------------------------------
    // Fecha o seletor e mostra o inventário de volta
    // --------------------------------------------------------
    public void Close()
    {
        currentPair = null;
        if (panelRoot != null) panelRoot.SetActive(false);

        // Mostra o PainelInventario de volta
        MuralUI muralUI = FindObjectOfType<MuralUI>();
        if (muralUI != null)
        {
            GameObject inv = muralUI.GetPainelInventario();
            if (inv != null) inv.SetActive(true);
        }
    }

    // Fecha sem reativar o inventário — usado ao abrir o mural
    public void ForceClose()
    {
        currentPair = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnLetterSelected(string letter)
    {
        if (currentPair != null)
            currentPair.SetChosenLetter(letter);
        Close();
    }
}