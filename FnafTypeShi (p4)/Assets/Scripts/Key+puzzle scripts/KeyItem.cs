using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Tooltip("Use 'Fuse' for fuse pickups, or a unique ID for regular keys")]
    public string keyID;
    public string keyName = "Key";
    [Tooltip("If true, uses the countable item system (for fuses). If false, uses unique key system (for doors).")]
    public bool isCountable = false;

    [Header("Pickup Detection")]
    [Tooltip("If true, uses the camera's box collider trigger (like the fuseboxes). If false, uses distance check from Player tag.")]
    public bool useCameraCollider = false;
    public float interactDistance = 2f;

    private Transform player;
    private bool canPickUp  = false;
    private bool wasInRange = false;

    void Start()
    {
        if (!useCameraCollider)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (useCameraCollider)
        {
            // Pickup is handled via OnTriggerEnter/Exit below instead.
            if (canPickUp && Input.GetKeyDown(KeyCode.E))
                PickUp();
            return;
        }

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

    void OnTriggerEnter(Collider other)
    {
        if (!useCameraCollider) return;
        if (!other.CompareTag("Player")) return;

        canPickUp = true;
        InteractUI.instance.ShowPrompt("E - Pick Up " + keyName);
    }

    void OnTriggerExit(Collider other)
    {
        if (!useCameraCollider) return;
        if (!other.CompareTag("Player")) return;

        canPickUp = false;
        InteractUI.instance.HidePrompt();
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
        if (useCameraCollider) return; // gizmo only meaningful for distance mode
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}