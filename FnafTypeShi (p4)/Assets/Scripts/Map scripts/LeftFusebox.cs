using System.Collections;
using UnityEngine;

/// <summary>
/// LeftFusebox.cs
///
/// SETUP:
/// - Add this script to your Left Fusebox GameObject
/// - Add a Box Collider, set Is Trigger = TRUE, size it to cover the interact zone
/// - Make sure your Player has the tag "Player"
/// - Assign levers[], leverLights[], unlockTarget, confettiParticles in inspector
/// - Player presses 1/2/3/4 to flip levers, in ANY order, each lever has ONE correct
///   end state (up or down) decided randomly each playthrough.
///
/// HOW THE PUZZLE WORKS NOW:
/// - On activate, each lever is randomly assigned a target state: UP or DOWN.
/// - Player can flip levers in any order, any number of times.
/// - A lever's light goes ORANGE the instant it's in its correct state (even if
///   that's its starting position) and goes back to RED if moved out of it.
/// - When ALL 4 levers are simultaneously in their correct state → solved.
/// - There is no "wrong lever" reset anymore — no Simon Says order, just match
///   the final state of every lever.
/// </summary>
public class LeftFusebox : MonoBehaviour
{
    [System.Serializable]
    public class LeverEntry
    {
        public string    leverName    = "Lever";
        public Transform leverTransform;
        public Vector3   rotationUp   = new Vector3( 30f, 0f, 0f);
        public Vector3   rotationDown = new Vector3(-30f, 0f, 0f);
    }

    [Header("Levers (1=LMT, 2=PWR, 3=VENT, 4=MAIN)")]
    public LeverEntry[] levers = new LeverEntry[4];

    [Header("Indicator Lights")]
    public Light[] leverLights      = new Light[4];
    public Color   lightColorIdle   = Color.red;     // wrong state
    public Color   lightColorStep   = new Color(1f, 0.5f, 0f); // orange = correct state
    public Color   lightColorSolved = Color.green;   // all correct, solved
    public float   lightIntensity   = 2f;

    [Header("On Solve")]
    public GameObject     unlockTarget;
    public ParticleSystem confettiParticles;
    public float          solveConfettiDelay = 2.5f;

    [Header("Feel")]
    public float rotationSpeed = 180f;

    // ── runtime ───────────────────────────────────────────────────────────────
    private bool   isActive      = false;
    private bool   isSolved      = false;
    private bool   playerInRange = false;
    private bool[] targetDown;       // true = correct final state is DOWN
    private bool[] leverDown;        // current state
    private bool[] leverAnimating;

    private KeyCode[] leverKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    void Start()
    {
        leverDown      = new bool[levers.Length];
        leverAnimating = new bool[levers.Length];
        targetDown     = new bool[levers.Length];

        foreach (var l in leverLights)
        {
            if (l != null)
            {
                l.enabled   = false;
                l.color     = lightColorIdle;
                l.intensity = lightIntensity;
            }
        }

        for (int i = 0; i < levers.Length; i++)
            if (levers[i].leverTransform != null)
                levers[i].leverTransform.localEulerAngles = levers[i].rotationUp;
    }

    void Update()
    {
        if (!isActive || isSolved || !playerInRange) return;

        for (int i = 0; i < leverKeys.Length; i++)
        {
            if (Input.GetKeyDown(leverKeys[i]) && !leverAnimating[i])
            {
                StartCoroutine(AnimateLever(i));
                break;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (isActive && !isSolved)
            InteractUI.instance.ShowPrompt("1/2/3/4 - Pull Levers");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractUI.instance.HidePrompt();
    }

    /// <summary>Called by RightFusebox when it breaks.</summary>
    public void ActivateFusebox()
    {
        isActive = true;

        string log = "";
        for (int i = 0; i < levers.Length; i++)
        {
            targetDown[i] = Random.value > 0.5f;
            log += levers[i].leverName + (targetDown[i] ? "↓" : "↑") + " ";

            if (leverLights[i] != null)
                leverLights[i].enabled = true;
        }
        Debug.Log("[LeftFusebox] Activated! Target states: " + log);

        // Check immediately in case any lever already happens to be in its correct starting state
        RefreshAllLights();

        if (playerInRange)
            InteractUI.instance.ShowPrompt("1/2/3/4 - Pull Levers");
    }

    IEnumerator AnimateLever(int index)
    {
        leverAnimating[index] = true;
        leverDown[index]      = !leverDown[index];

        Vector3    targetRot = leverDown[index] ? levers[index].rotationDown : levers[index].rotationUp;
        Quaternion startQ    = levers[index].leverTransform.localRotation;
        Quaternion targetQ   = Quaternion.Euler(targetRot);
        float      duration  = Quaternion.Angle(startQ, targetQ) / Mathf.Max(rotationSpeed, 0.01f);
        float      elapsed   = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            levers[index].leverTransform.localRotation =
                Quaternion.Slerp(startQ, targetQ, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        levers[index].leverTransform.localEulerAngles = targetRot;
        leverAnimating[index] = false;

        RefreshLight(index);
        CheckSolved();
    }

    void RefreshLight(int index)
    {
        if (leverLights[index] == null || isSolved) return;
        leverLights[index].color = (leverDown[index] == targetDown[index]) ? lightColorStep : lightColorIdle;
    }

    void RefreshAllLights()
    {
        for (int i = 0; i < levers.Length; i++)
            RefreshLight(i);
    }

    void CheckSolved()
    {
        for (int i = 0; i < levers.Length; i++)
            if (leverDown[i] != targetDown[i]) return; // not all matching yet

        StartCoroutine(SolveSequence());
    }

    IEnumerator SolveSequence()
    {
        isSolved = true;
        InteractUI.instance.HidePrompt();
        Debug.Log("[LeftFusebox] Solved!");

        foreach (var l in leverLights)
            if (l != null) l.color = lightColorSolved;

        yield return new WaitForSeconds(solveConfettiDelay);

        if (confettiParticles != null) confettiParticles.Play();
        if (unlockTarget != null)
        {
            Destroy(unlockTarget);
            Debug.Log("[LeftFusebox] Unlock target destroyed!");
        }
    }
}