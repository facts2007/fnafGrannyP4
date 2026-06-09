using UnityEngine;
using TMPro;


public class InteractUI : MonoBehaviour
{
    public static InteractUI instance;

    public TextMeshProUGUI promptText;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        HidePrompt();
    }

    public void ShowPrompt(string message)
    {
        promptText.text = message;
        promptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        promptText.gameObject.SetActive(false);
    }
}