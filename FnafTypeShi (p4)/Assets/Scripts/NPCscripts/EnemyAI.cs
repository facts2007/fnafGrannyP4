using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent ai;
    public List<Transform> destinations;
    public Animator aiAnim;
    public float walkSpeed, chaseSpeed, minIdleTime, maxIdleTime, idleTime, sightDistance, catchDistance, chaseTime, minChaseTime, maxChaseTime, jumpscareTime;
    public bool walking, chasing;
    public Transform player;
    Transform currentDest;
    Vector3 dest;
    int randNum, randNum2;
    public int DestinationAmount;
    public Vector3 RayCastOffset;
    public string DeathScene;

    private enum EnemyState { Walking, Idle, Chasing, Jumpscare }
    private EnemyState currentState = EnemyState.Walking;
    private bool isDead = false;

    // ── Camera Swap Chain ──────────────────────────────────────────
    // Step 1: these are disabled when the jumpscare triggers
    public Camera[] camerasToDisable;
    // Step 2: jumpscare cam enables when player is caught
    public Camera jumpscareCamera;
    // Step 3: game over cam enables when the video starts
    public Camera gameOverCamera;

    // ── Canvases ───────────────────────────────────────────────────
    public Canvas disableUI;
    public Canvas gameOverCanvas;

    // ── Post-Jumpscare Video ───────────────────────────────────────
    public VideoPlayer jumpscareVideo;
    public RawImage videoDisplay;

    // ── Death Text ─────────────────────────────────────────────────
    public TextMeshProUGUI deathText;
    public float dotCycleSpeed = 0.4f;

    // ── Post-Jumpscare Delay ───────────────────────────────────────
    public float postJumpscareDelay = 3f;

    // ── Vignette ───────────────────────────────────────────────────
    public Image vignetteImage;

    [Header("Vignette - Intro")]
    [Range(0f, 1f)] public float vignetteStartAlpha = 0.08f;
    public float vignetteIntroSpeed                 = 1.5f;

    [Header("Vignette - Scream")]
    public float screamTimestamp                    = 1.0f;
    [Range(0f, 1f)] public float vignettePeakAlpha  = 0.85f;
    public float vignetteScreamSpeed                = 8f;

    [Header("Vignette - Post-Scream")]
    [Range(0f, 1f)] public float vignettePostScreamAlpha = 0.25f;
    public float vignettePostScreamFadeSpeed             = 3f;

    [Header("Vignette - End")]
    public float vignetteEndFadeSpeed = 1.2f;


    void Start()
    {
        walking = true;
        randNum = Random.Range(0, DestinationAmount);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);

        // Make sure game over screen is fully hidden at start
        if (gameOverCanvas  != null) gameOverCanvas.enabled  = false;
        if (gameOverCamera  != null) gameOverCamera.enabled  = false;
        if (videoDisplay    != null) videoDisplay.enabled    = false;
        if (deathText       != null) deathText.enabled       = false;
        if (vignetteImage   != null) SetVignetteAlpha(0f);

        // Pre-prepare the video so it's ready to play instantly when needed
        if (jumpscareVideo != null)
        {
            jumpscareVideo.Prepare();
        }

        // Slot validation
        if (gameOverCanvas  == null) Debug.LogWarning("[EnemyAI] Game Over Canvas is not assigned!");
        if (gameOverCamera  == null) Debug.LogWarning("[EnemyAI] Game Over Camera is not assigned!");
        if (jumpscareVideo  == null) Debug.LogWarning("[EnemyAI] Jumpscare Video is not assigned!");
        if (videoDisplay    == null) Debug.LogWarning("[EnemyAI] Video Display (RawImage) is not assigned!");
        if (deathText       == null) Debug.LogWarning("[EnemyAI] Death Text is not assigned!");
        if (vignetteImage   == null) Debug.LogWarning("[EnemyAI] Vignette Image is not assigned!");
        if (jumpscareCamera == null) Debug.LogWarning("[EnemyAI] Jumpscare Camera is not assigned!");
        if (disableUI       == null) Debug.LogWarning("[EnemyAI] Disable UI canvas is not assigned!");
    }

    private void Update()
    {
        if (currentState == EnemyState.Jumpscare) return;

        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + RayCastOffset, direction, out hit, sightDistance))
        {
            if (hit.collider.gameObject.tag == "Player" && !chasing)
            {
                walking = false;
                chasing = true;
                StopCoroutine("stayIdle");
                StopCoroutine("chaseRoutine");
                StartCoroutine("chaseRoutine");
                SetAnimation(EnemyState.Chasing);
            }
        }

        if (chasing)
        {
            dest = player.position;
            ai.destination = dest;
            ai.speed = chaseSpeed;

            if (!isDead && ai.isOnNavMesh && ai.hasPath && ai.pathStatus == NavMeshPathStatus.PathComplete && ai.remainingDistance <= catchDistance)
            {
                isDead = true;
                chasing = false;
                walking = false;
                StopCoroutine("chaseRoutine");

                // ── Camera swap step 1: disable all player cams ────
                foreach (Camera cam in camerasToDisable)
                    if (cam != null) cam.enabled = false;

                // ── Camera swap step 2: enable jumpscare cam ───────
                if (disableUI       != null) disableUI.enabled       = false;
                if (jumpscareCamera != null) jumpscareCamera.enabled = true;

                player.gameObject.SetActive(false);
                SetAnimation(EnemyState.Jumpscare);
                StartCoroutine("DeathRoutine");
            }
        }
        else if (walking)
        {
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;

            if (currentState != EnemyState.Walking)
                SetAnimation(EnemyState.Walking);

            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                randNum2 = Random.Range(0, 2);
                if (randNum2 == 0)
                {
                    randNum = Random.Range(0, DestinationAmount);
                    currentDest = destinations[randNum];
                }
                else
                {
                    walking = false;
                    SetAnimation(EnemyState.Idle);
                    StopCoroutine("stayIdle");
                    StartCoroutine("stayIdle");
                }
            }
        }
    }

    private void SetAnimation(EnemyState state)
    {
        currentState = state;
        aiAnim.ResetTrigger("walk");
        aiAnim.ResetTrigger("idle");
        aiAnim.ResetTrigger("sprint");
        aiAnim.ResetTrigger("jumpscare");

        switch (state)
        {
            case EnemyState.Walking:   aiAnim.SetTrigger("walk");      break;
            case EnemyState.Idle:      aiAnim.SetTrigger("idle");      break;
            case EnemyState.Chasing:   aiAnim.SetTrigger("sprint");    break;
            case EnemyState.Jumpscare: aiAnim.SetTrigger("jumpscare"); break;
        }

        EnemyAudioController audio = GetComponent<EnemyAudioController>();
        if (audio != null) audio.OnStateChanged(state.ToString());
    }

    public void stopChase()
    {
        chasing = false;
        walking = true;
        randNum = Random.Range(0, DestinationAmount);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);
    }

    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);
        walking = true;
        randNum = Random.Range(0, DestinationAmount);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);
    }

    IEnumerator chaseRoutine()
    {
        chaseTime = Random.Range(minChaseTime, maxChaseTime);
        yield return new WaitForSeconds(chaseTime);
        stopChase();
    }

    IEnumerator DeathRoutine()
    {
        aiAnim.ResetTrigger("walk");
        aiAnim.ResetTrigger("idle");
        aiAnim.ResetTrigger("sprint");

        // ── Phase 1: vignette fades in subtly ─────────────────────
        StartCoroutine(FadeVignette(0f, vignetteStartAlpha, vignetteIntroSpeed));

        yield return new WaitForSeconds(screamTimestamp);

        // ── Phase 2: vignette slams to peak on scream ──────────────
        yield return StartCoroutine(FadeVignette(vignetteStartAlpha, vignettePeakAlpha, vignetteScreamSpeed));

        // ── Phase 3: fades to lingering intensity ──────────────────
        yield return StartCoroutine(FadeVignette(vignettePeakAlpha, vignettePostScreamAlpha, vignettePostScreamFadeSpeed));

        float remainingJumpscareTime = jumpscareTime - screamTimestamp;
        if (remainingJumpscareTime > 0f)
            yield return new WaitForSeconds(remainingJumpscareTime);

        aiAnim.speed = 0f;

        // ── Phase 4: vignette fades out ────────────────────────────
        yield return StartCoroutine(FadeVignette(vignettePostScreamAlpha, 0f, vignetteEndFadeSpeed));

        // ── Camera swap step 3: disable jumpscare cam, enable game over cam ──
        if (jumpscareCamera != null) jumpscareCamera.enabled = false;
        if (gameOverCamera  != null) gameOverCamera.enabled  = true;
        if (gameOverCanvas  != null) gameOverCanvas.enabled  = true;

        // ── Show video ─────────────────────────────────────────────
        if (videoDisplay != null) videoDisplay.enabled = true;

        if (deathText != null)
        {
            deathText.enabled = true;
            StartCoroutine("AnimateDots");
        }

        if (jumpscareVideo != null)
        {
            // If the video hasn't finished preparing yet, wait for it
            if (!jumpscareVideo.isPrepared)
                yield return new WaitUntil(() => jumpscareVideo.isPrepared);

            jumpscareVideo.Play();
            yield return new WaitUntil(() => !jumpscareVideo.isPlaying);
        }

        StopCoroutine("AnimateDots");
        if (deathText    != null) deathText.enabled    = false;
        if (videoDisplay != null) videoDisplay.enabled = false;

        yield return new WaitForSeconds(postJumpscareDelay);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(DeathScene);
    }

    IEnumerator FadeVignette(float from, float to, float speed)
    {
        if (vignetteImage == null) yield break;
        float alpha = from;
        while (!Mathf.Approximately(alpha, to))
        {
            alpha = Mathf.MoveTowards(alpha, to, speed * Time.deltaTime);
            SetVignetteAlpha(alpha);
            yield return null;
        }
        SetVignetteAlpha(to);
    }

    private void SetVignetteAlpha(float alpha)
    {
        if (vignetteImage == null) return;
        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }

    IEnumerator AnimateDots()
    {
        string[] dotStates = { "You died...", "You died..", "You died.", "You died.." };
        int i = 0;
        while (true)
        {
            if (deathText != null)
                deathText.text = dotStates[i % dotStates.Length];
            i++;
            yield return new WaitForSeconds(dotCycleSpeed);
        }
    }
}