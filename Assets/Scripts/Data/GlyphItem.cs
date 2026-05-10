using UnityEngine;

// ============================================================
// GlyphItem.cs
// ScriptableObject que representa um item coletável do mundo.
// Crie instâncias em: Assets > Create > NoEscape > GlyphItem
// ============================================================

[CreateAssetMenu(fileName = "NewGlyphItem", menuName = "NoEscape/GlyphItem")]
public class GlyphItem : ScriptableObject
{
    [Header("Identificação")]
    public string itemID;           // ex: "bird_symbol", "lion_sculpture"
    public string displayName;      // ex: "Símbolo do Pássaro"

    [Header("Visual")]
    public Sprite itemSprite;       // ícone que aparece no inventário

    [Header("Tipo")]
    public GlyphItemType itemType;  // define se vai para slot de silhueta

    [Header("Letra Correta")]
    // Letra que este símbolo representa — usada para validação do mural.
    // Deixe em branco se o item não for usado em slot de letra.
    public string correctLetter;    // ex: "A", "N", "I", "L", "O"
}

public enum GlyphItemType
{
    GlyphObject,    // objeto físico para slot de silhueta no mural
    KeyItem,        // chave, amuleto — não vai para o mural
    QuestItem       // item de missão geral
}
