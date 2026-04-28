using UnityEngine;

[System.Serializable]
public class DialogueChoices
{
    [Header("Player Choice")]
    public string choiceText;

    [Header("NPC Response")]
    [TextArea]
    public string[] responseLines;
}
