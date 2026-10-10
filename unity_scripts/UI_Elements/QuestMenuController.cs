using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanel;       // The retractable panel
    [SerializeField] private TextMeshProUGUI questText;   // Text area listing quests

    private bool isOpen = false;

    private void Start()
    {
        if (questPanel != null)
        {
            isOpen = questPanel.activeSelf;
        }
        RefreshQuests();
    }

    public void ToggleQuestMenu()
    {
        isOpen = !isOpen;
        questPanel.SetActive(isOpen);

        if (isOpen) RefreshQuests();
    }

    public void RefreshQuests()
    {
        if (questText == null || GameManager.Instance == null || GameManager.Instance.data == null)
            return;

        questText.text = "";

        List<QuestData> quests = GameManager.Instance.data.quests;

        bool hasActiveQuest = false;

        for (int i = 0; i < quests.Count; i++)
        {
            QuestData q = quests[i];
            if (q.isActive && !q.isCompleted)
            {
                questText.text += $"<color=#00FF00><b>{q.questTitle}</b></color>\n";
                questText.text += $"<color=#CCCCCC>{q.questDescription}</color>\n\n";
                hasActiveQuest = true;
            }
        }

        if (!hasActiveQuest && quests.Count > 0)
        {
            questText.text = "<color=#00FF00><b>All quests completed!</b></color>";
        }
    }

    public void CompleteQuest(int questIndex)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteQuest(questIndex);
        }
    }
}
