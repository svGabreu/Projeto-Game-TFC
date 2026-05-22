// SocialPuzzleTestHelper.cs
// Assets/Editor/SocialPuzzleTestHelper.cs
// Menu: Tools/Social Puzzle > [opções de teste]
// Só funciona em Play Mode.

using UnityEngine;
using UnityEditor;
using System.Linq;

public static class SocialPuzzleTestHelper
{
    // ── Etapa 1: adiciona os 5 pergaminhos de nome ao inventário ──────────
    [MenuItem("Tools/Social Puzzle/1. Add Name Items to Inventory")]
    public static void AddNameItems()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Test] Precisa estar em Play Mode."); return; }

        var inv = InventoryManager.Instance;
        if (inv == null) { Debug.LogError("[Test] InventoryManager não encontrado!"); return; }

        string[] ids = { "name_farao", "name_sacerdotes", "name_artesaos", "name_camponeses", "name_escribas" };
        foreach (var id in ids)
        {
            var item = FindItem(id);
            if (item == null) { Debug.LogWarning($"[Test] Item não encontrado: {id}"); continue; }
            inv.AddItem(item);
            Debug.Log($"[Test] Adicionado: {item.displayName}");
        }
    }

    // ── Abre o painel Mesa (Etapa 1) ──────────────────────────────────────
    [MenuItem("Tools/Social Puzzle/2. Open Mesa Panel (Etapa 1)")]
    public static void OpenMesaPanel()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Test] Precisa estar em Play Mode."); return; }
        var ui = SocialUI.Instance;
        if (ui == null) { Debug.LogError("[Test] SocialUI não encontrado!"); return; }
        ui.OpenPanel(SocialUI.PanelMode.Mesa);
        Debug.Log("[Test] Painel Mesa aberto.");
    }

    // ── Etapa 2: adiciona os 5 itens de peça ao inventário ───────────────
    [MenuItem("Tools/Social Puzzle/3. Add Piece Items to Inventory")]
    public static void AddPieceItems()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Test] Precisa estar em Play Mode."); return; }

        var inv = InventoryManager.Instance;
        if (inv == null) { Debug.LogError("[Test] InventoryManager não encontrado!"); return; }

        string[] ids = { "piece_farao", "piece_sacerdotes", "piece_artesaos", "piece_camponeses", "piece_escribas" };
        foreach (var id in ids)
        {
            var item = FindItem(id);
            if (item == null) { Debug.LogWarning($"[Test] Item não encontrado: {id}"); continue; }
            inv.AddItem(item);
            Debug.Log($"[Test] Adicionado: {item.displayName}");
        }
    }

    // ── Abre o painel Mural (Etapa 2) ────────────────────────────────────
    [MenuItem("Tools/Social Puzzle/4. Open Mural Panel (Etapa 2)")]
    public static void OpenMuralPanel()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Test] Precisa estar em Play Mode."); return; }
        var ui = SocialUI.Instance;
        if (ui == null) { Debug.LogError("[Test] SocialUI não encontrado!"); return; }
        var puzzle = SocialPuzzle.Instance;
        if (puzzle != null && puzzle.Etapa < 2)
        {
            Debug.LogWarning("[Test] Puzzle ainda está na Etapa 1. Force a transição antes.");
            return;
        }
        ui.OpenPanel(SocialUI.PanelMode.Mural);
        Debug.Log("[Test] Painel Mural aberto.");
    }

    // ── Força transição para Etapa 2 (simula coleta de todas as estátuas) ─
    [MenuItem("Tools/Social Puzzle/UTIL - Force Etapa 2 (skip statues)")]
    public static void ForceEtapa2()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Test] Precisa estar em Play Mode."); return; }
        var puzzle = SocialPuzzle.Instance;
        if (puzzle == null) { Debug.LogError("[Test] SocialPuzzle não encontrado!"); return; }

        // Chama OnStatueCollected 5 vezes para simular coleta das estátuas
        int needed = puzzle.estatuas.Length - 0;
        for (int i = 0; i < needed; i++)
            puzzle.OnStatueCollected();

        Debug.Log("[Test] Etapa 2 forçada via coleta simulada de estátuas.");
    }

    // ── Mostra estado atual do puzzle ─────────────────────────────────────
    [MenuItem("Tools/Social Puzzle/UTIL - Show Puzzle State")]
    public static void ShowState()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Test] Precisa estar em Play Mode."); return; }
        var puzzle = SocialPuzzle.Instance;
        if (puzzle == null) { Debug.LogError("[Test] SocialPuzzle não encontrado!"); return; }

        Debug.Log($"[Test] Etapa={puzzle.Etapa}, AllCollected={puzzle.AllCollected}");

        for (int i = 0; i < puzzle.pecas.Length; i++)
            Debug.Log($"  peça[{i}] IsNamed={puzzle.pecas[i].IsNamed}");

        for (int i = 0; i < puzzle.niveis.Length; i++)
            Debug.Log($"  nivel[{i}] IsFilled={puzzle.niveis[i].IsFilled}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static GlyphItem FindItem(string itemID)
    {
        string[] guids = AssetDatabase.FindAssets($"t:GlyphItem");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var item = AssetDatabase.LoadAssetAtPath<GlyphItem>(path);
            if (item != null && item.itemID == itemID)
                return item;
        }
        return null;
    }
}
