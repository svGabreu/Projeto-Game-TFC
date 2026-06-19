// NotebookHUDButton.cs
// Coloque em: Assets/Scripts/UI/
// Attach no botao do caderno no HUD (icone N na tela).
// Mostra um badge com a contagem de entradas nao lidas.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotebookHUDButton : MonoBehaviour
{
    [Header("Botao")]
    public Button button;

    [Header("Badge de entradas novas (opcional)")]
    public GameObject badgeRoot;            // objeto com o circulo vermelho
    public TextMeshProUGUI badgeCount;      // numero dentro do badge

    private int lastKnownCount = 0;

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);

        if (NotebookManager.Instance != null)
            NotebookManager.Instance.OnNotebookUpdated.AddListener(AtualizarBadge);

        AtualizarBadge();
    }

    private void OnClick()
    {
        if (NotebookUI.Instance != null)
        {
            NotebookUI.Instance.TogglePanel();
            // Zera badge ao abrir
            lastKnownCount = NotebookManager.Instance != null
                ? NotebookManager.Instance.GetAllEntries().Count
                : 0;
            if (badgeRoot != null) badgeRoot.SetActive(false);
        }
    }

    private void AtualizarBadge()
    {
        if (NotebookManager.Instance == null) return;

        int total = NotebookManager.Instance.GetAllEntries().Count;
        int novas = total - lastKnownCount;

        if (badgeRoot != null)
            badgeRoot.SetActive(novas > 0);

        if (badgeCount != null)
            badgeCount.text = novas > 0 ? novas.ToString() : "";
    }
}
