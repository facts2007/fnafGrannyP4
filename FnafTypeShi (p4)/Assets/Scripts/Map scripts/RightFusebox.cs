using System.Collections;
using UnityEngine;

/// <summary>
/// RightFusebox.cs
///
/// SETUP:
/// - Add this script to your Right Fusebox GameObject
/// - Add a Box Collider to the fusebox, set Is Trigger = TRUE, size it to cover the interact zone
/// - Make sure your Player has the tag "Player" and has a collider on it
/// - Assign sparkParticles, leftFuseboxScript, fuseSlotTransforms in inspector
/// </summary>
public class RightFusebox : MonoBehaviour
{
    [Header("Fuse Slots (optional visuals)")]
    public Transform[] fuseSlotTransforms = new Transform[4];
    public GameObject  fusePrefab;

    [Header("Break Effect")]
    public ParticleSystem sparkParticles;
    public float          breakDelay = 2f;

    [Header("References")]
    public LeftFusebox leftFuseboxScript;
    public Collider    interactCollider; // assign your separate cube collider here

    [Header("Inventory")]
    public string fuseInventoryKey = "Fuse";
    public int    fusesRequired    = 4;

    // ── runtime ───────────────────────────────────────────────────────────────
    private int  fusesInserted = 0;
    private bool isBroken      = false;
    private bool playerInRange = false;
    private bool wasInRangeFallback = false;

    void Update()
    {
        if (isBroken) return;

        // support both trigger zone AND separate collider cube
        bool inRange = playerInRange;
        if (!inRange && interactCollider != null)
        {
            // fallback: check if player is overlapping the assigned collider
            Collider[] hits = Physics.OverlapBox(
                interactCollider.bounds.center,
                interactCollider.bounds.extents);
            foreach (var h in hits)
                if (h.CompareTag("Player")) { inRange = true; break; }

            // mimic OnTriggerEnter/Exit prompt behaviour for the fallback path
            if (inRange && !wasInRangeFallback)
            {
                int remaining = fusesRequired - fusesInserted;
                InteractUI.instance.ShowPrompt(remaining == fusesRequired
                    ? $"Needs {fusesRequired} fuses!"
                    : $"E - Insert Fuse ({remaining} left)");
            }
            else if (!inRange && wasInRangeFallback)
            {
                InteractUI.instance.HidePrompt();
            }
            wasInRangeFallback = inRange;
        }

        if (!inRange) return;

        if (Input.GetKeyDown(KeyCode.E))
            TryInsertFuse();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (!isBroken)
        {
            int remaining = fusesRequired - fusesInserted;
            InteractUI.instance.ShowPrompt(remaining == fusesRequired
                ? $"Needs {fusesRequired} fuses!"
                : $"E - Insert Fuse ({remaining} left)");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractUI.instance.HidePrompt();
    }

    void TryInsertFuse()
    {
        if (fusesInserted >= fusesRequired)
        {
            Debug.Log("[RightFusebox] All fuses already inserted.");
            return;
        }

        if (!KeyInventory.instance.HasItem(fuseInventoryKey))
        {
            InteractUI.instance.ShowPrompt("No fuse in inventory!");
            return;
        }

        KeyInventory.instance.RemoveItem(fuseInventoryKey);
        InsertFuse(fusesInserted);
        fusesInserted++;

        int remaining = fusesRequired - fusesInserted;
        if (remaining > 0)
            InteractUI.instance.ShowPrompt($"E - Insert Fuse ({remaining} left)");
        else
            InteractUI.instance.HidePrompt();

        Debug.Log($"[RightFusebox] Fuse {fusesInserted}/{fusesRequired} inserted.");

        if (fusesInserted >= fusesRequired)
            StartCoroutine(BreakSequence());
    }

    void InsertFuse(int slotIndex)
    {
        if (fusePrefab != null && slotIndex < fuseSlotTransforms.Length && fuseSlotTransforms[slotIndex] != null)
            Instantiate(fusePrefab,
                        fuseSlotTransforms[slotIndex].position,
                        fuseSlotTransforms[slotIndex].rotation,
                        fuseSlotTransforms[slotIndex]);
    }

    IEnumerator BreakSequence()
    {
        isBroken = true;
        InteractUI.instance.HidePrompt();
        Debug.Log("[RightFusebox] All fuses inserted! Breaking...");

        if (sparkParticles != null) sparkParticles.Play();
        yield return new WaitForSeconds(breakDelay);
        if (sparkParticles != null) sparkParticles.Stop();

        Debug.Log("[RightFusebox] Broken! Activating left fusebox.");
        if (leftFuseboxScript != null)
            leftFuseboxScript.ActivateFusebox();
    }
}