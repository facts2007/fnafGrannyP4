using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Attach this to any GameObject in the main menu scene.
// Call TriggerTransition() from your Start button's OnClick instead of
// SceneLoader.LoadScene() — this fades to black and kills the music first,
// then loads the scene.

public class MenuTransition : MonoBehaviour
{
    [Header("Fade")]
    // Assign a full-screen black Image on top of everything in the Canvas.
    // Set its alpha to 0 in the Inspector — the script drives it.
    public Image blackOverlay;
    // Assign the Fader GameObject itself — it will be disabled at start
    // and only enabled when the transition triggers, so it won't block buttons.
    public GameObject faderObject;
    [Tooltip("How fast the screen fades to black.")]
    public float fadeToDarkSpeed = 2f;

    [Header("Music")]
    // Assign the same AudioSource as MainMenuManager's Music Source.
    public AudioSource musicSource;
    [Tooltip("How fast the music fades out during the transition.")]
    public float musicFadeOutSpeed = 3f;

    [Header("Scene")]
    public string sceneName;

    void Start()
    {
        // Keep fader disabled so it doesn't block buttons
        if (faderObject != null) faderObject.SetActive(false);
        if (blackOverlay != null) SetOverlayAlpha(0f);
    }

    // Hook this up to your Start button's OnClick
    public void TriggerTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        // Enable fader now that the button has been pressed
        if (faderObject  != null) faderObject.SetActive(true);
        if (blackOverlay != null) blackOverlay.enabled = true;

        // Fade screen to black and music to silence simultaneously
        float overlayAlpha = 0f;
        float musicVolume  = musicSource != null ? musicSource.volume : 0f;

        while (overlayAlpha < 1f)
        {
            overlayAlpha = Mathf.MoveTowards(overlayAlpha, 1f, fadeToDarkSpeed * Time.deltaTime);
            musicVolume  = Mathf.MoveTowards(musicVolume,  0f, musicFadeOutSpeed * Time.deltaTime);

            SetOverlayAlpha(overlayAlpha);
            if (musicSource != null) musicSource.volume = musicVolume;

            yield return null;
        }

        // Fully black and silent — load the scene
        if (musicSource != null) musicSource.volume = 0f;
        SetOverlayAlpha(1f);

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("[MenuTransition] No scene name set!");
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (blackOverlay == null) return;
        Color c = blackOverlay.color;
        c.a = alpha;
        blackOverlay.color = c;
    }
}