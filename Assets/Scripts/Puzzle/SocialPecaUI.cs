// SocialPecaUI.cs
// Coloque em: Assets/Scripts/Puzzle/
// Representa UMA peça (personagem egípcio) no painel de identificação — Etapa 1.
// Quando o nome correto é atribuído: slot fica verde e a estátua 3D é desbloqueada.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SocialPecaUI : MonoBehaviour
{
    [Header("Configuração")]
    public string expectedNameItemID;   // "name_farao" | "name_sacerdotes" | etc.
    public SocialStatue linkedStatue;   // estátua 3D correspondente na mesa

    [Header("Visual do Painel")]
    public Image characterImage;        // sprite/ícone do personagem
    public Button slotButton;           // botão do slot abaixo da imagem
    public TextMeshProUGUI slotLabel;   // texto do slot
    public Image slotBackground;        // cor de feedback do slot

    [Header("Cores")]
    public Color defaultColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);
    public Color correctColor = new Color(0.2f,  0.8f,  0.2f,  0.8f);
    public Color wrongColor   = new Color(0.8f,  0.2f,  0.2f,  0.8f);

    // ---- Estado ----
    private bool isNamed = false;
    public bool IsNamed => isNamed;

    // --------------------------------------------------------
    private void Start()
    {
        if (slotButton     != null) slotButton.onClick.AddListener(OnSlotClicked);
        if (slotBackground != null) slotBackground.color = defaultColor;
        if (slotLabel      != null) slotLabel.text       = "— ? —";

        // Apaga texto padrão "Button" do filho interno do SlotButton
        if (slotButton != null)
        {
            var txt = slotButton.GetComponentInChildren<TMP_Text>();
            if (txt != null && txt != slotLabel) txt.text = "";
        }
    }

    // --------------------------------------------------------
    private void OnSlotClicked()
    {
        if (isNamed) return;
        SocialUI.Instance?.OnPecaSlotClicked(this);
    }

    // --------------------------------------------------------
    // Chamado por SocialPuzzle.TryAssignName()
    // --------------------------------------------------------
    public bool TryAssignName(GlyphItem item)
    {
        if (isNamed) return false;

        if (item.itemID == expectedNameItemID)
        {
            isNamed = true;
            if (slotLabel      != null) slotLabel.text          = item.displayName;
            if (slotBackground != null) slotBackground.color    = correctColor;
            if (slotButton     != null) slotButton.interactable = false;

            // Consome o pergaminho de nome
            InventoryManager.Instance.RemoveItem(item.itemID);

            // Desbloqueia a estátua 3D para coleta com E
            if (linkedStatue != null)
                linkedStatue.Unlock();

            return true;
        }
        else
        {
            StartCoroutine(FlashWrong());
            return false;
        }
    }

    private IEnumerator FlashWrong()
    {
        if (slotBackground != null) slotBackground.color = wrongColor;
        yield return new WaitForSecondsRealtime(1f);
        if (slotBackground != null) slotBackground.color = defaultColor;
    }

    // --------------------------------------------------------
    // Restaura estado ao reentrar na cena
    // --------------------------------------------------------
    public void RestoreNamed(string displayName)
    {
        isNamed = true;
        if (slotLabel      != null) slotLabel.text          = displayName;
        if (slotBackground != null) slotBackground.color    = correctColor;
        if (slotButton     != null) slotButton.interactable = false;

        // A estátua pode já estar coletada — só desbloqueia se ainda existir
        if (linkedStatue != null && !linkedStatue.IsCollected)
            linkedStatue.Unlock();
    }
}
