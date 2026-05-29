using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public int DestinationAmount;
    public Vector3 RayCastOffset;
    public string DeathScene;

    private enum EnemyState { Walking, Idle, Chasing, Jumpscare }
    private EnemyState currentState = EnemyState.Walking;
    private bool isDead = false;

    void Start()
    {
        walking = true;
        randNum = Random.Range(0, DestinationAmount);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);
    }

    private void Update()
    {
        if (currentState == EnemyState.Jumpscare) return;

        Vector3 direction = (player.position - transform.position).normalized;
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

        if (chasing)
        {
            dest = player.position;
            ai.destination = dest;
            ai.speed = chaseSpeed;

            // isDead guard prevents this firing multiple times
            if (!isDead && ai.hasPath && ai.remainingDistance <= catchDistance)
            {
                isDead = true;
                chasing = false;
                walking = false;
                StopCoroutine("chaseRoutine");
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
                randNum2 = Random.Range(0, 2);
                if (randNum2 == 0)
                {
                    randNum = Random.Range(0, DestinationAmount);
                    currentDest = destinations[randNum];
                }
                else
                {
                    walking = false;
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
            case EnemyState.Walking:  aiAnim.SetTrigger("walk");      break;
            case EnemyState.Idle:     aiAnim.SetTrigger("idle");      break;
            case EnemyState.Chasing:  aiAnim.SetTrigger("sprint");    break;
            case EnemyState.Jumpscare: aiAnim.SetTrigger("jumpscare"); break;
        }
    }

    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);
        walking = true;
        randNum = Random.Range(0, DestinationAmount);
        currentDest = destinations[randNum];
        SetAnimation(EnemyState.Walking);
    }

    IEnumerator chaseRoutine()
    {
        chaseTime = Random.Range(minChaseTime, maxChaseTime);
        yield return new WaitForSeconds(chaseTime);
        if (!isDead)
        {
            chasing = false;
            walking = true;
            randNum = Random.Range(0, DestinationAmount);
            currentDest = destinations[randNum];
            SetAnimation(EnemyState.Walking);
        }
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(DeathScene);
    }
}