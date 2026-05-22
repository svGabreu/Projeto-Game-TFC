// SetupMuralUI.cs — Editor script para configurar os painéis do Mural Puzzle
// Menu: Tools → Setup Mural UI
// Executa uma vez para organizar todos os RectTransforms do MuralPanel_A e MuralPanel_B
// e do LetterSelectorPanel (teclado horizontal) e do painel de inventário (backpack).

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SetupMuralUI
{
    [MenuItem("Tools/Setup Mural UI")]
    public static void Run()
    {
        // Procura o canvas do mural na cena
        Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        GameObject canvasGO = null;
        foreach (var c in allCanvases)
        {
            if (c.gameObject.name.Contains("Mural") || c.gameObject.name.Contains("mural"))
            {
                canvasGO = c.gameObject;
                break;
            }
        }

        if (canvasGO == null)
        {
            GameObject found = GameObject.Find("UI_Canvas_Mural");
            if (found != null) canvasGO = found;
        }

        if (canvasGO == null)
        {
            Debug.LogError("[SetupMuralUI] Não encontrou UI_Canvas_Mural na cena.");
            return;
        }

        SetupPanel(canvasGO, "MuralPanel_A", "SlotPairsContainer_A", "PainelInventario_A", "BtnFechar_A", "Mural A — NILO");
        SetupPanel(canvasGO, "MuralPanel_B", "SlotPairsContainer_B", "PainelInventario_B", "BtnFechar_B", "Mural B — ROTA");
        SetupLetterSelector(canvasGO);
        SetupInventoryPanel(canvasGO);
        WireMuralUIComponents(canvasGO);   // ← wiring dos MuralUI na cena (3D objects)

        EditorUtility.SetDirty(canvasGO.scene.GetRootGameObjects()[0]);
        Debug.Log("[SetupMuralUI] ✅ UI configurada com sucesso!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MURAL PANELS (A e B)
    // ──────────────────────────────────────────────────────────────────────────
    static void SetupPanel(GameObject canvasGO, string panelName, string slotsContainerName,
                           string inventoryName, string btnFecharName, string titleText)
    {
        Transform panel = canvasGO.transform.Find(panelName);
        if (panel == null)
        {
            Debug.LogWarning($"[SetupMuralUI] Painel '{panelName}' não encontrado.");
            return;
        }

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        SetStretchFull(panelRT);

        Image panelImg = panel.GetComponent<Image>();
        if (panelImg == null) panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.sprite = null;
        panelImg.color = new Color(0f, 0f, 0f, 0.90f);
        panelImg.raycastTarget = true;

        // Vertical Layout Group — conteúdo centralizado verticalmente
        VerticalLayoutGroup vlg = panel.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 40, 40);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;   // ← centraliza verticalmente
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ----- Título -----
        Transform titleTr = panel.Find("TitleText");
        if (titleTr == null)
        {
            GameObject titleGO = new GameObject("TitleText", typeof(RectTransform));
            titleGO.transform.SetParent(panel, false);
            titleGO.transform.SetAsFirstSibling();
            titleTr = titleGO.transform;
        }
        RectTransform titleRT = titleTr.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(0, 64);
        TextMeshProUGUI titleTMP = titleTr.GetComponent<TextMeshProUGUI>();
        if (titleTMP == null) titleTMP = titleTr.gameObject.AddComponent<TextMeshProUGUI>();
        titleTMP.text = titleText;
        titleTMP.fontSize = 30;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.85f, 0.3f);   // dourado
        LayoutElement titleLE = titleTr.GetComponent<LayoutElement>();
        if (titleLE == null) titleLE = titleTr.gameObject.AddComponent<LayoutElement>();
        titleLE.minHeight = 64;
        titleLE.preferredHeight = 64;

        // Ordem: TitleText → SlotPairs → PainelInventario
        titleTr.SetAsFirstSibling();
        Transform slotsTr = panel.Find(slotsContainerName);
        if (slotsTr != null) slotsTr.SetSiblingIndex(1);
        Transform invTrOrder = panel.Find(inventoryName);
        if (invTrOrder != null) invTrOrder.SetSiblingIndex(2);

        // ----- SlotPairsContainer -----
        if (slotsTr == null)
        {
            Debug.LogWarning($"[SetupMuralUI] '{slotsContainerName}' não encontrado em '{panelName}'.");
        }
        else
        {
            RectTransform slotsRT = slotsTr.GetComponent<RectTransform>();
            slotsRT.sizeDelta = new Vector2(0, 240);

            LayoutElement slotsLE = slotsTr.GetComponent<LayoutElement>();
            if (slotsLE == null) slotsLE = slotsTr.gameObject.AddComponent<LayoutElement>();
            slotsLE.minHeight = 240;
            slotsLE.preferredHeight = 240;

            HorizontalLayoutGroup hlg = slotsTr.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null) hlg = slotsTr.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(10, 10, 5, 5);

            foreach (Transform pair in slotsTr)
                SetupPair(pair);
        }

        // ----- PainelInventario (mini-inventory) -----
        Transform invTr = panel.Find(inventoryName);
        if (invTr != null)
        {
            RectTransform invRT = invTr.GetComponent<RectTransform>();
            invRT.sizeDelta = new Vector2(0, 150);

            LayoutElement invLE = invTr.GetComponent<LayoutElement>();
            if (invLE == null) invLE = invTr.gameObject.AddComponent<LayoutElement>();
            invLE.minHeight = 150;
            invLE.preferredHeight = 150;
            invLE.flexibleHeight = 0;

            Image invImg = invTr.GetComponent<Image>();
            if (invImg == null) invImg = invTr.gameObject.AddComponent<Image>();
            invImg.sprite = null;
            invImg.color = new Color(0.08f, 0.08f, 0.08f, 0.75f);

            VerticalLayoutGroup invVlg = invTr.GetComponent<VerticalLayoutGroup>();
            if (invVlg == null) invVlg = invTr.gameObject.AddComponent<VerticalLayoutGroup>();
            invVlg.padding = new RectOffset(10, 10, 8, 8);
            invVlg.spacing = 5;
            invVlg.childAlignment = TextAnchor.UpperCenter;
            invVlg.childControlWidth = true;
            invVlg.childControlHeight = false;
            invVlg.childForceExpandWidth = true;
            invVlg.childForceExpandHeight = false;

            // Label
            Transform lblTr = invTr.Find("LabelInventario");
            if (lblTr == null)
            {
                GameObject lblGO = new GameObject("LabelInventario", typeof(RectTransform));
                lblGO.transform.SetParent(invTr, false);
                lblGO.transform.SetAsFirstSibling();
                lblTr = lblGO.transform;
            }
            RectTransform lblRT = lblTr.GetComponent<RectTransform>();
            lblRT.sizeDelta = new Vector2(0, 28);
            TextMeshProUGUI lblTMP = lblTr.GetComponent<TextMeshProUGUI>();
            if (lblTMP == null) lblTMP = lblTr.gameObject.AddComponent<TextMeshProUGUI>();
            lblTMP.text = "🎒 Inventário — clique num item e depois na silhueta";
            lblTMP.fontSize = 15;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.color = new Color(0.9f, 0.8f, 0.3f);
            LayoutElement lblLE = lblTr.GetComponent<LayoutElement>();
            if (lblLE == null) lblLE = lblTr.gameObject.AddComponent<LayoutElement>();
            lblLE.minHeight = 28;
            lblLE.preferredHeight = 28;

            // MiniInventory container
            string miniName = inventoryName.Replace("PainelInventario", "MiniInventory");
            Transform miniTr = invTr.Find(miniName);
            if (miniTr == null) miniTr = panel.Find(miniName);
            if (miniTr != null)
            {
                RectTransform miniRT = miniTr.GetComponent<RectTransform>();
                miniRT.sizeDelta = new Vector2(0, 100);

                LayoutElement miniLE = miniTr.GetComponent<LayoutElement>();
                if (miniLE == null) miniLE = miniTr.gameObject.AddComponent<LayoutElement>();
                miniLE.minHeight = 100;
                miniLE.preferredHeight = 100;

                GridLayoutGroup glg = miniTr.GetComponent<GridLayoutGroup>();
                if (glg != null)
                {
                    glg.cellSize = new Vector2(88f, 88f);
                    glg.spacing = new Vector2(10f, 6f);
                    glg.childAlignment = TextAnchor.MiddleCenter;
                    glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    glg.constraintCount = 1;
                    glg.padding = new RectOffset(8, 8, 4, 4);
                }
                else
                {
                    HorizontalLayoutGroup miniHlg = miniTr.GetComponent<HorizontalLayoutGroup>();
                    if (miniHlg == null) miniHlg = miniTr.gameObject.AddComponent<HorizontalLayoutGroup>();
                    miniHlg.spacing = 12;
                    miniHlg.childAlignment = TextAnchor.MiddleCenter;
                    miniHlg.childControlWidth = false;
                    miniHlg.childControlHeight = false;
                    miniHlg.childForceExpandWidth = false;
                    miniHlg.childForceExpandHeight = false;
                }
            }
        }

        // ----- BtnFechar -----
        Transform btnTr = panel.Find(btnFecharName);
        if (btnTr != null)
        {
            RectTransform btnRT = btnTr.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(1f, 1f);
            btnRT.anchorMax = new Vector2(1f, 1f);
            btnRT.pivot = new Vector2(1f, 1f);
            btnRT.anchoredPosition = new Vector2(-15f, -15f);
            btnRT.sizeDelta = new Vector2(110f, 38f);

            Image btnImg = btnTr.GetComponent<Image>();
            if (btnImg == null) btnImg = btnTr.gameObject.AddComponent<Image>();
            btnImg.sprite = null;
            btnImg.color = new Color(0.7f, 0.15f, 0.15f, 1f);

            LayoutElement btnLE = btnTr.GetComponent<LayoutElement>();
            if (btnLE == null) btnLE = btnTr.gameObject.AddComponent<LayoutElement>();
            btnLE.ignoreLayout = true;

            TextMeshProUGUI btnTMP = btnTr.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTMP != null)
            {
                btnTMP.text = "✕ Fechar";
                btnTMP.fontSize = 17;
                btnTMP.alignment = TextAlignmentOptions.Center;
                btnTMP.color = Color.white;
            }
        }

        // Wiring feito em WireMuralUIComponents() após SetupLetterSelector()
        Debug.Log($"[SetupMuralUI] '{panelName}' configurado.");
    }

    static void SetupPair(Transform pair)
    {
        RectTransform pairRT = pair.GetComponent<RectTransform>();
        pairRT.sizeDelta = new Vector2(155f, 228f);

        VerticalLayoutGroup vlg = pair.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = pair.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        Image pairImg = pair.GetComponent<Image>();
        if (pairImg == null) pairImg = pair.gameObject.AddComponent<Image>();
        pairImg.sprite = null;
        pairImg.color = new Color(0.18f, 0.13f, 0.08f, 0.70f);

        Transform silTr = pair.Find("SilhouetteSlot");
        if (silTr != null)
        {
            RectTransform silRT = silTr.GetComponent<RectTransform>();
            silRT.sizeDelta = new Vector2(135f, 135f);

            Image silImg = silTr.GetComponent<Image>();
            if (silImg == null) silImg = silTr.gameObject.AddComponent<Image>();
            if (silImg.color == Color.white || silImg.color.a < 0.3f)
                silImg.color = new Color(0.28f, 0.22f, 0.16f, 1f);
        }

        Transform letTr = pair.Find("LetterSlot");
        if (letTr != null)
        {
            RectTransform letRT = letTr.GetComponent<RectTransform>();
            letRT.sizeDelta = new Vector2(135f, 62f);

            Image letImg = letTr.GetComponent<Image>();
            if (letImg == null) letImg = letTr.gameObject.AddComponent<Image>();
            if (letImg.color == Color.white || letImg.color.a < 0.3f)
                letImg.color = new Color(0.22f, 0.18f, 0.32f, 1f);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LETTER SELECTOR — teclado horizontal 2 linhas (A-M / N-Z)
    // ──────────────────────────────────────────────────────────────────────────
    // ──────────────────────────────────────────────────────────────────────────
    // Configura o LetterSelectorPanel DENTRO de cada MuralPanel (A e B),
    // como irmão do PainelInventario no VLG — ocupa o mesmo espaço.
    // ──────────────────────────────────────────────────────────────────────────
    static void SetupLetterSelector(GameObject canvasGO)
    {
        // Configura em ambos os murais
        SetupLetterSelectorForMural(canvasGO, "MuralPanel_A", "PainelInventario_A");
        SetupLetterSelectorForMural(canvasGO, "MuralPanel_B", "PainelInventario_B");

        // Se ainda existir um LetterSelectorPanel antigo na raiz do canvas, desativa
        Transform oldPanel = canvasGO.transform.Find("LetterSelectorPanel");
        if (oldPanel != null) oldPanel.gameObject.SetActive(false);

        Debug.Log("[SetupMuralUI] LetterSelectorPanel configurado dentro de cada MuralPanel.");
    }

    static void SetupLetterSelectorForMural(GameObject canvasGO, string muralPanelName, string painelInvName)
    {
        Transform muralTr = canvasGO.transform.Find(muralPanelName);
        if (muralTr == null) { Debug.LogWarning($"[SetupMuralUI] {muralPanelName} não encontrado."); return; }

        // Procura o LetterSelectorPanel já existente dentro do muralPanel
        string selectorName = "LetterSelectorPanel_" + muralPanelName.Replace("MuralPanel_", "");
        Transform selectorTr = muralTr.Find(selectorName);
        if (selectorTr == null)
        {
            // Tenta nome genérico também
            selectorTr = muralTr.Find("LetterSelectorPanel");
        }
        if (selectorTr == null)
        {
            // Cria o painel
            GameObject selectorGO = new GameObject(selectorName, typeof(RectTransform));
            selectorGO.transform.SetParent(muralTr, false);
            selectorTr = selectorGO.transform;
            Debug.Log($"[SetupMuralUI] {selectorName} criado dentro de {muralPanelName}.");
        }

        // ----- Painel raiz -----
        // Participa do VLG do muralPanel como irmão do PainelInventario
        RectTransform selectorRT = selectorTr.GetComponent<RectTransform>();
        selectorRT.sizeDelta = new Vector2(0, 160);

        LayoutElement le = selectorTr.GetComponent<LayoutElement>();
        if (le == null) le = selectorTr.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 160;
        le.preferredHeight = 160;
        le.ignoreLayout = false;   // participa do VLG

        Image img = selectorTr.GetComponent<Image>();
        if (img == null) img = selectorTr.gameObject.AddComponent<Image>();
        img.sprite = null;
        img.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        img.raycastTarget = true;

        // Posiciona logo após o PainelInventario no VLG
        Transform painelInvTr = muralTr.Find(painelInvName);
        if (painelInvTr != null)
            selectorTr.SetSiblingIndex(painelInvTr.GetSiblingIndex() + 1);

        // ----- Vertical layout dentro do seletor -----
        VerticalLayoutGroup vlg = selectorTr.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = selectorTr.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ----- Label -----
        Transform lblTr = selectorTr.Find("LabelSeletor");
        if (lblTr == null)
        {
            GameObject lblGO = new GameObject("LabelSeletor", typeof(RectTransform));
            lblGO.transform.SetParent(selectorTr, false);
            lblTr = lblGO.transform;
        }
        RectTransform lblRT = lblTr.GetComponent<RectTransform>();
        lblRT.sizeDelta = new Vector2(0, 28);
        TextMeshProUGUI lblTMP = lblTr.GetComponent<TextMeshProUGUI>();
        if (lblTMP == null) lblTMP = lblTr.gameObject.AddComponent<TextMeshProUGUI>();
        lblTMP.text = "Selecione a letra do hieróglifo";
        lblTMP.fontSize = 18;
        lblTMP.fontStyle = FontStyles.Bold;
        lblTMP.alignment = TextAlignmentOptions.Center;
        lblTMP.color = new Color(1f, 0.85f, 0.3f);
        LayoutElement lblLE = lblTr.GetComponent<LayoutElement>();
        if (lblLE == null) lblLE = lblTr.gameObject.AddComponent<LayoutElement>();
        lblLE.minHeight = 28;
        lblLE.preferredHeight = 28;

        // ----- LetterGrid -----
        Transform gridTr = selectorTr.Find("LetterGrid");
        if (gridTr == null)
        {
            // Tenta mover o grid do painel antigo se existir
            Transform oldRoot = canvasGO.transform.Find("LetterSelectorPanel");
            if (oldRoot != null)
            {
                Transform oldGrid = oldRoot.Find("LetterGrid");
                if (oldGrid != null) oldGrid.SetParent(selectorTr, false);
                gridTr = oldGrid;
            }
            if (gridTr == null)
            {
                GameObject gridGO = new GameObject("LetterGrid", typeof(RectTransform));
                gridGO.transform.SetParent(selectorTr, false);
                gridTr = gridGO.transform;
            }
        }

        RectTransform gridRT = gridTr.GetComponent<RectTransform>();
        gridRT.sizeDelta = new Vector2(0, 116);

        LayoutElement gridLE = gridTr.GetComponent<LayoutElement>();
        if (gridLE == null) gridLE = gridTr.gameObject.AddComponent<LayoutElement>();
        gridLE.minHeight = 116;
        gridLE.preferredHeight = 116;

        // Remove layouts antigos
        VerticalLayoutGroup oldVlg = gridTr.GetComponent<VerticalLayoutGroup>();
        if (oldVlg != null) Object.DestroyImmediate(oldVlg);
        HorizontalLayoutGroup oldHlg = gridTr.GetComponent<HorizontalLayoutGroup>();
        if (oldHlg != null) Object.DestroyImmediate(oldHlg);

        GridLayoutGroup glg = gridTr.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = gridTr.gameObject.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(52f, 52f);
        glg.spacing         = new Vector2(4f, 4f);
        glg.childAlignment  = TextAnchor.MiddleCenter;
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 13;   // 13 colunas → A-M linha 1, N-Z linha 2
        glg.padding         = new RectOffset(4, 4, 4, 4);
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;

        ContentSizeFitter csf = gridTr.GetComponent<ContentSizeFitter>();
        if (csf != null) Object.DestroyImmediate(csf);   // tamanho fixo via LE

        // Garante que o painel começa desativado
        selectorTr.gameObject.SetActive(false);

        // Adiciona / garante LetterSelectorUI no painel
        LetterSelectorUI lsUI = selectorTr.GetComponent<LetterSelectorUI>();
        if (lsUI == null) lsUI = selectorTr.gameObject.AddComponent<LetterSelectorUI>();
        lsUI.panelRoot        = selectorTr.gameObject;
        lsUI.buttonContainer  = gridTr;
        // letterButtonPrefab precisa ser wireado manualmente ou via código abaixo:
        GameObject btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LetterButton.prefab");
        if (btnPrefab != null) lsUI.letterButtonPrefab = btnPrefab;
        else Debug.LogWarning("[SetupMuralUI] LetterButton.prefab não encontrado em Assets/Prefabs/UI/");
        EditorUtility.SetDirty(lsUI);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // INVENTORY PANEL — backpack com grid de itens
    // ──────────────────────────────────────────────────────────────────────────
    static void SetupInventoryPanel(GameObject canvasGO)
    {
        // O InventoryUI script está no canvasGO; o painel visual pode ser filho
        Transform invUITr = canvasGO.transform.Find("InventoryUI");
        if (invUITr == null)
        {
            Debug.LogWarning("[SetupMuralUI] InventoryUI não encontrado como filho de UI_Canvas_Mural.");
            return;
        }

        // Procura o painel raiz dentro do InventoryUI
        Transform panelTr = invUITr.Find("InventoryPanel");
        if (panelTr == null)
        {
            // Se não encontrou filho "InventoryPanel", o próprio InventoryUI pode ser o painel
            panelTr = invUITr;
        }

        RectTransform panelRT = panelTr.GetComponent<RectTransform>();
        // Painel centralizado — não full-screen
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(520f, 420f);

        Image bgImg = panelTr.GetComponent<Image>();
        if (bgImg == null) bgImg = panelTr.gameObject.AddComponent<Image>();
        bgImg.sprite = null;
        bgImg.color = new Color(0.08f, 0.06f, 0.04f, 0.95f);

        VerticalLayoutGroup vlg = panelTr.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = panelTr.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 16, 16);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Título
        Transform titleTr = panelTr.Find("TitleInventory");
        if (titleTr == null)
        {
            GameObject tGO = new GameObject("TitleInventory", typeof(RectTransform));
            tGO.transform.SetParent(panelTr, false);
            tGO.transform.SetAsFirstSibling();
            titleTr = tGO.transform;
        }
        RectTransform titleRT = titleTr.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(0, 44);
        TextMeshProUGUI titleTMP = titleTr.GetComponent<TextMeshProUGUI>();
        if (titleTMP == null) titleTMP = titleTr.gameObject.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "🎒  Mochila";
        titleTMP.fontSize = 24;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.85f, 0.3f);
        LayoutElement titleLE = titleTr.GetComponent<LayoutElement>();
        if (titleLE == null) titleLE = titleTr.gameObject.AddComponent<LayoutElement>();
        titleLE.minHeight = 44;
        titleLE.preferredHeight = 44;

        // SlotsContainer — grid 4 colunas
        Transform slotsTr = panelTr.Find("SlotsContainer");
        if (slotsTr == null) slotsTr = invUITr.Find("SlotsContainer");
        if (slotsTr != null)
        {
            RectTransform slotsRT = slotsTr.GetComponent<RectTransform>();
            slotsRT.sizeDelta = new Vector2(0, 300);

            LayoutElement slotsLE = slotsTr.GetComponent<LayoutElement>();
            if (slotsLE == null) slotsLE = slotsTr.gameObject.AddComponent<LayoutElement>();
            slotsLE.minHeight = 300;
            slotsLE.preferredHeight = 300;
            slotsLE.flexibleHeight = 1;

            // Remove layout antigo
            HorizontalLayoutGroup oldHlg = slotsTr.GetComponent<HorizontalLayoutGroup>();
            if (oldHlg != null) Object.DestroyImmediate(oldHlg);
            VerticalLayoutGroup oldVlg = slotsTr.GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) Object.DestroyImmediate(oldVlg);

            GridLayoutGroup glg = slotsTr.GetComponent<GridLayoutGroup>();
            if (glg == null) glg = slotsTr.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize       = new Vector2(105f, 105f);
            glg.spacing        = new Vector2(10f, 10f);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint     = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;
            glg.padding        = new RectOffset(8, 8, 8, 8);
        }

        // Dica inferior
        Transform hintTr = panelTr.Find("HintClose");
        if (hintTr == null)
        {
            GameObject hGO = new GameObject("HintClose", typeof(RectTransform));
            hGO.transform.SetParent(panelTr, false);
            hintTr = hGO.transform;
        }
        RectTransform hintRT = hintTr.GetComponent<RectTransform>();
        hintRT.sizeDelta = new Vector2(0, 26);
        TextMeshProUGUI hintTMP = hintTr.GetComponent<TextMeshProUGUI>();
        if (hintTMP == null) hintTMP = hintTr.gameObject.AddComponent<TextMeshProUGUI>();
        hintTMP.text = "Pressione  I  para fechar";
        hintTMP.fontSize = 14;
        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        LayoutElement hintLE = hintTr.GetComponent<LayoutElement>();
        if (hintLE == null) hintLE = hintTr.gameObject.AddComponent<LayoutElement>();
        hintLE.minHeight = 26;
        hintLE.preferredHeight = 26;

        // ── Wiring do InventoryUI script ──────────────────────────────────────
        // O script DEVE ficar no canvasGO (sempre ativo), não no invUITr.
        // Se por engano estava no invUITr, remove para evitar auto-desativação.
        InventoryUI wrongScript = invUITr.GetComponent<InventoryUI>();
        if (wrongScript != null)
        {
            Object.DestroyImmediate(wrongScript);
            Debug.Log("[SetupMuralUI] InventoryUI removido do invUITr (estava causando auto-desativação).");
        }

        InventoryUI invScript = canvasGO.GetComponent<InventoryUI>();
        if (invScript == null)
        {
            invScript = canvasGO.AddComponent<InventoryUI>();
            Debug.Log("[SetupMuralUI] InventoryUI adicionado ao UI_Canvas_Mural (canvas sempre ativo).");
        }

        // inventoryPanel = invUITr (o painel visual filho — pode ser ativado/desativado com segurança)
        invScript.inventoryPanel = invUITr.gameObject;

        // slotsContainer = filho SlotsContainer
        Transform finalSlotsTr = panelTr.Find("SlotsContainer");
        if (finalSlotsTr == null) finalSlotsTr = invUITr.Find("SlotsContainer");
        if (finalSlotsTr != null)
            invScript.slotsContainer = finalSlotsTr;
        else
            Debug.LogWarning("[SetupMuralUI] SlotsContainer não encontrado sob InventoryUI.");

        // inventorySlotPrefab = carrega do projeto
        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/UI/InventorySlot.prefab");
        if (slotPrefab != null)
            invScript.inventorySlotPrefab = slotPrefab;
        else
            Debug.LogWarning("[SetupMuralUI] InventorySlot.prefab não encontrado em Assets/Prefabs/UI/");

        EditorUtility.SetDirty(invScript);
        Debug.Log("[SetupMuralUI] InventoryUI wireado: panel=InventoryUI, slots=SlotsContainer, prefab=InventorySlot.");

        Debug.Log("[SetupMuralUI] InventoryPanel configurado como backpack.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // WIRING — busca MuralUI em toda a cena e wirea painelMiniInventario/letterSelector
    // (os MuralUI ficam nos objetos 3D Mural_A/Mural_B, não no canvas)
    // ──────────────────────────────────────────────────────────────────────────
    static void WireMuralUIComponents(GameObject canvasGO)
    {
        MuralUI[] allMuralUIs = Object.FindObjectsByType<MuralUI>(FindObjectsSortMode.None);
        Debug.Log($"[SetupMuralUI] WireMuralUIComponents: {allMuralUIs.Length} MuralUI encontrado(s) na cena.");

        var pairs = new (string panelName, string suffix)[]
        {
            ("MuralPanel_A", "A"),
            ("MuralPanel_B", "B"),
        };

        foreach (var (panelName, suffix) in pairs)
        {
            Transform muralPanelTr = canvasGO.transform.Find(panelName);
            if (muralPanelTr == null)
            {
                Debug.LogWarning($"[SetupMuralUI] {panelName} não encontrado no canvas.");
                continue;
            }

            // 1ª tentativa: MuralUI cujo muralPanel aponta para este painel
            MuralUI target = null;
            foreach (var m in allMuralUIs)
            {
                if (m.muralPanel == muralPanelTr.gameObject) { target = m; break; }
            }

            // 2ª tentativa: objeto chamado "Mural_A" ou "Mural_B"
            if (target == null)
            {
                GameObject go = GameObject.Find("Mural_" + suffix);
                if (go != null)
                {
                    target = go.GetComponent<MuralUI>();
                    if (target == null) target = go.GetComponentInChildren<MuralUI>();
                }
                Debug.Log($"[SetupMuralUI] Fallback Find('Mural_{suffix}'): {(go != null ? go.name : "null")}, MuralUI: {(target != null ? "encontrado" : "null")}");
            }

            // 3ª tentativa: qualquer MuralUI da cena cujo nome contém o sufixo
            if (target == null)
            {
                foreach (var m in allMuralUIs)
                {
                    if (m.gameObject.name.Contains(suffix)) { target = m; break; }
                }
            }

            if (target == null)
            {
                Debug.LogWarning($"[SetupMuralUI] ⚠ Nenhum MuralUI encontrado para {panelName}. Wire manualmente no Inspector.");
                continue;
            }

            Debug.Log($"[SetupMuralUI] MuralUI({suffix}) encontrado em: {target.gameObject.name}");

            // Wire painelMiniInventario
            string invName = "PainelInventario_" + suffix;
            Transform invTr = muralPanelTr.Find(invName);
            if (invTr != null)
            {
                target.painelMiniInventario = invTr.gameObject;
                Debug.Log($"[SetupMuralUI]   painelMiniInventario → {invName} ✓");
            }
            else
                Debug.LogWarning($"[SetupMuralUI]   ⚠ {invName} não encontrado em {panelName}.");

            // Wire letterSelector
            string selectorName = "LetterSelectorPanel_" + suffix;
            Transform selTr = muralPanelTr.Find(selectorName);
            if (selTr == null) selTr = muralPanelTr.Find("LetterSelectorPanel");
            if (selTr != null)
            {
                LetterSelectorUI lsUI = selTr.GetComponent<LetterSelectorUI>();
                if (lsUI == null) lsUI = selTr.gameObject.AddComponent<LetterSelectorUI>();

                // Garante que o LetterSelectorUI está com os campos internos wireados
                lsUI.panelRoot       = selTr.gameObject;
                lsUI.buttonContainer = selTr.Find("LetterGrid");
                GameObject btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LetterButton.prefab");
                if (btnPrefab != null) lsUI.letterButtonPrefab = btnPrefab;
                EditorUtility.SetDirty(lsUI);

                target.letterSelector = lsUI;
                Debug.Log($"[SetupMuralUI]   letterSelector → {selTr.name} ✓");
            }
            else
                Debug.LogWarning($"[SetupMuralUI]   ⚠ {selectorName} não encontrado em {panelName}.");

            EditorUtility.SetDirty(target);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────────────────────────────────
    static void SetStretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }
}
#endif
