using UnityEngine;

public class Door : MonoBehaviour
{
    [Tooltip("Must match the keyID on the Key that unlocks this door")]
    public string keyID;
    public string keyName = "Key";
    public float interactDistance = 2f;

    private Transform player;
    private bool canInteract = false;
    private bool wasInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        canInteract = distance <= interactDistance;

        if (canInteract && !wasInRange)
        {
            if (KeyInventory.instance.HasKey(keyID))
                InteractUI.instance.ShowPrompt("E - Unlock Door");
            else
                InteractUI.instance.ShowPrompt("Locked - Needs " + keyName);
            wasInRange = true;
        }
        else if (!canInteract && wasInRange)
        {
            InteractUI.instance.HidePrompt();
            wasInRange = false;
        }

        if (canInteract && Input.GetKeyDown(KeyCode.E))
            TryOpen();
    }

    void TryOpen()
    {
        if (KeyInventory.instance.HasKey(keyID))
        {
            KeyInventory.instance.RemoveKey(keyID);
            InteractUI.instance.HidePrompt();
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}