using UnityEngine;
using UnityEngine.SceneManagement;
public class BackpackChange : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
