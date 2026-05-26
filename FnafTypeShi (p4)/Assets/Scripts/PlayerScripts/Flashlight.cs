using UnityEngine;

using System.Collections;


public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Objects")]
    public GameObject ON;
    public GameObject OFF;

    [Header("Flicker Settings")]
    public bool enableFlicker = true;
   
    public Vector2 timeBetweenBursts = new Vector2(2f, 6f);
   
    public Vector2 burstDuration = new Vector2(0.2f, 1.0f);
  
    public Vector2 intraFlickerDelay = new Vector2(0.03f, 0.15f);
   
    [Range(0f, 1f)] public float hardCutChance = 0.25f;

    private bool isOn = false;
    private Coroutine flickerRoutine;

    void Start()
    {
        SetFlashlight(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SetFlashlight(!isOn);
        }
    }

    private void SetFlashlight(bool turnOn)
    {
        isOn = turnOn;
        ON.SetActive(isOn);
        OFF.SetActive(!isOn);

        if (isOn && enableFlicker)
        {
            if (flickerRoutine == null)
                flickerRoutine = StartCoroutine(FlickerLoop());
        }
        else
        {
            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }
            ON.SetActive(isOn);
            OFF.SetActive(!isOn);
        }
    }

    private IEnumerator FlickerLoop()
    {
        var waitBetweenBursts = new WaitForSeconds(0f);
        while (isOn)
        {
            float waitTime = Random.Range(timeBetweenBursts.x, timeBetweenBursts.y);
            waitBetweenBursts = new WaitForSeconds(waitTime);
            yield return waitBetweenBursts;

            float endTime = Time.time + Random.Range(burstDuration.x, burstDuration.y);
            bool lastState = true;

            while (Time.time < endTime && isOn)
            {
                bool hardCut = Random.value < hardCutChance;

                // Toggle visibility
                lastState = !lastState;
                ON.SetActive(lastState && !hardCut);
                OFF.SetActive(!(lastState && !hardCut));

                float d = Random.Range(intraFlickerDelay.x, intraFlickerDelay.y);
                yield return new WaitForSeconds(d);

                if (Random.value < 0.2f)
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
            }

            if (isOn)
            {
                ON.SetActive(true);
                OFF.SetActive(false);
            }
        }

        flickerRoutine = null;
    }
}
