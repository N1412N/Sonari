using UnityEngine;
using UnityEngine.InputSystem; // New Input System
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public string npcName = "Sir Lancelot Du Lac";
    public TextMeshProUGUI dialogueText; // Drag TMP text here
    public GameObject dialoguePanel;     // Drag Panel here

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false); // Hide at start
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            // Show panel only when close
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Interact();
            }
        }
        else
        {
            // Hide panel when far
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
    }

    void Interact()
    {
        if (dialogueText != null)
            dialogueText.text = $"You are talking to {npcName}";
    }
}
