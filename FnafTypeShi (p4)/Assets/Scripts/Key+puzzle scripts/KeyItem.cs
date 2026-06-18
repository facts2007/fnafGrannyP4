using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Tooltip("Use 'Fuse' for fuse pickups, or a unique ID for regular keys")]
    public string keyID;
    public string keyName = "Key";
    [Tooltip("If true, uses the countable item system (for fuses). If false, uses unique key system (for doors).")]
    public bool isCountable = false;
    public float interactDistance = 2f;

    private Transform player;
    private bool canPickUp  = false;
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

        if (isCountable)
            KeyInventory.instance.AddItem(keyID);
        else
            KeyInventory.instance.AddKey(keyID);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}