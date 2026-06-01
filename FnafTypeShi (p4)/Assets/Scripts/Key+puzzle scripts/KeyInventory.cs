using System.Collections.Generic;
using UnityEngine;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory instance;

    private List<string> collectedKeys = new List<string>();

    void Awake()
    {
        // Singleton so any script can access it easily
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

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
}