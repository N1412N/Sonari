using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class KnightInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public string npcName = "Sir Lancelot";
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;

    [Header("Sign Guide UI")]
    public Image signGuideImage;
    public Sprite signSprite;

    private Transform player;
    private bool hasTalked = false;

    // Lock and practice states
    private bool isLocked = false;
    private int aCount = 0;
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
        "User: \"Huh..? Where am I? This isn’t my hometown…\"",
        "User: \"Oh! A knight? Maybe he knows where I am.\"",
        "Sir Lancelot: \"...\"",
        "User: \"...is he using sign language?\"",
        "Sir Lancelot: *demonstrates sign A*"
    };

    private string[] dialogueAfter = {
        "Sir Lancelot: *Points toward the distant hill.*",
        "User: \"Is that…a house? Maybe someone there can speak! Let’s go!!\""
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
                        aCount = 0;
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
                // Check for gesture 'A' simulated input (which maps to 'P' key)
                if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
                {
                    aCount++;
                    ShowPracticeText();

                    if (aCount >= 5)
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

            if (dialogueText != null)
            {
                if (hasTalked)
                {
                    dialogueText.text = $"{npcName}: *Points toward the distant hill.*";
                }
                else
                {
                    dialogueText.text = $"Press E to talk to {npcName}";
                }
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !hasTalked)
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
        aCount = 0;
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
            dialogueText.text = $"<color=#00FF00><b>Tutorial: Learn Sign A</b></color>\n\nPractice the letter A. Perform 'A' 5 times!\nProgress: {aCount}/5\n";
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
            GameManager.Instance.CompleteQuest(0); // Completes Quest 0 (Part 1) and refreshes active UI
        }

        // Move arrow guide to next NPC
        NPCArrowGuide arrowGuide = UnityEngine.Object.FindAnyObjectByType<NPCArrowGuide>();
        if (arrowGuide != null)
            arrowGuide.NextNPC();
    }
}
