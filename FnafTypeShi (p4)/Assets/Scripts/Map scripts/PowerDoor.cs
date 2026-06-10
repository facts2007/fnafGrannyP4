using UnityEngine;

public class PowerDoor : MonoBehaviour
{
    public float triggerDistance = 3f;
    public float closeDistance = 5f;
    public float openHeight = 3f;
    public float openSpeed = 2f;
    public float closeDelay = 2f;

    [Header("Sounds")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)] public float volume = 1f;

    private Transform player;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private float closeTimer = 0f;
    private bool waitingToClose = false;
    private AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0, openHeight, 0);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = volume;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerDistance)
        {
            if (!isOpen)
            {
                isOpen = true;
                PlaySound(openSound);
            }
            waitingToClose = false;
            closeTimer = 0f;
        }

        if (distance >= closeDistance && isOpen)
        {
            float distanceToTop = Vector3.Distance(transform.position, openPosition);
            if (distanceToTop < 0.05f && !waitingToClose)
            {
                waitingToClose = true;
                closeTimer = 0f;
            }
        }
        else if (distance < closeDistance)
        {
            waitingToClose = false;
            closeTimer = 0f;
        }

        if (waitingToClose)
        {
            closeTimer += Time.deltaTime;
            if (closeTimer >= closeDelay)
            {
                isOpen = false;
                waitingToClose = false;
                PlaySound(closeSound);
            }
        }

        Vector3 target = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeDistance);
    }
}