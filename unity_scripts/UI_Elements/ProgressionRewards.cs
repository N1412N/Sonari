using UnityEngine;
using UnityEngine.UI;

public class ProgressionRewards : MonoBehaviour
{
    [System.Serializable]
    public class LevelReward
    {
        public int requiredSigns;       // how many signs needed to unlock
        public RawImage unlockIcon;     // fades when unlocked
        public RawImage levelIcon;      // becomes visible when unlocked
        public Button claimButton;      // becomes interactable when unlocked
        public RawImage claimedIcon;    // fades when claimed
        public int xpReward = 50;       // XP gained per level
        public int echoReward = 100;    // Echoes gained per level
        [HideInInspector] public bool claimed = false;
    }

    [Header("Levels")]
    public LevelReward[] levels;

    private int lastSignsLearned = -1;
    private int lastActiveLevelIndex = -1;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.data != null)
        {
            Debug.Log($"[ProgressionRewards] Start - signsLearned: {GameManager.Instance.data.signsLearned}");
        }
        else
        {
            Debug.LogError("[ProgressionRewards] Start - GameManager.Instance or GameManager.Instance.data is null!");
        }

        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning("[ProgressionRewards] No levels defined in the Levels array!");
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.data == null)
            return; // skip until GameManager is ready

        int signsLearned = GameManager.Instance.data.signsLearned;
        bool changed = (signsLearned != lastSignsLearned);
        if (changed)
        {
            lastSignsLearned = signsLearned;
            Debug.Log($"[ProgressionRewards] Update - signsLearned value changed to: {signsLearned}");
        }

        // Find the LevelsSwitch component in the scene to know which page is active
        LevelsSwitch levelsSwitch = Object.FindAnyObjectByType<LevelsSwitch>();
        int activeLevelIndex = (levelsSwitch != null) ? levelsSwitch.currentActiveLevel : 0;

        if (activeLevelIndex != lastActiveLevelIndex)
        {
            lastActiveLevelIndex = activeLevelIndex;
            Debug.Log($"[ProgressionRewards] Page switched to activeLevelIndex: {activeLevelIndex} (levelsSwitch found: {levelsSwitch != null})");
        }

        for (int i = 0; i < levels.Length; i++)
        {
            LevelReward level = levels[i];
            if (level == null) continue;

            bool isUnlocked = (signsLearned >= level.requiredSigns);

            // 1. Manage the navigation button (tab button) for this level:
            // Allow selecting/viewing a page only if the player has unlocked it
            if (levelsSwitch != null && i < levelsSwitch.levels.Length && levelsSwitch.levels[i].levelButton != null)
            {
                levelsSwitch.levels[i].levelButton.interactable = isUnlocked;
            }

            if (changed)
            {
                Debug.Log($"[ProgressionRewards] Checking Level {i + 1} - Required: {level.requiredSigns}, Current signsLearned: {signsLearned}, IsUnlocked: {isUnlocked}");
            }

            // 2. Manage the lock/level icons (these should display on the tab bar based on progress)
            if (isUnlocked)
            {
                if (level.unlockIcon != null)
                {
                    SetAlpha(level.unlockIcon, 0f); // Hide lock icon
                    level.unlockIcon.gameObject.SetActive(false); // Disable lock GameObject
                }
                if (level.levelIcon != null)
                {
                    SetAlpha(level.levelIcon, 1f);  // Show level content icon
                    level.levelIcon.gameObject.SetActive(true);  // Enable level content GameObject
                }
            }
            else
            {
                if (level.unlockIcon != null)
                {
                    SetAlpha(level.unlockIcon, 1f); // Show lock icon
                    level.unlockIcon.gameObject.SetActive(true);  // Enable lock GameObject
                }
                if (level.levelIcon != null)
                {
                    SetAlpha(level.levelIcon, 0f);  // Hide level content icon
                    level.levelIcon.gameObject.SetActive(false); // Disable level content GameObject
                }
            }

            // 3. Manage the claim button (only interactable if this level is active, unlocked, and unclaimed)
            if (level.claimButton != null)
            {
                level.claimButton.interactable = (i == activeLevelIndex && isUnlocked && !level.claimed);
            }
        }
    }


    public void ClaimReward(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        LevelReward level = levels[levelIndex];
        if (level.claimed) return;

        level.claimed = true;

        // Fade claimed icon
        SetAlpha(level.claimedIcon, 0f);

        // Add rewards to GameData
        GameManager.Instance.data.userXP += level.xpReward;
        GameManager.Instance.data.echoes += level.echoReward;

        Debug.Log($"Level {levelIndex + 1} reward claimed! +{level.xpReward} XP, +{level.echoReward} Echoes");

        // Disable button permanently
        level.claimButton.interactable = false;
    }

    void SetAlpha(RawImage img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
