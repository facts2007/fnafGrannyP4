using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// NightSelectMenu.cs
///
/// Pressing START reveals 7 night-select buttons sliding out to the RIGHT
/// from behind the START button. Locked nights are greyed out and unclickable.
///
/// SETUP:
/// - Put this on an empty GameObject in your Main Menu canvas (e.g. "NightSelectMenu")
/// - startButton: your existing START button — this now triggers the reveal
///   instead of (or before) loading the scene directly
/// - nightButtons[]: the 7 night number buttons, IN ORDER (1 through 7)
/// - panelRoot: the parent RectTransform holding all 7 night buttons
/// - hiddenLocalX: where the panel sits when hidden (behind the START button, e.g. 0)
/// - shownLocalX: where the panel slides out to (to the right, e.g. 400)
/// - slideSpeed: how fast the panel slides (units/sec)
/// - staggerDelay: small delay between each button appearing for a nicer cascade effect (0 = all at once)
/// </summary>
public class NightSelectMenu : MonoBehaviour
{
    [Header("Trigger Button")]
    public Button startButton;
    [Tooltip("If true, pressing START only reveals the night panel (first press) then loads on night select. If false, START still loads directly and this panel must be opened some other way.")]
    public bool startButtonRevealsPanel = true;

    [Header("Night Buttons (in order, 1-7)")]
    public Button[] nightButtons = new Button[7];

    [Header("Slide Panel")]
    public float hiddenLocalX = 0f;
    public float shownLocalX  = 400f;
    public float slideSpeed   = 1500f;
    [Tooltip("Delay in seconds between each button's individual slide-out, for a cascading reveal. Set 0 for all at once.")]
    public float staggerDelay = 0.05f;

    [Header("Locked/Unlocked Look")]
    public Color unlockedColor = Color.white;

    [Header("Scene Transition")]
    [Tooltip("Drag your MenuTransition component here — it handles the fade-to-black and scene load.")]
    public MenuTransition menuTransition;

    private bool isShown = false;
    private bool isAnimating = false;
    private RectTransform[] buttonRects;
    private Vector2[] buttonHiddenPos;
    private Vector2[] buttonShownPos;

    void Start()
    {
        if (startButton != null && startButtonRevealsPanel)
            startButton.onClick.AddListener(OnStartPressed);

        CacheButtonPositions();
        SetupNightButtons();
        HideAllButtonsInstant();
    }

    void CacheButtonPositions()
    {
        buttonRects     = new RectTransform[nightButtons.Length];
        buttonHiddenPos = new Vector2[nightButtons.Length];
        buttonShownPos  = new Vector2[nightButtons.Length];

        for (int i = 0; i < nightButtons.Length; i++)
        {
            if (nightButtons[i] == null) continue;
            RectTransform rt = nightButtons[i].GetComponent<RectTransform>();
            buttonRects[i] = rt;

            // Each button's "shown" position is its original designed position in the editor.
            Vector2 originalPos = rt.anchoredPosition;
            buttonShownPos[i]  = originalPos;
            buttonHiddenPos[i] = new Vector2(hiddenLocalX, originalPos.y);
        }
    }

    void HideAllButtonsInstant()
    {
        for (int i = 0; i < buttonRects.Length; i++)
        {
            if (buttonRects[i] == null) continue;
            buttonRects[i].anchoredPosition = buttonHiddenPos[i];
            SetButtonAlpha(i, 0f);
        }
    }

    void SetButtonAlpha(int index, float alpha)
    {
        if (nightButtons[index] == null) return;
        Image img = nightButtons[index].GetComponent<Image>();
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void SetupNightButtons()
    {
        int unlocked = NightManager.instance != null ? NightManager.instance.nightsUnlocked : 1;

        for (int i = 0; i < nightButtons.Length; i++)
        {
            int nightNumber = i + 1; // closures need a local copy
            Button btn = nightButtons[i];
            if (btn == null) continue;

            bool isUnlocked = nightNumber <= unlocked;
            btn.interactable = isUnlocked;

            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (isUnlocked)
                {
                    img.color = unlockedColor;
                }
                else
                {
                    Color c = unlockedColor; // start from full white/normal
                    c.r = 0f;                // zero out the R channel for locked look
                    img.color = c;
                }
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectNight(nightNumber));
        }
    }

    void OnStartPressed()
    {
        if (isAnimating) return;

        if (!isShown)
        {
            // First press: reveal the night options instead of loading immediately
            isShown = true;
            StartCoroutine(RevealButtons());
        }
        // If already shown, pressing START again does nothing here —
        // the player needs to pick a specific night number.
        // (If you'd rather START load Night 1 directly on second press, let me know.)
    }

    IEnumerator RevealButtons()
    {
        isAnimating = true;

        for (int i = 0; i < nightButtons.Length; i++)
        {
            if (nightButtons[i] != null)
                StartCoroutine(SlideButton(i, true));

            if (staggerDelay > 0f)
                yield return new WaitForSeconds(staggerDelay);
        }

        // Wait for the last one to actually finish sliding before unlocking input
        yield return new WaitForSeconds(0.4f);
        isAnimating = false;
    }

    IEnumerator SlideButton(int index, bool show)
    {
        RectTransform rt = buttonRects[index];
        if (rt == null) yield break;

        Vector2 startPos  = rt.anchoredPosition;
        Vector2 target    = show ? buttonShownPos[index] : buttonHiddenPos[index];
        float   startA    = nightButtons[index] != null ? nightButtons[index].GetComponent<Image>().color.a : 0f;
        float   targetA   = show ? 1f : 0f;

        // Use total travel distance to drive a consistent 0-1 progress for the fade,
        // since slide speed is in units/sec and distances vary slightly per button.
        float totalDist = Vector2.Distance(startPos, target);
        if (totalDist < 0.01f) totalDist = 0.01f;

        while (Vector2.Distance(rt.anchoredPosition, target) > 0.5f)
        {
            rt.anchoredPosition = Vector2.MoveTowards(rt.anchoredPosition, target, slideSpeed * Time.deltaTime);

            float traveled = Vector2.Distance(startPos, rt.anchoredPosition);
            float t = Mathf.Clamp01(traveled / totalDist);
            SetButtonAlpha(index, Mathf.Lerp(startA, targetA, t));

            yield return null;
        }
        rt.anchoredPosition = target;
        SetButtonAlpha(index, targetA);
    }

    void SelectNight(int nightNumber)
    {
        if (NightManager.instance != null)
            NightManager.instance.SetCurrentNight(nightNumber);

        Debug.Log($"[NightSelectMenu] Starting Night {nightNumber}");

        if (menuTransition != null)
            menuTransition.TriggerTransition();
        else
            Debug.LogWarning("[NightSelectMenu] No MenuTransition assigned! Loading scene directly as fallback.");
    }
}