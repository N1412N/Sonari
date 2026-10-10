using UnityEngine;

[System.Serializable]
public class QuestData
{
    [Header("Quest Info")]
    public string questTitle;
    [TextArea] public string questDescription;

    [Header("Rewards")]
    public int rewardXP;
    public int rewardEchoes;
    public int rewardSigns;

    [Header("State")]
    public bool isActive;     // visible in quest menu
    public bool isCompleted;  // finished quest
}

