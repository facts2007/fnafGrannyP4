using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    [Tooltip("Must match the keyID on the Key that unlocks this door")]
    public string keyID;
    public float interactDistance = 2f;

    private Transform player;
    private TextMeshPro promptText;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Always auto-create the prompt text as a child
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        promptText = textObj.AddComponent<TextMeshPro>();
        promptText.text = "E - Unlock";
        promptText.fontSize = 3;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        bool canInteract = distance <= interactDistance;

        promptText.gameObject.SetActive(canInteract);

        if (canInteract)
        {
            promptText.transform.LookAt(Camera.main.transform);
            promptText.transform.Rotate(0, 180f, 0);
            promptText.text = KeyInventory.instance.HasKey(keyID) ? "E - Unlock" : "Locked";
        }

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            TryOpen();
        }
    }

    void TryOpen()
    {
        if (KeyInventory.instance.HasKey(keyID))
        {
            KeyInventory.instance.RemoveKey(keyID);
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}