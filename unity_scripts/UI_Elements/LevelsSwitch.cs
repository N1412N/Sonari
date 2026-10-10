using UnityEngine;
using UnityEngine.UI;

public class LevelsSwitch : MonoBehaviour
{
    [System.Serializable]
    public class LevelUI
    {
        public RawImage[] elements; // all icons/images for this level
        public Button levelButton;  // navigation button for this level
        public Button claimButton;  // ✅ claim button for this level
    }

    public LevelUI[] levels; // array of levels
    public int currentActiveLevel = 0; // Track the currently active level page

    void Start()
    {
        SwitchToLevel(0); // Initialize and show Level 1 on startup
    }

    public void SwitchToLevel(int levelIndex)
    {
        currentActiveLevel = levelIndex;

        // Hide all levels first
        for (int i = 0; i < levels.Length; i++)
        {
            foreach (RawImage img in levels[i].elements)
                SetAlpha(img, 0f); // fully transparent
        }

        // Show the chosen level
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            foreach (RawImage img in levels[levelIndex].elements)
                SetAlpha(img, 1f); // fully visible
        }
    }

    void SetAlpha(RawImage img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
