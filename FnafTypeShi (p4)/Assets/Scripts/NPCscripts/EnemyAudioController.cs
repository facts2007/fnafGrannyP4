using UnityEngine;

// Attach this script to any enemy GameObject alongside EnemyAI.
// Assign an AudioSource and your clips in the Inspector.
// EnemyAI will automatically call OnStateChanged() when the enemy's state changes.

public class EnemyAudioController : MonoBehaviour
{
    public AudioSource audioSource;

    // ── Per-State Clips ────────────────────────────────────────────
    public AudioClip walkSound;
    public AudioClip idleSound;
    public AudioClip chaseSound;
    public AudioClip jumpscareSound;

    // ── Per-State Loop Settings ────────────────────────────────────
    public bool loopWalk      = true;
    public bool loopIdle      = false;
    public bool loopChase     = true;
    public bool loopJumpscare = false;

    // ── Per-State Volume ───────────────────────────────────────────
    [Range(0f, 1f)] public float walkVolume      = 1f;
    [Range(0f, 1f)] public float idleVolume      = 1f;
    [Range(0f, 1f)] public float chaseVolume     = 1f;
    [Range(0f, 1f)] public float jumpscareVolume = 1f;

    // ── Per-State Pitch (sound speed) ─────────────────────────────
    [Range(0.1f, 3f)] public float walkPitch      = 1f;
    [Range(0.1f, 3f)] public float idlePitch      = 1f;
    [Range(0.1f, 3f)] public float chasePitch     = 1f;
    [Range(0.1f, 3f)] public float jumpscarePitch = 1f;

    // ── Per-State Spatial Blend ────────────────────────────────────
    // 0 = fully 2D (no positional falloff), 1 = fully 3D
    [Range(0f, 1f)] public float walkSpatialBlend      = 1f;
    [Range(0f, 1f)] public float idleSpatialBlend      = 1f;
    [Range(0f, 1f)] public float chaseSpatialBlend     = 1f;
    [Range(0f, 1f)] public float jumpscareSpatialBlend = 0.3f; // default 2D-leaning for jumpscare

    // Called by EnemyAI whenever the enemy changes state.
    public void OnStateChanged(string state)
    {
        switch (state)
        {
            case "Walking":   PlaySound(walkSound,      loopWalk,      walkVolume,      walkPitch,      walkSpatialBlend);      break;
            case "Idle":      PlaySound(idleSound,      loopIdle,      idleVolume,      idlePitch,      idleSpatialBlend);      break;
            case "Chasing":   PlaySound(chaseSound,     loopChase,     chaseVolume,     chasePitch,     chaseSpatialBlend);     break;
            case "Jumpscare": PlaySound(jumpscareSound, loopJumpscare, jumpscareVolume, jumpscarePitch, jumpscareSpatialBlend); break;
        }
    }

    private void PlaySound(AudioClip clip, bool loop, float volume, float pitch, float spatialBlend)
    {
        if (audioSource == null || clip == null) return;
        audioSource.Stop();
        audioSource.clip          = clip;
        audioSource.loop          = loop;
        audioSource.volume        = volume;
        audioSource.pitch         = pitch;
        audioSource.spatialBlend  = spatialBlend;
        audioSource.Play();
    }
}