using UnityEngine;
using UnityEngine.SceneManagement;

public class SignDictionary_Change : MonoBehaviour
{
    // Standard scene load (destroys current scene)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Additive scene load (overlays the new scene, preserving the main scene in the background)
    public void LoadSceneAdditive(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    // Unloads a specific scene by name (returns to the main scene)
    public void UnloadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }

    // Unloads the current scene this script is in (perfect for Back/Close buttons)
    public void UnloadCurrentScene()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
