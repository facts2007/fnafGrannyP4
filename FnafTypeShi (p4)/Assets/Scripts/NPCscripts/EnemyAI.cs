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
    public Vector3 RayCastOffset;
    public string DeathScene;

    [HideInInspector] public bool sightDisabled = false;

    private enum EnemyState { Walking, Idle, Chasing, Jumpscare }
    private EnemyState currentState = EnemyState.Walking;
    private bool isDead = false;

    // ── Camera Swap Chain ──────────────────────────────────────────
    public Camera[] camerasToDisable;
    public Camera jumpscareCamera;
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

    // ── Fade To Black ──────────────────────────────────────────────
    public Image fadeToBlackImage;
    [Tooltip("How fast the screen fades to black before the scene loads.")]
    public float fadeToBlackSpeed = 2f;

    // ── Idle Walk Distance ─────────────────────────────────────────
    public float minWalkDistanceBeforeIdle = 3f;
    private Vector3 lastIdlePosition;
    private bool mustWalkBeforeIdle = false;


    void Start()
    {
        walking = true;
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);

        if (gameOverCanvas   != null) gameOverCanvas.enabled   = false;
        if (gameOverCamera   != null) gameOverCamera.enabled   = false;
        if (videoDisplay     != null) videoDisplay.enabled     = false;
        if (deathText        != null) deathText.enabled        = false;
        if (fadeToBlackImage != null) SetFadeAlpha(0f);

        if (jumpscareVideo != null) jumpscareVideo.Prepare();

        if (gameOverCanvas   == null) Debug.LogWarning("[EnemyAI] Game Over Canvas is not assigned!");
        if (gameOverCamera   == null) Debug.LogWarning("[EnemyAI] Game Over Camera is not assigned!");
        if (jumpscareVideo   == null) Debug.LogWarning("[EnemyAI] Jumpscare Video is not assigned!");
        if (videoDisplay     == null) Debug.LogWarning("[EnemyAI] Video Display (RawImage) is not assigned!");
        if (deathText        == null) Debug.LogWarning("[EnemyAI] Death Text is not assigned!");
        if (fadeToBlackImage == null) Debug.LogWarning("[EnemyAI] Fade To Black Image is not assigned!");
        if (jumpscareCamera  == null) Debug.LogWarning("[EnemyAI] Jumpscare Camera is not assigned!");
        if (disableUI        == null) Debug.LogWarning("[EnemyAI] Disable UI canvas is not assigned!");
        if (destinations.Count == 0)  Debug.LogWarning("[EnemyAI] No destinations assigned!");
    }

    private void Update()
    {
        if (currentState == EnemyState.Jumpscare) return;

        // Only raycast if sight isn't disabled by a hiding spot
        if (!sightDisabled)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, direction);

            // Only raycast if player is within 170 degree FOV (85 degrees either side)
            if (angleToPlayer <= 85f)
            {
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

                foreach (Camera cam in camerasToDisable)
                    if (cam != null) cam.enabled = false;

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
                bool walkedFarEnough = !mustWalkBeforeIdle ||
                    Vector3.Distance(transform.position, lastIdlePosition) >= minWalkDistanceBeforeIdle;

                randNum2 = Random.Range(0, 2);
                if (randNum2 == 0 || !walkedFarEnough)
                {
                    randNum = Random.Range(0, destinations.Count);
                    currentDest = destinations[randNum];
                }
                else
                {
                    lastIdlePosition   = transform.position;
                    mustWalkBeforeIdle = true;
                    walking            = false;
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
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);
    }

    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);
        walking = true;
        randNum = Random.Range(0, destinations.Count);
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

        yield return new WaitForSeconds(jumpscareTime);

        aiAnim.speed = 0f;

        if (jumpscareCamera != null) jumpscareCamera.enabled = false;
        if (gameOverCamera  != null) gameOverCamera.enabled  = true;
        if (gameOverCanvas  != null) gameOverCanvas.enabled  = true;
        if (videoDisplay    != null) videoDisplay.enabled    = true;

        if (deathText != null)
        {
            deathText.enabled = true;
            StartCoroutine("AnimateDots");
        }

        if (jumpscareVideo != null)
        {
            if (!jumpscareVideo.isPrepared)
                yield return new WaitUntil(() => jumpscareVideo.isPrepared);

            jumpscareVideo.Play();
            yield return new WaitUntil(() => !jumpscareVideo.isPlaying);
        }

        StopCoroutine("AnimateDots");
        if (deathText != null) deathText.enabled = false;

        if (fadeToBlackImage != null)
        {
            fadeToBlackImage.enabled = true;
            yield return StartCoroutine(FadeToBlack());
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(DeathScene);
    }

    IEnumerator FadeToBlack()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha = Mathf.MoveTowards(alpha, 1f, fadeToBlackSpeed * Time.deltaTime);
            SetFadeAlpha(alpha);
            yield return null;
        }
        SetFadeAlpha(1f);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeToBlackImage == null) return;
        Color c = fadeToBlackImage.color;
        c.a = alpha;
        fadeToBlackImage.color = c;
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