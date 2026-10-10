using UnityEngine;
using UnityEngine.SceneManagement;
public class SettingsChange : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
