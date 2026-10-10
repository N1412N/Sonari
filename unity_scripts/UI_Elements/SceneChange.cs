using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    // Standard scene load (destroys current scene)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Additive scene load (overlays the new scene, preserving the main scene)
    public void LoadSceneAdditive(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    // Unloads a specific scene by name
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
