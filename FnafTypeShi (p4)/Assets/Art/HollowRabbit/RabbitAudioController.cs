using UnityEngine;

public class RabbitAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource footstepSource;
    public AudioSource mechanicalSource;
    public AudioSource voiceSource;

    [Header("Footsteps")]
    public AudioClip[] walkFootsteps;
    public AudioClip[] runFootsteps;

    [Header("Idle Sounds")]
    public AudioClip[] idleMechanicalSounds;

    [Header("Voice Sounds")]
    public AudioClip[] distortedLaughs;
    public AudioClip jumpscareScream;

    [Range(0f, 1f)]
    public float footstepVolume = 1f;
    [Range(0f, 1f)]
    public float mechanicalVolume = 1f;
    [Range(0f, 1f)]
    public float voiceVolume = 1f;
    [Range(0f, 2f)]
    public float jumpscareVolume = 1.5f;

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }

    // WALK
    public void PlayFootstep()
    {
        AudioClip clip = GetRandomClip(walkFootsteps);

        if (clip != null)
            footstepSource.PlayOneShot(clip, footstepVolume);
    }

    // RUN
    public void PlayRunStep()
    {
        AudioClip clip = GetRandomClip(runFootsteps);

        if (clip != null)
            footstepSource.PlayOneShot(clip, footstepVolume);
    }

    // IDLE
    public void PlayIdleMechanical()
    {
        AudioClip clip = GetRandomClip(idleMechanicalSounds);

        if (clip != null)
            mechanicalSource.PlayOneShot(clip, mechanicalVolume);
    }

    // RANDOM LAUGH
    public void PlayDistortedLaugh()
    {
        AudioClip clip = GetRandomClip(distortedLaughs);

        if (clip != null)
            voiceSource.PlayOneShot(clip, voiceVolume);
    }

    // JUMPSCARE
    public void PlayJumpscare()
    {
        if (jumpscareScream != null)
            voiceSource.PlayOneShot(jumpscareScream, jumpscareVolume);
    }
}