using UnityEngine;
using UnityEngine.SceneManagement;
public class MainScreenChange : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
