using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TrophyDisplay.cs
///
/// Fills a grid in the corner of the main menu with trophy icons based on
/// NightManager.trophyCount. Grid fills left-to-right, top-to-bottom.
///
/// EASTER EGGS:
/// Type "thatsfoxy" anywhere (no need to focus an input field) to award a
/// bonus trophy. Type "thatshollow" to reset all trophies back to 0.
/// These work like a Konami code — just type the letters in order on the
/// keyboard while in the main menu scene.
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

    [Header("Easter Egg Codes")]
    public string giveTrophyCode = "thatsfoxy";
    public string resetTrophyCode = "thatshollow";
    [Tooltip("How many seconds of inactivity before the typed buffer resets, so unrelated typing doesn't accidentally chain into a code.")]
    public float typingTimeout = 2f;

    private StringBuilder typedBuffer = new StringBuilder();
    private float lastKeyTime = 0f;

    void Start()
    {
        RefreshTrophies();
    }

    void Update()
    {
        HandleEasterEggTyping();
    }

    void HandleEasterEggTyping()
    {
        if (!Input.anyKeyDown) return;

        // Only care about letter keys for these codes
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;

        foreach (char c in input)
        {
            if (!char.IsLetter(c)) continue;

            // Reset buffer if the player paused typing for too long
            if (Time.time - lastKeyTime > typingTimeout)
                typedBuffer.Clear();

            lastKeyTime = Time.time;
            typedBuffer.Append(char.ToLower(c));

            // Keep the buffer from growing forever — trim to the longest code length
            int maxLen = Mathf.Max(giveTrophyCode.Length, resetTrophyCode.Length);
            if (typedBuffer.Length > maxLen)
                typedBuffer.Remove(0, typedBuffer.Length - maxLen);

            string current = typedBuffer.ToString();

            if (current.EndsWith(giveTrophyCode))
            {
                GiveBonusTrophy();
                typedBuffer.Clear();
            }
            else if (current.EndsWith(resetTrophyCode))
            {
                ResetTrophies();
                typedBuffer.Clear();
            }
        }
    }

    void GiveBonusTrophy()
    {
        if (NightManager.instance == null) return;
        NightManager.instance.trophyCount++;
        Debug.Log($"[TrophyDisplay] Easter egg triggered: bonus trophy awarded. Count: {NightManager.instance.trophyCount}");
        RefreshTrophies();
    }

    void ResetTrophies()
    {
        if (NightManager.instance == null) return;
        NightManager.instance.trophyCount = 0;
        Debug.Log("[TrophyDisplay] Easter egg triggered: trophies reset to 0.");
        RefreshTrophies();
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