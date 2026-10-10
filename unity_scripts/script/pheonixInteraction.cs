using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class pheonixInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public string npcName = "Phoenix";
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    [Header("Sign Guide UI")]
    public Image signGuideImage;
    public Sprite signSprite;

    private Transform player;
    private bool hasTalked = false;

    // Lock and practice states
    private bool isLocked = false;
    private int cCount = 0;
    private Vector3 lockedPosition;

    private enum InteractionState
    {
        NotStarted,
        DialogueBefore,
        Practicing,
        DialogueAfter,
        Finished
    }

    private InteractionState state = InteractionState.NotStarted;
    private int dialogueIndex = 0;

    private string[] dialogueBefore = {
        "Phoenix: \"A new face!\"",
        "User: \"?!...You can speak?\"",
        "Phoenix: \"Of course I can!\"",
        "User: \"I thought nobody here spoke English.\"",
        "Phoenix: \"Most don't.\"",
        "User: \"...Do you know how I can get home?\"",
        "Phoenix: \"I’m sorry, I don’t know how you can get home.\"",
        "Phoenix: \"But...You don't have to figure everything out today.\"",
        "User: \"I've…learnt a few sign languages. So far, there’s 2: A and B from a knight and a witch.\"",
        "Phoenix: \"Oh! That's fun.\"",
        "Phoenix: \"I know one you don't!\""
    };

    private string[] dialogueAfter = {
        "User: \"So...What now?\"",
        "Phoenix: \"Go back. Get some rest.\"",
        "User: \"...Thanks.\"",
        "Phoenix: \"And come visit me again! It gets pretty lonely out here.\""
    };

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (signGuideImage != null)
            signGuideImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // If locked during dialogue challenge
        if (isLocked)
        {
            // Lock position
            player.position = lockedPosition;

            // Stop any rigidbody movement
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (state == InteractionState.DialogueBefore)
            {
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    dialogueIndex++;
                    if (dialogueIndex >= dialogueBefore.Length)
                    {
                        state = InteractionState.Practicing;
                        cCount = 0;
                        ShowPracticeText();
                    }
                    else
                    {
                        ShowDialogueText(dialogueBefore[dialogueIndex]);
                    }
                }
            }
            else if (state == InteractionState.Practicing)
            {
                // Check for gesture 'C' simulated input (which maps to 'I' key)
                if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
                {
                    cCount++;
                    ShowPracticeText();

                    if (cCount >= 5)
                    {
                        state = InteractionState.DialogueAfter;
                        dialogueIndex = 0;
                        if (signGuideImage != null)
                            signGuideImage.gameObject.SetActive(false);
                        ShowDialogueText(dialogueAfter[dialogueIndex]);
                    }
                }
            }
            else if (state == InteractionState.DialogueAfter)
            {
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    dialogueIndex++;
                    if (dialogueIndex >= dialogueAfter.Length)
                    {
                        CompleteInteraction();
                    }
                    else
                    {
                        ShowDialogueText(dialogueAfter[dialogueIndex]);
                    }
                }
            }
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            // Check if Phoenix's quest (index 2) is active
            bool isQuestActive = false;
            if (GameManager.Instance != null && GameManager.Instance.data != null && GameManager.Instance.data.quests.Count > 2)
            {
                isQuestActive = GameManager.Instance.data.quests[2].isActive;
            }

            if (dialogueText != null)
            {
                if (hasTalked)
                {
                    dialogueText.text = $"{npcName}: \"And come visit me again! It gets pretty lonely out here.\"";
                }
                else if (!isQuestActive)
                {
                    dialogueText.text = $"{npcName}: \"Come back later.\"\n<color=#FF8800>(Talk to Morgana first)</color>";
                }
                else
                {
                    dialogueText.text = $"Press E to talk to {npcName}";
                }
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !hasTalked && isQuestActive)
            {
                StartInteraction();
            }
        }
        else
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (signGuideImage != null)
                signGuideImage.gameObject.SetActive(false);
        }
    }

    void StartInteraction()
    {
        isLocked = true;
        cCount = 0;
        lockedPosition = player.position;
        state = InteractionState.DialogueBefore;
        dialogueIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (signGuideImage != null)
            signGuideImage.gameObject.SetActive(false);

        ShowDialogueText(dialogueBefore[dialogueIndex]);
    }

    void ShowDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text + "\n\n<color=#FFFF00>[Press Space to continue]</color>";
        }
    }

    void ShowPracticeText()
    {
        if (signGuideImage != null && signSprite != null)
        {
            signGuideImage.sprite = signSprite;
            signGuideImage.gameObject.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.text = $"<color=#00FF00><b>Tutorial: Learn Sign C</b></color>\n\nPractice the letter C. Perform 'C' 5 times!\nProgress: {cCount}/5\n";
        }
    }

    void CompleteInteraction()
    {
        isLocked = false;
        hasTalked = true;
        state = InteractionState.Finished;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (signGuideImage != null)
            signGuideImage.gameObject.SetActive(false);

        // Progression reward logic via GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteQuest(2); // Completes Quest 2 (Part 3) and refreshes active UI
        }

        // Move arrow guide to next NPC
        NPCArrowGuide arrowGuide = UnityEngine.Object.FindAnyObjectByType<NPCArrowGuide>();
        if (arrowGuide != null)
            arrowGuide.NextNPC();
    }
}
