using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int userXP;
    public int echoes;
    public int signsLearned;
    public int questsCompleted;

    // Quest progression
    public List<QuestData> quests = new List<QuestData>();

    // Constructor sets defaults
    public GameData()
    {
        userXP = 0;
        echoes = 0;
        signsLearned = 0;
        questsCompleted = 0;
    }
}
