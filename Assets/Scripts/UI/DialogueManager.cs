using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIndicator;

    [Header("Configuração")]
    [SerializeField] private float typewriterSpeed = 0.03f;

    private DialogueLine[] currentLines;
    private int            currentIndex;
    private PlayableDirector currentDirector;
    private bool           isTyping;
    private Coroutine      typewriterCoroutine;

    public bool IsOpen()
    {
        try { return dialoguePanel != null && dialoguePanel.activeSelf; }
        catch { dialoguePanel = null; return false; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // ── API pública ───────────────────────────────────────────────────────────

    public void StartDialogue(DialogueData data, PlayableDirector director)
    {
        if (data == null || data.lines.Length == 0) return;

        bool panelOk = false;
        try { panelOk = dialoguePanel != null; } catch { dialoguePanel = null; }
        if (!panelOk)
        {
            Debug.LogWarning("[DialogueManager] dialoguePanel destruído ou nulo — diálogo ignorado.");
            return;
        }

        currentLines    = data.lines;
        currentIndex    = 0;
        currentDirector = director;

        if (director != null) director.Pause();

        dialoguePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        ShowLine(currentLines[0]);
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!IsOpen()) return;

        var kb    = UnityEngine.InputSystem.Keyboard.current;
        var mouse = UnityEngine.InputSystem.Mouse.current;

        bool advance = (mouse != null && mouse.leftButton.wasPressedThisFrame)
                    || (kb    != null && (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame));

        if (advance) Advance();
    }

    // ── Lógica de avanço ─────────────────────────────────────────────────────

    private void Advance()
    {
        if (isTyping)
        {
            // Primeiro clique durante typewriter → mostra texto completo
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueText.text = currentLines[currentIndex].text;
            isTyping = false;
            SetContinueIndicator(true);
            return;
        }

        currentIndex++;

        if (currentIndex < currentLines.Length)
            ShowLine(currentLines[currentIndex]);
        else
            EndDialogue();
    }

    private void ShowLine(DialogueLine line)
    {
        nameText.text = line.characterName;
        SetContinueIndicator(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text));
    }

    private IEnumerator TypewriterRoutine(string text)
    {
        isTyping          = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        isTyping = false;
        SetContinueIndicator(true);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (currentDirector != null) currentDirector.Resume();

        currentDirector = null;
        currentLines    = null;
    }

    private void SetContinueIndicator(bool active)
    {
        if (continueIndicator != null) continueIndicator.SetActive(active);
    }
}
