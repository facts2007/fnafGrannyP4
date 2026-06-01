using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingDoor : MonoBehaviour
{
    [Header("Scene to load")]
    public string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger when the player touches it
        if (other.CompareTag("Player"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(sceneName);
        }
    }
}