using UnityEngine;

public class Flashlight : MonoBehaviour
{

    public GameObject ON;
    public GameObject OFF;
    private bool isOn = false;
    void Start()
    {
        ON.SetActive(false);
        OFF.SetActive(true);
        isOn = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isOn)
                {
                ON.SetActive(false);
                OFF.SetActive(true);
                isOn = false;
            }
            else
            {
                ON.SetActive(true);
                OFF.SetActive(false);
                isOn = true;
            }
        }
    }
}
