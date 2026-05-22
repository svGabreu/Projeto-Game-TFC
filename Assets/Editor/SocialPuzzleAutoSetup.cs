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

            if (peca.slotButton == null)
            {
                foreach (Transform child in peca.transform.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "SlotButton" && child.GetComponent<Button>() != null)
                    {
                        peca.slotButton = child.GetComponent<Button>();
                        changes++;
                        break;
                    }
                }
            }

            if (peca.slotLabel == null)
            {
                foreach (Transform child in peca.transform.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "SlotLabel" && child.GetComponent<TextMeshProUGUI>() != null)
                    {
                        peca.slotLabel = child.GetComponent<TextMeshProUGUI>();
                        changes++;
                        break;
                    }
                }
            }

            if (peca.slotBackground == null)
            {
                foreach (Transform child in peca.transform.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "SlotBackground" && child.GetComponent<Image>() != null)
                    {
                        peca.slotBackground = child.GetComponent<Image>();
                        changes++;
                        break;
                    }
                }
            }

            if (peca.characterImage == null)
            {
                foreach (Transform child in peca.transform.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "CharacterImage" && child.GetComponent<Image>() != null)
                    {
                        peca.characterImage = child.GetComponent<Image>();
                        changes++;
                        break;
                    }
                }
            }

            EditorUtility.SetDirty(peca);
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

            // pecas[]
            if (pecas.Length == 5)
            {
                var ordered = new SocialPecaUI[5];
                foreach (var p in pecas)
                {
                    if (p.expectedNameItemID == "name_farao")           ordered[0] = p;
                    else if (p.expectedNameItemID == "name_sacerdotes") ordered[1] = p;
                    else if (p.expectedNameItemID == "name_escribas")   ordered[2] = p;
                    else if (p.expectedNameItemID == "name_artesaos")   ordered[3] = p;
                    else if (p.expectedNameItemID == "name_camponeses") ordered[4] = p;
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

        // ── 5. Salva a cena ──
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
}
#endif
