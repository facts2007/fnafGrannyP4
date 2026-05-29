using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class StartGame : MonoBehaviour
{
    public void StartGameNow()
    {
        SceneManager.LoadScene("Game");
    }
}