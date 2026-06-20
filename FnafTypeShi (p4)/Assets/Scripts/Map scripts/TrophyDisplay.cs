using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TrophyDisplay.cs
///
/// Fills a grid in the corner of the main menu with trophy icons based on
/// NightManager.trophyCount. Grid fills left-to-right, top-to-bottom.
///
/// SETUP:
/// - Put this on an empty GameObject in your Main Menu canvas
/// - trophyPrefab: a UI Image prefab for a single trophy icon
/// - gridParent: the RectTransform that will hold all spawned trophy icons
///   (give it a Grid Layout Group component for automatic positioning!)
/// - maxTrophiesShown: cap how many icons can visually fit (optional safety limit)
/// </summary>
public class TrophyDisplay : MonoBehaviour
{
    [Header("Trophy Icon")]
    public GameObject trophyPrefab;     // a simple UI Image prefab
    public RectTransform gridParent;    // needs a Grid Layout Group component

    [Header("Limits")]
    [Tooltip("Safety cap so trophies don't infinitely overflow the screen. Set high or 0 for unlimited.")]
    public int maxTrophiesShown = 100;

    void Start()
    {
        RefreshTrophies();
    }

    void Update()
    {
        // DEBUG TEST: press Y to add a fake trophy and refresh the grid.
        // Remove this before shipping!
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (NightManager.instance != null)
            {
                NightManager.instance.trophyCount++;
                Debug.Log($"[TrophyDisplay] DEBUG: Added test trophy. New count: {NightManager.instance.trophyCount}");
                RefreshTrophies();
            }
            else
            {
                Debug.LogWarning("[TrophyDisplay] DEBUG: No NightManager instance found!");
            }
        }

        // DEBUG TEST: press R to fully reset trophy count + saved PlayerPrefs data.
        // Remove this before shipping!
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (NightManager.instance != null)
            {
                NightManager.instance.trophyCount   = 0;
                NightManager.instance.nightsUnlocked = 1;
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[TrophyDisplay] DEBUG: Reset trophyCount/nightsUnlocked and cleared PlayerPrefs.");
                RefreshTrophies();
            }
        }
    }

    public void RefreshTrophies()
    {
        if (gridParent == null || trophyPrefab == null) return;

        // Clear any existing icons first (in case this is called again later)
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        int count = NightManager.instance != null ? NightManager.instance.trophyCount : 0;
        if (maxTrophiesShown > 0)
            count = Mathf.Min(count, maxTrophiesShown);

        for (int i = 0; i < count; i++)
        {
            Instantiate(trophyPrefab, gridParent);
        }

        Debug.Log($"[TrophyDisplay] Showing {count} trophies.");
    }
}