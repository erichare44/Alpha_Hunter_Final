using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class MonsterDialogue
{
    [Header("Contract Types")]
    public string monsterGroupName;

    [Header("NPC Dialogue")]
    [TextArea]
    public string[] specialDialogue;

    [Header("Contract Choices")]
    public DialogueChoices[] specialChoices;
}
    
public class NPC : MonoBehaviour, IInteractable
{
    [Header("NPC Info")]
    public string npcName = "ENTER NAME";

    [Header("Default Dialogue")]
    [TextArea]
    public string[] dialogueLines;

    [Header("Player Choices")]
    public DialogueChoices[] choices;

    [Header("Special Dialogue")]
    public MonsterDialogue[] specificDialogue;

    public void Interact()
    {
        Debug.Log("Talking to: " + npcName);

        string[] linesToUse = dialogueLines;
        DialogueChoices[] choicesToUse = choices;
        gameManager.instance.talkedNPCCount++;
        if (gameManager.instance != null && gameManager.instance.selectedMonster != null)
        {
            string currentMonster = gameManager.instance.selectedMonster.groupName;

            foreach (MonsterDialogue entry in specificDialogue)
            {
                if (entry.monsterGroupName == currentMonster)
                {
                    linesToUse = entry.specialDialogue;
                    choicesToUse = entry.specialChoices;
                    break;
                }
            }
        }

        DialogueManager.Instance.StartDialogue(
            npcName,
            linesToUse,
            choicesToUse
        );
    }
}
