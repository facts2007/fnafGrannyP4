using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// EndingDoor.cs
///
/// Player reaches this door to finish a night. For Nights 1-6, it just unlocks
/// the next night and loads the next scene normally. For Night 7 (the final
/// night), it instead plays a true-ending video overlay first, awards a trophy,
/// resets night progress to 1, THEN loads the scene (typically back to main menu).
///
/// SETUP:
/// - sceneName: scene to load afterwards (same for all nights, usually your main menu)
/// - completesCurrentNight: leave true so this door always reports progress
/// - trueEndingVideo / trueEndingDisplay: assign these for the Night 7 case.
///   Same pattern as EnemyAI's jumpscare video — a VideoPlayer + RawImage.
/// - trueEndingCanvas: optional canvas to enable while the video overlay plays,
///   useful for hiding gameplay UI underneath.
/// </summary>
public class EndingDoor : MonoBehaviour
{
    [Header("Scene to load")]
    public string sceneName;

    [Header("Night Progression")]
    [Tooltip("If true, this door marks the current night as complete and unlocks the next one.")]
    public bool completesCurrentNight = true;

    [Header("True Ending (Night 7 only)")]
    public VideoPlayer trueEndingVideo;
    public RawImage     trueEndingDisplay;
    public Canvas       trueEndingCanvas;   // optional, enabled while video plays

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;
        if (!other.CompareTag("Player")) return;

        isTransitioning = true;

        bool isFinalNight = NightManager.instance != null &&
                             NightManager.instance.currentNight >= NightManager.instance.maxNights;

        if (isFinalNight)
        {
            StartCoroutine(TrueEndingRoutine());
        }
        else
        {
            CompleteAndLoad();
        }
    }

    void CompleteAndLoad()
    {
        if (completesCurrentNight && NightManager.instance != null)
            NightManager.instance.CompleteNight(NightManager.instance.currentNight);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator TrueEndingRoutine()
    {
        // Award the trophy / reset progress immediately so it's saved even if
        // something interrupts the video.
        if (completesCurrentNight && NightManager.instance != null)
            NightManager.instance.CompleteNight(NightManager.instance.currentNight);

        if (trueEndingCanvas  != null) trueEndingCanvas.enabled  = true;
        if (trueEndingDisplay != null) trueEndingDisplay.enabled = true;

        if (trueEndingVideo != null)
        {
            if (!trueEndingVideo.isPrepared)
            {
                trueEndingVideo.Prepare();
                yield return new WaitUntil(() => trueEndingVideo.isPrepared);
            }

            trueEndingVideo.Play();
            yield return new WaitUntil(() => !trueEndingVideo.isPlaying);
        }

        if (trueEndingDisplay != null) trueEndingDisplay.enabled = false;
        if (trueEndingCanvas  != null) trueEndingCanvas.enabled  = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(sceneName);
    }
}