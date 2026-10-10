using UnityEngine;
using TMPro;

public class QuestMenuAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup questTextGroup;
    [SerializeField] private RectTransform questTextRect;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private Vector2 shownPosition;

    private bool isShown = false;
    private float timer = 0f;

    public void ToggleQuestText()
    {
        isShown = !isShown;
        timer = 0f;
    }

    private void OnEnable()
    {
        isShown = true;
        timer = 0f;
    }

    void Update()
    {
        if (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;

            // Smooth interpolation
            float alpha = isShown ? Mathf.Lerp(0f, 1f, t) : Mathf.Lerp(1f, 0f, t);
            questTextGroup.alpha = alpha;

            Vector2 pos = isShown
                ? Vector2.Lerp(hiddenPosition, shownPosition, t)
                : Vector2.Lerp(shownPosition, hiddenPosition, t);

            questTextRect.anchoredPosition = pos;
        }
    }
}
