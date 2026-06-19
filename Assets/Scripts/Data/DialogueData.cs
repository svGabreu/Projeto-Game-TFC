using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(2, 4)]
    public string text;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
