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
/// - Player presses 1/2/3/4 to pull levers in sequence while in range
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
    public Color   lightColorIdle   = Color.red;
    public Color   lightColorStep   = new Color(1f, 0.5f, 0f); // orange
    public Color   lightColorSolved = Color.green;
    public float   lightIntensity   = 2f;

    [Header("On Solve")]
    public GameObject     unlockTarget;
    public ParticleSystem confettiParticles;
    public float          solveConfettiDelay = 2.5f;

    [Header("Feel")]
    public float rotationSpeed      = 180f;
    public float resetFlashDuration = 0.4f;

    // ── runtime ───────────────────────────────────────────────────────────────
    private bool   isActive      = false;
    private bool   isSolved      = false;
    private bool   interactable  = true;
    private bool   playerInRange = false;
    private int[]  correctSequence;   // order to pull levers
    private bool[] correctLeverDown;  // true = must be DOWN, false = must be UP
    private int    currentStep   = 0;
    private bool[] leverDown;
    private bool[] leverAnimating;

    // Keyboard keys mapped to lever indices (1=0, 2=1, 3=2, 4=3)
    private KeyCode[] leverKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    void Start()
    {
        leverDown      = new bool[levers.Length];
        leverAnimating = new bool[levers.Length];
        correctLeverDown = new bool[levers.Length];

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
        if (!isActive || isSolved || !interactable || !playerInRange) return;

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

    public void ActivateFusebox()
    {
        isActive         = true;
        correctSequence  = RandomSequence(levers.Length);
        correctLeverDown = new bool[levers.Length];

        // randomly assign each lever a required direction
        for (int i = 0; i < correctLeverDown.Length; i++)
            correctLeverDown[i] = Random.value > 0.5f;

        foreach (var l in leverLights)
        {
            if (l != null)
            {
                l.enabled = true;
                l.color   = lightColorIdle;
            }
        }

        string seq = "";
        foreach (int idx in correctSequence)
            seq += levers[idx].leverName + (correctLeverDown[idx] ? "↓" : "↑") + " → ";
        Debug.Log("[LeftFusebox] Activated! Sequence: " + seq.TrimEnd(' ', '→'));

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

        if (leverDown[index])
            EvaluatePull(index);
    }

    void EvaluatePull(int index)
    {
        bool expectedDown = correctLeverDown[correctSequence[currentStep]];

        if (index == correctSequence[currentStep] && leverDown[index] == expectedDown)
        {
            if (leverLights[index] != null)
                leverLights[index].color = lightColorStep;

            currentStep++;
            Debug.Log($"[LeftFusebox] Correct! Step {currentStep}/{levers.Length}");

            if (currentStep >= levers.Length)
                StartCoroutine(SolveSequence());
        }
        else
        {
            Debug.Log("[LeftFusebox] Wrong! Resetting.");
            StartCoroutine(WrongReset());
        }
    }

    IEnumerator WrongReset()
    {
        interactable = false;

        foreach (var l in leverLights)
            if (l != null) l.color = Color.red;

        yield return new WaitForSeconds(resetFlashDuration);

        currentStep = 0;
        for (int i = 0; i < levers.Length; i++)
        {
            leverDown[i] = false;
            if (levers[i].leverTransform != null)
                levers[i].leverTransform.localEulerAngles = levers[i].rotationUp;
            if (leverLights[i] != null)
                leverLights[i].color = lightColorIdle;
        }

        interactable = true;
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

    int[] RandomSequence(int length)
    {
        int[] seq = new int[length];
        for (int i = 0; i < length; i++) seq[i] = i;
        for (int i = length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (seq[i], seq[j]) = (seq[j], seq[i]);
        }
        return seq;
    }
}