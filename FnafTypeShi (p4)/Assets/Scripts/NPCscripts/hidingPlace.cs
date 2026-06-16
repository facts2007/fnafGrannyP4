using UnityEngine;
using System.Collections.Generic;

public class hidingPlace : MonoBehaviour
{
    public GameObject hideText, stopHideText;
    public GameObject normalPlayer, hidingPlayer;
    public List<EnemyAI> monsterScripts;
    public List<Transform> monsterTransforms;
    bool hiding;
    bool playerInRange;
    public float loseDistance;

    [Header("Player Collider")]
    [Tooltip("Drag the specific collider on the player that should trigger the locker")]
    public Collider playerCollider;

    void Start()
    {
        hiding = false;
        playerInRange = false;
        if (hideText != null)     hideText.SetActive(false);
        if (stopHideText != null) stopHideText.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // only react to the specific assigned collider
        if (playerCollider != null && other == playerCollider)
        {
            playerInRange = true;
            if (!hiding && hideText != null)
                hideText.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (playerCollider != null && other == playerCollider)
        {
            playerInRange = false;
            if (hideText != null)
                hideText.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && !hiding && Input.GetKeyDown(KeyCode.E))
        {
            hiding = true;
            if (hideText != null)     hideText.SetActive(false);
            if (stopHideText != null) stopHideText.SetActive(true);
            if (hidingPlayer != null) hidingPlayer.SetActive(true);
            if (normalPlayer != null) normalPlayer.SetActive(false);

            for (int i = 0; i < monsterScripts.Count; i++)
            {
                EnemyAI monster = monsterScripts[i];
                if (monster == null) continue;

                if (monsterTransforms.Count > i && monsterTransforms[i] != null)
                {
                    float dist = Vector3.Distance(monsterTransforms[i].position, normalPlayer.transform.position);
                    if (dist > loseDistance && monster.chasing)
                        monster.stopChase();
                }

                monster.sightDisabled = true;
            }
        }

        if (hiding && Input.GetKeyDown(KeyCode.Q))
        {
            hiding = false;
            if (stopHideText != null) stopHideText.SetActive(false);
            if (hidingPlayer != null) hidingPlayer.SetActive(false);
            if (normalPlayer != null) normalPlayer.SetActive(true);

            foreach (EnemyAI monster in monsterScripts)
            {
                if (monster == null) continue;
                monster.sightDisabled = false;
            }
        }
    }
}