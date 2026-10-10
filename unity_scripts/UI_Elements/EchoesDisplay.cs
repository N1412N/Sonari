using UnityEngine;
using TMPro; // TextMeshPro UI

public class EchoesDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI echoesText; // assign once in Inspector

    private int lastEchoesValue = -1;

    void Start()
    {
        UpdateEchoesUI(); // initialize display
    }

    void Update()
    {
        UpdateEchoesUI(); // refresh every frame
    }

    private void UpdateEchoesUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.data != null)
        {
            int currentEchoes = GameManager.Instance.data.echoes;
            if (currentEchoes != lastEchoesValue)
            {
                lastEchoesValue = currentEchoes;
                if (echoesText != null)
                {
                    echoesText.text = currentEchoes.ToString();
                    Debug.Log($"[EchoesDisplay] UI text updated to echoes value: {currentEchoes}");
                }
                else
                {
                    Debug.LogWarning($"[EchoesDisplay] echoesText reference is null! Cannot update text to value: {currentEchoes}");
                }
            }
        }
    }
}
