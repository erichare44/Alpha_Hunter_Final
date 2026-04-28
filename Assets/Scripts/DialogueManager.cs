using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dialogueText;

    [Header("Choices")]
    [SerializeField] GameObject choicesContainer;
    [SerializeField] GameObject choiceButtonPrefab;

    [Header("Typing Effect")]
    [SerializeField] float typingSpeed;

    string[] currentLines;
    DialogueChoices[] currentChoices;

    int currentIndex;
    bool isTyping;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dialogueUI.activeSelf)
            return;

        if (currentLines == null || currentLines.Length == 0)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if(isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLines[currentIndex];
                isTyping = false;
                return;
            }

            NextLine();
        }
    }

    public void StartDialogue(string npcName, string[] lines, DialogueChoices[] choices)
    {
        
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines assigned for: " + npcName);
            return;
        }

        dialogueUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (gameManager.instance !=null)
        {
            gameManager.instance.StatePause();
        }

        choicesContainer.SetActive(false);

        nameText.text = npcName;

        currentLines = lines;
        currentChoices = choices;

        currentIndex = 0;

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(currentLines[currentIndex]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        if (currentLines == null || currentLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        currentIndex++;

        if (currentIndex >= currentLines.Length)
        {
            if (currentChoices != null && currentChoices.Length > 0)
            {
                ShowChoices(currentChoices);
                return;
            }

            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    void ShowChoices(DialogueChoices[] choices)
    {
        choicesContainer.SetActive(true);

        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (DialogueChoices choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);

            buttonObj.SetActive(true);

            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;

            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    choicesContainer.SetActive(false);

                    StartDialogue(nameText.text, choice.responseLines, null);
                } );
        }
    }

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        choicesContainer.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (gameManager.instance != null)
        {
            gameManager.instance.StateUnpause();
        }

        currentLines = null;
        currentChoices = null;
        currentIndex = 0;
        isTyping = false;
    }
}
