// SocialPuzzleAutoSetup.cs
// Menu: Tools > Social Puzzle > Auto Setup Scene
// Atribui automaticamente todas as referências dos componentes do Puzzle 3.

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class SocialPuzzleAutoSetup
{
    [MenuItem("Tools/Social Puzzle/Auto Setup Scene")]
    public static void AutoSetup()
    {
        int changes = 0;

        // ── Usa Resources.FindObjectsOfTypeAll para incluir objetos em painéis desativados ──
        // (mais confiável que FindObjectsByType com FindObjectsInactive.Include)

        // ── 1. PiramideNivelUI: atribui Button, Image e cria TMP Text ──
        var niveis = Resources.FindObjectsOfTypeAll<PiramideNivelUI>()
            .Where(x => x.gameObject.scene.IsValid())   // apenas objetos da cena (não assets)
            .ToArray();
        Debug.Log($"[AutoSetup] PiramideNivelUI encontrados: {niveis.Length}");

        foreach (var nivel in niveis)
        {
            Undo.RecordObject(nivel, "Setup PiramideNivelUI");

            if (nivel.nivelButton == null)
            {
                nivel.nivelButton = nivel.GetComponent<Button>();
                if (nivel.nivelButton != null) changes++;
            }

            if (nivel.nivelBackground == null)
            {
                nivel.nivelBackground = nivel.GetComponent<Image>();
                if (nivel.nivelBackground != null) changes++;
            }

            if (nivel.nivelText == null)
            {
                // Procura TMP filho existente
                var tmp = nivel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp == null)
                {
                    // Cria TMP filho
                    var go = new GameObject("NivelLabel");
                    go.transform.SetParent(nivel.transform, false);

                    var rt = go.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    tmp = go.AddComponent<TextMeshProUGUI>();
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.fontSize = 14;
                    tmp.color = Color.white;
                    tmp.raycastTarget = false;

                    Undo.RegisterCreatedObjectUndo(go, "Create NivelLabel");
                }
                nivel.nivelText = tmp;
                changes++;
            }

            EditorUtility.SetDirty(nivel);
        }

        // ── 2. SocialPecaUI: atribui campos visuais se estiverem vazios ──
        var pecas = Resources.FindObjectsOfTypeAll<SocialPecaUI>()
            .Where(x => x.gameObject.scene.IsValid())
            .ToArray();
        Debug.Log($"[AutoSetup] SocialPecaUI encontrados: {pecas.Length}");

        foreach (var peca in pecas)
        {
            Undo.RecordObject(peca, "Setup SocialPecaUI");

            // slotButton — procura por nome (contem "Slot") ou qualquer Button nos filhos
            if (peca.slotButton == null)
            {
                Button found = null;
                // Prioridade: filho com "Slot" no nome
                foreach (Transform t in peca.GetComponentsInChildren<Transform>(true))
                    if (t.name.Contains("Slot") && t.TryGetComponent<Button>(out var b)) { found = b; break; }
                // Fallback: qualquer Button nos filhos
                if (found == null) found = peca.GetComponentInChildren<Button>(true);
                if (found != null) { peca.slotButton = found; changes++; }
            }

            // slotLabel — procura por nome ou qualquer TMP
            if (peca.slotLabel == null)
            {
                TextMeshProUGUI found = null;
                foreach (Transform t in peca.GetComponentsInChildren<Transform>(true))
                    if ((t.name.Contains("Label") || t.name.Contains("Text")) && t.TryGetComponent<TextMeshProUGUI>(out var tmp)) { found = tmp; break; }
                if (found == null) found = peca.GetComponentInChildren<TextMeshProUGUI>(true);
                if (found != null) { peca.slotLabel = found; changes++; }
            }

            // slotBackground — filho Image com "Background" no nome, ou Image do slotButton
            if (peca.slotBackground == null)
            {
                Image found = null;
                foreach (Transform t in peca.GetComponentsInChildren<Transform>(true))
                    if (t.name.Contains("Background") && t.TryGetComponent<Image>(out var img)) { found = img; break; }
                // Fallback: Image do próprio slotButton
                if (found == null && peca.slotButton != null)
                    found = peca.slotButton.GetComponent<Image>();
                if (found != null) { peca.slotBackground = found; changes++; }
            }

            // characterImage — filho Image com "Character" no nome, ou Image do objeto raiz
            if (peca.characterImage == null)
            {
                Image found = null;
                foreach (Transform t in peca.GetComponentsInChildren<Transform>(true))
                    if ((t.name.Contains("Character") || t.name.Contains("Icon") || t.name.Contains("Sprite"))
                        && t.TryGetComponent<Image>(out var img)) { found = img; break; }
                // Fallback: Image no próprio GO da peça
                if (found == null) found = peca.GetComponent<Image>();
                if (found != null) { peca.characterImage = found; changes++; }
            }

            // Garante raycastTarget correto
            if (peca.slotButton != null && peca.slotButton.targetGraphic != null)
                peca.slotButton.targetGraphic.raycastTarget = true;
            if (peca.slotBackground != null) peca.slotBackground.raycastTarget = false;
            if (peca.characterImage  != null) peca.characterImage.raycastTarget  = false;

            EditorUtility.SetDirty(peca);
        }

        // ── 2b. Reordena PecasContainer para ordem da Mesa: Faraó, Camponeses, Escribas, Sacerdotes, Artesões ──
        {
            // Ordem visual desejada (por expectedNameItemID)
            var mesaOrder = new[] { "name_farao", "name_camponeses", "name_escribas", "name_sacerdotes", "name_artesaos" };

            // Encontra o PecasContainer (pai das peças)
            Transform pecasContainer = null;
            if (pecas.Length > 0 && pecas[0] != null)
                pecasContainer = pecas[0].transform.parent;

            if (pecasContainer != null)
            {
                bool reordered = false;
                for (int i = 0; i < mesaOrder.Length; i++)
                {
                    foreach (var p in pecas)
                    {
                        if (p.expectedNameItemID == mesaOrder[i] && p.transform.parent == pecasContainer)
                        {
                            Undo.RecordObject(pecasContainer, "Reorder PecasContainer");
                            p.transform.SetSiblingIndex(i);
                            reordered = true;
                            break;
                        }
                    }
                }
                if (reordered)
                {
                    EditorUtility.SetDirty(pecasContainer.gameObject);
                    changes++;
                    Debug.Log("[AutoSetup] PecasContainer reordenado: Faraó, Camponeses, Escribas, Sacerdotes, Artesões");
                }
            }
        }

        // ── 3. InteractionPrompt: reposiciona para baixo da tela ──
        var allTmps = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
            .Where(x => x.gameObject.scene.IsValid())
            .ToArray();
        foreach (var tmp in allTmps)
        {
            if (tmp.gameObject.name == "InteractionPrompt")
            {
                var rt = tmp.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Undo.RecordObject(rt, "Reposition InteractionPrompt");
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot     = new Vector2(0.5f, 0f);
                    rt.anchoredPosition = new Vector2(0f, 60f);
                    rt.sizeDelta        = new Vector2(500f, 60f);
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.fontSize  = 24;
                    EditorUtility.SetDirty(rt);
                    changes++;
                }
            }
        }

        // ── 4. SocialPuzzle: preenche arrays pecas[] e estatuas[] ──
        var puzzleArr = Resources.FindObjectsOfTypeAll<SocialPuzzle>()
            .Where(x => x.gameObject.scene.IsValid())
            .ToArray();
        var puzzle = puzzleArr.Length > 0 ? puzzleArr[0] : null;

        if (puzzle != null)
        {
            Undo.RecordObject(puzzle, "Setup SocialPuzzle arrays");

            // pecas[] — ordem da mesa: Faraó(0), Camponeses(1), Escribas(2), Sacerdotes(3), Artesões(4)
            if (pecas.Length == 5)
            {
                var mesaOrder = new[] { "name_farao", "name_camponeses", "name_escribas", "name_sacerdotes", "name_artesaos" };
                var ordered = new SocialPecaUI[5];
                foreach (var p in pecas)
                {
                    int idx = System.Array.IndexOf(mesaOrder, p.expectedNameItemID);
                    if (idx >= 0) ordered[idx] = p;
                }
                puzzle.pecas = ordered;
                changes++;
            }

            // estatuas[]
            var estatuas = Resources.FindObjectsOfTypeAll<SocialStatue>()
                .Where(x => x.gameObject.scene.IsValid())
                .ToArray();
            Debug.Log($"[AutoSetup] SocialStatue encontrados: {estatuas.Length}");
            if (estatuas.Length == 5)
            {
                var orderedEst = new SocialStatue[5];
                foreach (var e in estatuas)
                {
                    if (e.pieceItem == null) continue;
                    if (e.pieceItem.itemID == "piece_farao")               orderedEst[0] = e;
                    else if (e.pieceItem.itemID == "piece_sacerdotes")     orderedEst[1] = e;
                    else if (e.pieceItem.itemID == "piece_escribas")       orderedEst[2] = e;
                    else if (e.pieceItem.itemID == "piece_artesaos")       orderedEst[3] = e;
                    else if (e.pieceItem.itemID == "piece_camponeses")     orderedEst[4] = e;
                }
                puzzle.estatuas = orderedEst;
                changes++;
            }

            // niveis[]
            if (niveis.Length == 5)
            {
                var orderedNiv = new PiramideNivelUI[5];
                foreach (var n in niveis)
                {
                    int idx = n.nivelCorreto - 1;
                    if (idx >= 0 && idx < 5) orderedNiv[idx] = n;
                }
                puzzle.niveis = orderedNiv;
                changes++;
            }

            EditorUtility.SetDirty(puzzle);
        }

        // ── 5. SocialUI: configura miniInventoryContainer e miniInventoryContainer2 ──
        var socialUIArr = Resources.FindObjectsOfTypeAll<SocialUI>()
            .Where(x => x.gameObject.scene.IsValid())
            .ToArray();
        var socialUI = socialUIArr.Length > 0 ? socialUIArr[0] : null;

        if (socialUI != null)
        {
            Undo.RecordObject(socialUI, "Setup SocialUI containers");

            // miniInventoryContainer → Content dentro do Etapa1Panel > MiniInventario > Viewport
            if (socialUI.miniInventoryContainer == null && socialUI.etapa1Panel != null)
            {
                var content = FindContentInMiniInventario(socialUI.etapa1Panel);
                if (content != null) { socialUI.miniInventoryContainer = content; changes++; }
            }

            // miniInventoryContainer2 → Content dentro do Etapa2Panel > MiniInventario > Viewport
            // Cria o Content se não existir
            if (socialUI.etapa2Panel != null)
            {
                var content2 = FindOrCreateContentInMiniInventario(socialUI.etapa2Panel, ref changes);
                if (content2 != null && socialUI.miniInventoryContainer2 != content2)
                {
                    socialUI.miniInventoryContainer2 = content2;
                    changes++;
                }
            }

            EditorUtility.SetDirty(socialUI);
        }

        // ── 6. Salva a cena ──
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[SocialPuzzleAutoSetup] Concluído! {changes} referências configuradas.");
        EditorUtility.DisplayDialog("Auto Setup", $"Concluído!\n{changes} referências configuradas.", "OK");
    }
    // ──────────────────────────────────────────────────────────────────────────
    // MENU: Tools > Social Puzzle > Setup Pyramid Layout
    // Configura o layout visual da pirâmide (larguras crescentes, alinhamento)
    // ──────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Social Puzzle/Setup Pyramid Layout")]
    public static void SetupPyramidLayout()
    {
        // ── PiramideContainer: VerticalLayoutGroup ──
        var containerObj = Resources.FindObjectsOfTypeAll<VerticalLayoutGroup>()
            .Where(x => x.gameObject.name == "PiramideContainer" && x.gameObject.scene.IsValid())
            .FirstOrDefault();

        if (containerObj != null)
        {
            Undo.RecordObject(containerObj, "Setup Pyramid VLG");
            containerObj.childAlignment       = TextAnchor.UpperCenter;
            containerObj.reverseArrangement   = false;
            containerObj.childControlWidth    = false;
            containerObj.childControlHeight   = true;
            containerObj.childForceExpandWidth  = false;
            containerObj.childForceExpandHeight = false;
            containerObj.spacing = 6;
            EditorUtility.SetDirty(containerObj);
            Debug.Log("[PyramidLayout] PiramideContainer VLG configurado.");
        }
        else
        {
            Debug.LogError("[PyramidLayout] PiramideContainer não encontrado!");
        }

        // ── Niveis: larguras crescentes para formar pirâmide ──
        // Nivel1 (Faraó) = topo (menor); Nivel5 (Camponeses) = base (maior)
        var levelWidths  = new int[] { 110, 170, 230, 290, 350 };
        int levelHeight  = 44;

        for (int i = 1; i <= 5; i++)
        {
            var nivelObj = Resources.FindObjectsOfTypeAll<PiramideNivelUI>()
                .Where(x => x.nivelCorreto == i && x.gameObject.scene.IsValid())
                .FirstOrDefault();

            if (nivelObj == null) { Debug.LogWarning($"[PyramidLayout] Nivel {i} não encontrado!"); continue; }

            var go = nivelObj.gameObject;
            Undo.RecordObject(go, "Setup Nivel Width");

            // Adiciona ou obtém LayoutElement para definir largura preferida
            var le = go.GetComponent<LayoutElement>();
            if (le == null) { le = Undo.AddComponent<LayoutElement>(go); }

            le.preferredWidth  = levelWidths[i - 1];
            le.preferredHeight = levelHeight;
            EditorUtility.SetDirty(go);

            // Configura o texto label do nível (placeholder)
            var nivel = nivelObj;
            if (nivel.nivelText != null && nivel.nivelText.text == "")
                nivel.nivelText.text = nivel.placeholderText;

            Debug.Log($"[PyramidLayout] Nivel{i} → width={levelWidths[i-1]}, height={levelHeight}");
        }

        // ── PiramideContainer RectTransform: expande para conter os 5 níveis ──
        if (containerObj != null)
        {
            var rt = containerObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                Undo.RecordObject(rt, "Setup Container RT");
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(380f, 270f);  // largura = Nivel5 + margem; altura = 5×44 + 4×6
                rt.anchoredPosition = new Vector2(0f, -30f);
                EditorUtility.SetDirty(rt);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[PyramidLayout] Layout da pirâmide configurado!");
        EditorUtility.DisplayDialog("Pyramid Layout", "Layout da pirâmide configurado com sucesso!", "OK");
    }
    // ── Helpers para miniInventoryContainer ───────────────────────────────────

    /// <summary>
    /// Procura o Content do ScrollRect dentro de um painel.
    /// Usa GetComponentsInChildren para ser robusto a qualquer nível de hierarquia e nome.
    /// Retorna o Content ou null se não encontrado.
    /// </summary>
    private static Transform FindContentInMiniInventario(GameObject panel)
    {
        // Tenta primeiro o Content já referenciado pelo ScrollRect
        var scrollRects = panel.GetComponentsInChildren<ScrollRect>(true);
        if (scrollRects.Length > 0 && scrollRects[0].content != null)
            return scrollRects[0].content.transform;

        // Fallback: busca por nome na hierarquia completa
        return FindDescendantByName(panel.transform, "Content");
    }

    /// <summary>
    /// Procura ou cria o Content do ScrollRect dentro de um painel.
    /// Usa GetComponentsInChildren para ser robusto a qualquer nível de hierarquia e nome.
    /// Se o Content não existir, cria e configura como container horizontal.
    /// </summary>
    private static Transform FindOrCreateContentInMiniInventario(GameObject panel, ref int changes)
    {
        // Busca o ScrollRect em qualquer nível da hierarquia (ignora nome do objeto)
        var scrollRects = panel.GetComponentsInChildren<ScrollRect>(true);
        if (scrollRects.Length == 0)
        {
            Debug.LogWarning($"[AutoSetup] Nenhum ScrollRect encontrado em {panel.name}");
            return null;
        }

        var scrollRect = scrollRects[0];
        var miniInv    = scrollRect.transform;

        // Se o ScrollRect já tem Content configurado, usa direto
        if (scrollRect.content != null)
        {
            Debug.Log($"[AutoSetup] ScrollRect em '{miniInv.name}' já tem Content: {scrollRect.content.name}");
            return scrollRect.content.transform;
        }

        // Busca o Viewport (referenciado ou pelo nome)
        Transform viewport = scrollRect.viewport != null
            ? scrollRect.viewport.transform
            : FindDescendantByName(miniInv, "Viewport");

        if (viewport == null)
        {
            // Cria um Viewport se não existir
            var vpGO = new GameObject("Viewport");
            Undo.RegisterCreatedObjectUndo(vpGO, "Create Viewport");
            vpGO.transform.SetParent(miniInv, false);
            var vpRT = vpGO.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            vpGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            vpGO.AddComponent<Mask>().showMaskGraphic = false;
            viewport = vpGO.transform;
            changes++;
            Debug.Log($"[AutoSetup] Viewport criado em {panel.name}/{miniInv.name}");
        }

        // Verifica se o Content já existe como filho do Viewport
        var existingContent = FindDescendantByName(viewport, "Content");
        if (existingContent != null)
        {
            // Garante que o ScrollRect aponta para ele
            Undo.RecordObject(scrollRect, "Set ScrollRect Content");
            scrollRect.content  = existingContent.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = true;
            scrollRect.vertical   = false;
            EditorUtility.SetDirty(scrollRect);
            return existingContent;
        }

        // Cria o Content
        var contentGO = new GameObject("Content");
        Undo.RegisterCreatedObjectUndo(contentGO, "Create MiniInventario Content");
        contentGO.transform.SetParent(viewport, false);

        var rt = contentGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot     = new Vector2(0f, 0.5f);

        // HorizontalLayoutGroup para empilhar itens horizontalmente
        var hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.spacing                = 8f;
        hlg.childControlWidth      = false;
        hlg.childControlHeight     = false;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.padding                = new RectOffset(4, 4, 2, 2);

        // ContentSizeFitter para o Content crescer com os itens
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;

        // Configura o ScrollRect
        Undo.RecordObject(scrollRect, "Set ScrollRect Content");
        scrollRect.content  = rt;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = true;
        scrollRect.vertical   = false;
        EditorUtility.SetDirty(scrollRect);

        EditorUtility.SetDirty(contentGO);
        changes++;
        Debug.Log($"[AutoSetup] Content criado em {panel.name}/{miniInv.name}/Viewport");
        return contentGO.transform;
    }

    /// <summary>
    /// Busca um descendente por nome em qualquer nível da hierarquia (busca em profundidade).
    /// </summary>
    private static Transform FindDescendantByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDescendantByName(child, name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>Busca filho DIRETO por nome (apenas primeiro nível).</summary>
    private static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
            if (child.name == name) return child;
        return null;
    }
}
#endif
