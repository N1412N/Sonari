using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;   // Singleton reference
    public GameData data;                  // Holds quests, xp, echoes, etc.

    void Awake()
    {
        // Singleton pattern: only one GameManager survives
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene loads
        }
        else
        {
            Destroy(gameObject);           // Kill duplicates
            return;
        }

        // Initialize game data
        data = new GameData();

        // Add some starter quests
        data.quests.Add(new QuestData {
            questTitle = "Part 1: Into the Unknown",
            questDescription = "Talk to the knight.",
            rewardXP = 100,
            rewardEchoes = 80,
            rewardSigns = 1,
            isActive = true,
            isCompleted = false
        });

        data.quests.Add(new QuestData {
            questTitle = "Part 2: Into the Unknown",
            questDescription = "Follow Sir Lancelot's pointed direction.",
            rewardXP = 200,
            rewardEchoes = 150,
            rewardSigns = 1,
            isActive = false,
            isCompleted = false
        });

        data.quests.Add(new QuestData {
            questTitle = "Part 3: ...Answers?",
            questDescription = "Travel to the Ancient Ruins to find the answer.",
            rewardXP = 300,
            rewardEchoes = 200,
            rewardSigns = 1,
            isActive = false,
            isCompleted = false
        });
    }

    public void CompleteQuest(int questIndex)
    {
        Debug.Log($"[GameManager] CompleteQuest called for index: {questIndex}");
        if (data == null || data.quests == null) 
        {
            Debug.LogError("[GameManager] data or quests list is null!");
            return;
        }
        if (questIndex < 0 || questIndex >= data.quests.Count)
        {
            Debug.LogError($"[GameManager] Quest index {questIndex} is out of bounds (Count: {data.quests.Count})");
            return;
        }

        QuestData q = data.quests[questIndex];
        Debug.Log($"[GameManager] Target quest: {q.questTitle}, Current isCompleted: {q.isCompleted}, rewardEchoes: {q.rewardEchoes}");
        if (q.isCompleted) return;

        q.isCompleted = true;
        data.userXP += q.rewardXP;
        data.echoes += q.rewardEchoes;
        data.signsLearned += q.rewardSigns;
        Debug.Log($"[GameManager] Quest completed! Added {q.rewardEchoes} echoes. New total echoes: {data.echoes}");

        if (questIndex + 1 < data.quests.Count)
        {
            data.quests[questIndex + 1].isActive = true;
            Debug.Log($"[GameManager] Next quest '{data.quests[questIndex + 1].questTitle}' set to Active.");
        }

        // Refresh UI if it is active in the scene
        QuestMenuController questMenu = Object.FindAnyObjectByType<QuestMenuController>();
        if (questMenu != null)
        {
            questMenu.RefreshQuests();
        }
    }

    private float nextSearchTime = 0f;
    void Update()
    {
        if (Time.time > nextSearchTime)
        {
            nextSearchTime = Time.time + 1f;
            SignDictionary dict = Object.FindAnyObjectByType<SignDictionary>();
            if (dict != null)
            {
                Debug.Log($"[GameManager] Found active SignDictionary in scene! GameObject: {dict.gameObject.name}, Enabled: {dict.enabled}");
            }
        }
    }
}
