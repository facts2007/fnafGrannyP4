using UnityEngine;

/// <summary>
/// NightManager.cs
///
/// Tracks the current night and how many nights have been unlocked.
/// Persists across scene loads (DontDestroyOnLoad) so the gameplay scene
/// and main menu both read/write the same values.
///
/// SETUP:
/// - Create an empty GameObject in your FIRST loaded scene (e.g. main menu), name it "NightManager"
/// - Add this script to it
/// - currentNight starts at 1, nightsUnlocked starts at 1 (only Night 1 playable at first)
/// - speedIncreasePerNight: how much faster enemies get per night (e.g. 0.15 = +15% speed per night)
/// </summary>
public class NightManager : MonoBehaviour
{
    public static NightManager instance;

    [Header("Night Progress")]
    public int currentNight    = 1;   // which night is about to be / being played
    public int nightsUnlocked  = 1;   // highest night the player can select
    public int maxNights       = 7;

    [Header("Trophies")]
    [Tooltip("How many times the player has beaten Night 7 (the true ending).")]
    public int trophyCount = 0;

    [Header("Difficulty Scaling")]
    [Tooltip("Multiplier added per night above 1. e.g. 0.15 = Night 2 is 15% faster, Night 3 is 30% faster, etc.")]
    public float speedIncreasePerNight = 0.15f;

    private const string SAVE_KEY_UNLOCKED = "NightsUnlocked";
    private const string SAVE_KEY_TROPHIES = "TrophyCount";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Returns the multiplier to apply to enemy speeds for the current night.</summary>
    public float GetSpeedMultiplier()
    {
        return 1f + (speedIncreasePerNight * (currentNight - 1));
    }

    /// <summary>Call this when the player WINS a night (reaches the true ending of that night).</summary>
    public void CompleteNight(int nightNumber)
    {
        if (nightNumber >= maxNights)
        {
            // Beat the final night — award a trophy and reset progression for a fresh run.
            trophyCount++;
            nightsUnlocked = 1;
            SaveProgress();
            Debug.Log($"[NightManager] FINAL NIGHT COMPLETE! Trophy #{trophyCount} earned. Progress reset to Night 1.");
            return;
        }

        if (nightNumber >= nightsUnlocked)
        {
            nightsUnlocked = nightNumber + 1;
            SaveProgress();
            Debug.Log($"[NightManager] Night {nightNumber} complete! Night {nightsUnlocked} unlocked.");
        }
    }

    public void SetCurrentNight(int nightNumber)
    {
        currentNight = Mathf.Clamp(nightNumber, 1, maxNights);
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt(SAVE_KEY_UNLOCKED, nightsUnlocked);
        PlayerPrefs.SetInt(SAVE_KEY_TROPHIES, trophyCount);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        nightsUnlocked = PlayerPrefs.GetInt(SAVE_KEY_UNLOCKED, 1);
        trophyCount    = PlayerPrefs.GetInt(SAVE_KEY_TROPHIES, 0);
    }
}