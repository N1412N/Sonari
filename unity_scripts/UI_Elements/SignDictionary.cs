using UnityEngine;
using UnityEngine.UI; // or RawImage if your icons use RawImage

public class SignDictionary : MonoBehaviour
{
    public RawImage signIcon1; // fades when signsLearned == 1
    public RawImage signIcon2; // fades when signsLearned == 2
    public RawImage signIcon3; // fades when signsLearned == 3

    private int lastSignsLearned = -1;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.data != null)
        {
            Debug.Log($"[SignDictionary] Start - signsLearned: {GameManager.Instance.data.signsLearned}");
        }
        else
        {
            Debug.LogError("[SignDictionary] Start - GameManager.Instance or GameManager.Instance.data is null!");
        }

        // Verify Inspector assignments
        if (signIcon1 == null) Debug.LogWarning("[SignDictionary] signIcon1 is NOT assigned in the Inspector!");
        if (signIcon2 == null) Debug.LogWarning("[SignDictionary] signIcon2 is NOT assigned in the Inspector!");
        if (signIcon3 == null) Debug.LogWarning("[SignDictionary] signIcon3 is NOT assigned in the Inspector!");
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.data != null)
        {
            int signsLearned = GameManager.Instance.data.signsLearned;
            if (signsLearned != lastSignsLearned)
            {
                lastSignsLearned = signsLearned;
                Debug.Log($"[SignDictionary] Update - signsLearned value changed to: {signsLearned}");

                // Reset all icons visible (locked)
                SetAlpha(signIcon1, 1f);
                SetAlpha(signIcon2, 1f);
                SetAlpha(signIcon3, 1f);

                // Apply rules: unlock signs cumulatively by fading out the lock overlay (alpha = 0)
                if (signsLearned >= 1)
                {
                    SetAlpha(signIcon1, 0f);
                    Debug.Log("[SignDictionary] Sign 1 Unlocked (Faded lock icon 1)");
                }
                if (signsLearned >= 2)
                {
                    SetAlpha(signIcon2, 0f);
                    Debug.Log("[SignDictionary] Sign 2 Unlocked (Faded lock icon 2)");
                }
                if (signsLearned >= 3)
                {
                    SetAlpha(signIcon3, 0f);
                    Debug.Log("[SignDictionary] Sign 3 Unlocked (Faded lock icon 3)");
                }
            }
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
