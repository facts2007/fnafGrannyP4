using UnityEngine;
public class hidingPlace : MonoBehaviour
{
    public GameObject hideText, stopHideText;
    public GameObject normalPlayer, hidingPlayer;
    public EnemyAI monsterScript;
    public Transform monsterTransform;
    bool interactable, hiding;
    public float loseDistance;

    [Header("Raycast Settings")]
    public float interactDistance = 3f;
    public Transform playerCamera;

    void Start()
    {
        interactable = false;
        hiding = false;
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, interactDistance))
        {
            if (hit.transform == transform)
            {
                hideText.SetActive(true);
                interactable = true;
            }
            else
            {
                hideText.SetActive(false);
                interactable = false;
            }
        }
        else
        {
            hideText.SetActive(false);
            interactable = false;
        }

        if (interactable && Input.GetKeyDown(KeyCode.E))
        {
            hideText.SetActive(false);
            hidingPlayer.SetActive(true);

            float distance = Vector3.Distance(monsterTransform.position, normalPlayer.transform.position);
            if (distance > loseDistance)
            {
                if (monsterScript.chasing == true)
                {
                    monsterScript.stopChase();
                }
            }

            hiding = true;
            normalPlayer.SetActive(false);
            interactable = false;
            stopHideText.SetActive(true);
            hideText.SetActive(false);
        }

        if (hiding)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                stopHideText.SetActive(false);
                hidingPlayer.SetActive(false);
                normalPlayer.SetActive(true);
                hiding = false;
            }
        }
    }
}