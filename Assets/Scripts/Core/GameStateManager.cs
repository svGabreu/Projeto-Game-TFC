// GameStateManager.cs
// Coloque em: Assets/Scripts/Core/
// Singleton persistente entre cenas.
// Armazena estado de qualquer puzzle via chave-valor genérico.
// Cada puzzle usa um prefixo único (ex: "rdv.", "mural.") para suas chaves.

using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // Dicionário central — persiste na memória enquanto o jogo roda
    private readonly Dictionary<string, string> data = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Escrita ──────────────────────────────────────────────
    public void SetInt   (string key, int    value) => data[key] = value.ToString();
    public void SetBool  (string key, bool   value) => data[key] = value ? "1" : "0";
    public void SetString(string key, string value) => data[key] = value ?? "";

    // ── Leitura ──────────────────────────────────────────────
    public int GetInt(string key, int defaultValue = 0)
    {
        if (data.TryGetValue(key, out string v) && int.TryParse(v, out int r)) return r;
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (data.TryGetValue(key, out string v)) return v == "1";
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        if (data.TryGetValue(key, out string v)) return v;
        return defaultValue;
    }

    public bool HasKey(string key) => data.ContainsKey(key);

    // ── Utilitários ──────────────────────────────────────────
    /// <summary>Apaga todas as chaves com o prefixo indicado (reseta um puzzle).</summary>
    public void ClearPrefix(string prefix)
    {
        var toRemove = new List<string>();
        foreach (var key in data.Keys)
            if (key.StartsWith(prefix)) toRemove.Add(key);
        foreach (var key in toRemove) data.Remove(key);
    }

    /// <summary>Apaga todo o estado (reinício do jogo).</summary>
    public void ClearAll() => data.Clear();
}
