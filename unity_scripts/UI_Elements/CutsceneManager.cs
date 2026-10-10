using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer cutscenePlayer;
    [SerializeField] private GameObject startPageUI;
    [SerializeField] private string mainGameSceneName; // Name of the main game scene to load
    [SerializeField] private bool forcePlayCutscene = false; // Check this in the Inspector to force play for testing!

    private AsyncOperation asyncLoadOperation;

    void Start()
    {
        // Hide UI until cutscene finishes
        if (startPageUI != null) startPageUI.SetActive(false);

        if (PlayerPrefs.GetInt("CutscenePlayed", 0) == 1 && !forcePlayCutscene)
        {
            // Skip cutscene and load the main game scene normally
            SceneManager.LoadScene(mainGameSceneName);
        }
        else
        {
            // Pre-load the main game scene in the background during the cutscene!
            StartCoroutine(LoadMainGameAsync());

            // Play cutscene once
            if (cutscenePlayer != null)
            {
                cutscenePlayer.loopPointReached += OnCutsceneFinished;
                cutscenePlayer.Play();
            }
        }
    }

    IEnumerator LoadMainGameAsync()
    {
        yield return null; // Wait 1 frame to ensure video starts first

        Debug.Log($"[CutsceneManager] Starting background load for: {mainGameSceneName}");
        asyncLoadOperation = SceneManager.LoadSceneAsync(mainGameSceneName);
        
        // Prevent the scene from displaying immediately when it finishes loading
        asyncLoadOperation.allowSceneActivation = false;

        float lastReportedProgress = -10f;

        // Keep waiting while the scene loads in the background
        while (!asyncLoadOperation.isDone)
        {
            // progress goes from 0.0 to 0.9. Map it to 0% to 100%
            float progressPercentage = Mathf.Clamp01(asyncLoadOperation.progress / 0.9f) * 100f;
            if (progressPercentage >= lastReportedProgress + 5f || asyncLoadOperation.progress >= 0.9f)
            {
                lastReportedProgress = progressPercentage;
                Debug.Log($"[CutsceneManager] Background loading progress: {progressPercentage:F0}%");
            }

            // When progress reaches 0.9f, it means the scene is fully loaded and waiting for activation
            if (asyncLoadOperation.progress >= 0.9f)
            {
                Debug.Log("[CutsceneManager] Background load COMPLETE (waiting for user to click Start)!");
                break;
            }
            yield return new WaitForSeconds(0.1f); // Check 10 times a second to optimize performance
        }
    }

    void OnCutsceneFinished(VideoPlayer vp)
    {
        PlayerPrefs.SetInt("CutscenePlayed", 1);
        PlayerPrefs.Save();

        // Hide video, show UI (Start Button page)
        if (cutscenePlayer != null) cutscenePlayer.gameObject.SetActive(false);
        if (startPageUI != null) startPageUI.SetActive(true);
    }

    // Call this from your Start Button in the Inspector!
    public void StartGame()
    {
        if (asyncLoadOperation != null)
        {
            Debug.Log($"[CutsceneManager] StartGame clicked! Activating preloaded scene. Current progress: {asyncLoadOperation.progress * 100f}%");
            
            // Instantly activate the pre-loaded main game scene (no wait time!)
            asyncLoadOperation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("[CutsceneManager] StartGame clicked but asyncLoadOperation was null! Loading synchronously...");
            // Fallback if background load hasn't finished or wasn't triggered
            SceneManager.LoadScene(mainGameSceneName);
        }
    }
}
