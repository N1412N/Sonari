using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class witchInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public string npcName = "Morgana";
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    [Header("Sign Guide UI")]
    public Image signGuideImage;
    public Sprite signSprite;

    private Transform player;
    private bool hasTalked = false;

    // Lock and practice states
    private bool isLocked = false;
    private int bCount = 0;
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
        "User: \"...Hello?\"",
        "Morgana: \"Oh, visitor. I’m Morgana.\"",
        "User: \"...You can speak?\"",
        "Morgana: \"A little.\"",
        "User: \"Do you know where I am??\"",
        "Morgana: \"This world...Sonari. But you’re…an outsider. But I don’t have answers.\"",
        "User: \"So...\"",
        "User: \"I'm stuck?\"",
        "Morgana: \"For now.\"",
        "Morgana: (Silence.)",
        "User: \"The knight in the village…taught me sign language. Do you guys use that here?\"",
        "Morgana: \"Ah, learning signs is learning our language. To go back home, you must understand this world first.\"",
        "Morgana: \"This one...You have not learned yet.\""
    };

    private string[] dialogueAfter = {
        "Morgana: \"Beyond the woods. Ancient ruins may have answers…\"",
        "Morgana: \"Do not fear the people here. They simply...forgot how to speak.\"",
        "User: \"...Forgot?\"",
        "Morgana: \"...\""
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
                        bCount = 0;
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
                // Check for gesture 'B' simulated input (which maps to 'O' key)
                if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
                {
                    bCount++;
                    ShowPracticeText();

                    if (bCount >= 5)
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

            // Check if Morgana's quest (index 1) is active
            bool isQuestActive = false;
            if (GameManager.Instance != null && GameManager.Instance.data != null && GameManager.Instance.data.quests.Count > 1)
            {
                isQuestActive = GameManager.Instance.data.quests[1].isActive;
            }

            if (dialogueText != null)
            {
                if (hasTalked)
                {
                    dialogueText.text = $"{npcName}: \"Beyond the woods. Ancient ruins may have answers…\"";
                }
                else if (!isQuestActive)
                {
                    dialogueText.text = $"{npcName}: \"I am busy right now.\"\n<color=#FF8800>(Talk to Sir Lancelot first)</color>";
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
        bCount = 0;
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
            dialogueText.text = $"<color=#00FF00><b>Tutorial: Learn Sign B</b></color>\n\nPractice the letter B. Perform 'B' 5 times!\nProgress: {bCount}/5\n";
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
            GameManager.Instance.CompleteQuest(1); // Completes Quest 1 (Part 2) and refreshes active UI
        }

        NPCArrowGuide arrowGuide = UnityEngine.Object.FindAnyObjectByType<NPCArrowGuide>();
        if (arrowGuide != null)
            arrowGuide.NextNPC();
    }
}
