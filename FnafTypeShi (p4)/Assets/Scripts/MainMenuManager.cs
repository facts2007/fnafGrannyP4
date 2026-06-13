using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Splash Video")]
    public VideoPlayer splashVideo;
    public RawImage splashDisplay;
    public AudioSource splashAudioSource;
    [Tooltip("Seconds into the video when the slide-up and fade-out begins.")]
    public float splashExitTimestamp  = 3f;
    [Tooltip("How far upward the splash slides (in UI units).")]
    public float splashSlideDistance  = 300f;
    [Tooltip("How fast the slide and fade happen (higher = faster).")]
    public float splashExitSpeed      = 2f;

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    [Tooltip("Overall master volume of the music (1 = full).")]
    [Range(0f, 1f)] public float musicMasterVolume = 1f;
    [Tooltip("How many seconds after start before the music fades down.")]
    public float musicFadeDelay       = 2.5f;
    [Tooltip("Volume the music settles to after the initial fade down (relative to master).")]
    [Range(0f, 1f)] public float musicAmbientVolume = 0.4f;
    [Tooltip("How fast the music fades down to ambient volume.")]
    public float musicFadeSpeed       = 1.5f;

    [Header("Background Image BPM Pulse")]
    public RectTransform backgroundImage;
    [Tooltip("BPM of your background music track.")]
    public float bpm                  = 120f;
    [Tooltip("How much the image scales up on the beat. 1.05 = 5% bigger.")]
    public float pulseScale           = 1.05f;
    [Tooltip("How fast it snaps up to the pulse scale (higher = sharper hit).")]
    public float pulseAttackSpeed     = 20f;
    [Tooltip("How fast it eases back down to normal size.")]
    public float pulseDecaySpeed      = 4f;

    [Header("Menu UI")]
    public GameObject menuUI;

    // ── Internals ──────────────────────────────────────────────────
    private Vector3 baseScale;
    private bool pulsing = false;
    private static bool splashHasPlayed = false;

    [Header("Startup Delay")]
    [Tooltip("Seconds to wait before music, pulse and splash all begin. Tweak this to fix sync.")]
    public float startupDelay = 0.15f;


    void Start()
    {
        if (menuUI != null) menuUI.SetActive(true);
        baseScale = backgroundImage != null ? backgroundImage.localScale : Vector3.one;

        if (splashDisplay != null) splashDisplay.enabled = false;

        // Prepare video early so it's ready when the delay ends
        if (!splashHasPlayed && splashVideo != null)
        {
            if (splashAudioSource != null)
            {
                splashVideo.audioOutputMode = VideoAudioOutputMode.AudioSource;
                splashVideo.SetTargetAudioSource(0, splashAudioSource);
            }
            splashVideo.Prepare();
        }

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(startupDelay);

        // Start background music at full volume then fade to ambient
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip   = backgroundMusic;
            musicSource.loop   = true;
            musicSource.volume = musicMasterVolume;
            musicSource.Play();
            StartCoroutine(FadeMusicToAmbient());
        }

        // Start BPM pulse
        if (backgroundImage != null)
        {
            pulsing = true;
            StartCoroutine(BeatPulse());
        }

        // Play splash only the first time this scene is visited
        if (!splashHasPlayed && splashVideo != null && splashDisplay != null)
        {
            splashHasPlayed       = true;
            splashDisplay.enabled = true;
            StartCoroutine(PlaySplash());
        }
    }

    void Update()
    {
        if (backgroundImage != null && pulsing)
        {
            backgroundImage.localScale = Vector3.Lerp(
                backgroundImage.localScale,
                baseScale,
                pulseDecaySpeed * Time.deltaTime
            );
        }

        // Press Space to instantly skip the splash video
        if (Input.GetKeyDown(KeyCode.Space) && splashDisplay != null && splashDisplay.enabled)
        {
            StopCoroutine("PlaySplash");
            StopCoroutine("SplashExit");
            splashVideo.Stop();
            splashDisplay.enabled = false;
        }
    }

    IEnumerator PlaySplash()
    {
        yield return new WaitUntil(() => splashVideo.isPrepared);
        splashVideo.Play();
        yield return new WaitForSeconds(splashExitTimestamp);
        yield return StartCoroutine(SplashExit());
        splashVideo.Stop();
        splashDisplay.enabled = false;
    }

    IEnumerator SplashExit()
    {
        RectTransform rt  = splashDisplay.GetComponent<RectTransform>();
        Vector2 startPos  = rt.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0f, splashSlideDistance);
        Color startColor  = splashDisplay.color;
        float elapsed     = 0f;

        while (elapsed < 1f)
        {
            elapsed            += Time.deltaTime * splashExitSpeed;
            float t             = Mathf.Clamp01(elapsed);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            Color c             = startColor;
            c.a                 = Mathf.Lerp(1f, 0f, t);
            splashDisplay.color = c;
            yield return null;
        }

        rt.anchoredPosition = targetPos;
        Color final         = startColor;
        final.a             = 0f;
        splashDisplay.color = final;
    }

    IEnumerator FadeMusicToAmbient()
    {
        yield return new WaitForSeconds(musicFadeDelay);
        float ambientTarget = musicMasterVolume * musicAmbientVolume;
        while (musicSource.volume > ambientTarget)
        {
            musicSource.volume = Mathf.MoveTowards(
                musicSource.volume,
                ambientTarget,
                musicFadeSpeed * Time.deltaTime
            );
            yield return null;
        }
        musicSource.volume = ambientTarget;
    }

    IEnumerator BeatPulse()
    {
        float beatInterval = 60f / bpm;
        while (pulsing)
        {
            backgroundImage.localScale = baseScale * pulseScale;
            yield return new WaitForSeconds(beatInterval);
        }
    }

    // ── Button Methods ─────────────────────────────────────────────
    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}