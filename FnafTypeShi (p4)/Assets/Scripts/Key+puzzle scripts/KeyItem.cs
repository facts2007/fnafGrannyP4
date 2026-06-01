using UnityEngine;
using TMPro;

public class KeyItem : MonoBehaviour
{
    [Tooltip("Must match the keyID on the Door this key unlocks")]
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
        promptText.text = "E - Pick Up";
        promptText.fontSize = 3;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        bool canPickUp = distance <= interactDistance;

        promptText.gameObject.SetActive(canPickUp);

        if (canPickUp)
        {
            promptText.transform.LookAt(Camera.main.transform);
            promptText.transform.Rotate(0, 180f, 0);
        }

        if (canPickUp && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        KeyInventory.instance.AddKey(keyID);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}