
// Singleton persistente entre cenas.
// Armazena estado de qualquer puzzle via chave-valor genérico.
// Cada puzzle usa um prefixo único (ex: "rdv.", "mural.") para suas chaves.

using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour 
{
    public static GameStateManager Instance { get; private set; }  // Singleton público para acesso global

    private readonly Dictionary<string, string> data = new Dictionary<string, string>(); // Armazena o estado do jogo

    private void Awake() // Executado quando o objeto é instanciado
    {
        Debug.Log($"[GSM] Awake — Instance={(!ReferenceEquals(Instance, null) ? Instance.GetInstanceID().ToString() : "null")} | Este={GetInstanceID()}");

        if (!ReferenceEquals(Instance, null) && Instance != this)
        {
            Debug.Log($"[GSM] Duplicata detectada — destruindo este ({GetInstanceID()})");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        Debug.Log($"[GSM] Inicializado e persistido. ID={GetInstanceID()}");
    }

    private void OnDestroy()// Executado quando o objeto é destruído
    {
        Debug.Log($"[GSM] *** DESTRUÍDO *** ID={GetInstanceID()} | Era o Instance={Instance == this} | StackTrace a seguir:");
        Debug.Log(System.Environment.StackTrace);
        if (Instance == this)
            Instance = null;
    }
     
    public void SetInt(string key, int value) => data[key] = value.ToString(); // Armazena o valor inteiro como string
    public void SetBool(string key, bool value) => data[key] = value ? "1" : "0"; // Armazena o valor booleano como "1" ou "0"
    public void SetString(string key, string value) => data[key] = value ?? ""; // Armazena o valor string, substituindo null por string vazia

    public int GetInt(string key, int defaultValue = 0) // Retorna o valor inteiro associado à chave, ou defaultValue se a chave não existir ou não for um inteiro
    {
        if (data.TryGetValue(key, out string v) && int.TryParse(v, out int r)) return r;
        return defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false) // Retorna o valor booleano associado à chave, ou defaultValue se a chave não existir ou não for um booleano
    {
        if (data.TryGetValue(key, out string v)) return v == "1";
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "") // Retorna o valor string associado à chave, ou defaultValue se a chave não existir
    {
        if (data.TryGetValue(key, out string v)) return v;
        return defaultValue;
    }

    public bool HasKey(string key) => data.ContainsKey(key); // Verifica se a chave existe no dicionário

    public void ClearPrefix(string prefix) // Remove todas as chaves que começam com o prefixo fornecido
    {
        var toRemove = new List<string>();
        foreach (var key in data.Keys)
            if (key.StartsWith(prefix)) toRemove.Add(key);
        foreach (var key in toRemove) data.Remove(key);
    }

    public void ClearAll() => data.Clear();
}