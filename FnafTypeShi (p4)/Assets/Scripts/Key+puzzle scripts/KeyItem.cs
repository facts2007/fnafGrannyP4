using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Tooltip("Must match the keyID on the Door this key unlocks")]
    public string keyID;
    public string keyName = "Key";
    public float interactDistance = 2f;

    private Transform player;
    private bool canPickUp = false;
    private bool wasInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        canPickUp = distance <= interactDistance;

        if (canPickUp && !wasInRange)
        {
            InteractUI.instance.ShowPrompt("E - Pick Up " + keyName);
            wasInRange = true;
        }
        else if (!canPickUp && wasInRange)
        {
            InteractUI.instance.HidePrompt();
            wasInRange = false;
        }

        if (canPickUp && Input.GetKeyDown(KeyCode.E))
            PickUp();
    }

    void PickUp()
    {
        InteractUI.instance.HidePrompt();
        KeyInventory.instance.AddKey(keyID);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}