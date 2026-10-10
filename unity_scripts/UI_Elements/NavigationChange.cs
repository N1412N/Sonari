using UnityEngine;
using UnityEngine.SceneManagement;
public class NavigationChange : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
