using System.Collections.Generic;
using UnityEngine;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory instance;

    // Regular keys (unique, no duplicates)
    private List<string> collectedKeys = new List<string>();

    // Countable items (e.g. fuses)
    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // ── unique keys (existing behaviour, unchanged) ───────────────────────────

    public void AddKey(string keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log("Picked up key: " + keyID);
        }
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }

    public void RemoveKey(string keyID)
    {
        collectedKeys.Remove(keyID);
    }

    // ── countable items ───────────────────────────────────────────────────────

    public void AddItem(string itemID)
    {
        if (!itemCounts.ContainsKey(itemID))
            itemCounts[itemID] = 0;

        itemCounts[itemID]++;
        Debug.Log($"Picked up {itemID}. Count: {itemCounts[itemID]}");
    }

    public bool HasItem(string itemID)
    {
        return itemCounts.ContainsKey(itemID) && itemCounts[itemID] > 0;
    }

    public int GetItemCount(string itemID)
    {
        return itemCounts.ContainsKey(itemID) ? itemCounts[itemID] : 0;
    }

    public void RemoveItem(string itemID)
    {
        if (!HasItem(itemID))
        {
            Debug.LogWarning($"Tried to remove {itemID} but none in inventory.");
            return;
        }
        itemCounts[itemID]--;
        Debug.Log($"Used {itemID}. Remaining: {itemCounts[itemID]}");
    }
}