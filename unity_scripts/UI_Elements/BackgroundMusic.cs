using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // Force the game to keep running and playing audio when minimized or unfocused (Alt-Tabbed)
        Application.runInBackground = true;

        // Singleton pattern: ensures only one music player exists in the game
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persists this music player across scene changes!
        }
        else
        {
            Destroy(gameObject); // Destroys duplicates if you return to this scene later
        }
    }
}
