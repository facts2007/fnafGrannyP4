using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Transform cameraTransform;
    public float gravity = -9.81f;

    [Header("Dash Settings")]
    [Tooltip("How much faster than normal speed the dash burst is.")]
    public float dashSpeedMultiplier = 3f;
    [Tooltip("How long the dash burst lasts, in seconds.")]
    public float dashDuration = 0.2f;
    [Tooltip("Seconds it takes to regenerate ONE dash charge.")]
    public float dashRechargeTime = 5f;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Charges")]
    [Tooltip("Max dash charges, set automatically from trophy count at Start (1 trophy = 1 dash, minimum 1).")]
    public int maxDashCharges = 1;
    [HideInInspector] public int currentDashCharges;

    CharacterController controller;
    float verticalVelocity = 0f;

    // ── Dash runtime state ──────────────────────────────────────────
    bool isDashing = false;
    float dashTimer = 0f;
    Vector3 dashDirection;
    float rechargeTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 1 trophy = 1 dash charge, minimum 1 so the player always has at least one dash.
        int trophies = NightManager.instance != null ? NightManager.instance.trophyCount : 0;
        maxDashCharges    = Mathf.Max(1, trophies);
        currentDashCharges = maxDashCharges;

        Debug.Log($"[PlayerMovement] Max dash charges set to {maxDashCharges} (from {trophies} trophies).");
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * moveZ + right * moveX;
        move = move.normalized;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        // ── Dash input ──────────────────────────────────────────────
        if (Input.GetKeyDown(dashKey) && !isDashing && currentDashCharges > 0)
            StartDash(move);

        HandleDashRecharge();

        // ── Movement ────────────────────────────────────────────────
        Vector3 finalMove;
        if (isDashing)
        {
            finalMove = dashDirection * (speed * dashSpeedMultiplier) + Vector3.up * verticalVelocity;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }
        else
        {
            finalMove = move * speed + Vector3.up * verticalVelocity;
        }

        controller.Move(finalMove * Time.deltaTime);
    }

    void StartDash(Vector3 moveInput)
    {
        // Dash in whatever direction the player is currently moving;
        // if standing still, dash straight forward.
        dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput : (cameraTransform.forward.WithY(0f)).normalized;

        isDashing = true;
        dashTimer = dashDuration;
        currentDashCharges--;

        Debug.Log($"[PlayerMovement] Dash used. Charges remaining: {currentDashCharges}/{maxDashCharges}");
    }

    void HandleDashRecharge()
    {
        if (currentDashCharges >= maxDashCharges) return;

        rechargeTimer += Time.deltaTime;
        if (rechargeTimer >= dashRechargeTime)
        {
            rechargeTimer = 0f;
            currentDashCharges++;
            Debug.Log($"[PlayerMovement] Dash charge recovered. Charges: {currentDashCharges}/{maxDashCharges}");
        }
    }
}

// Small helper extension to zero out Y component inline.
public static class Vector3Extensions
{
    public static Vector3 WithY(this Vector3 v, float y)
    {
        v.y = y;
        return v;
    }
}