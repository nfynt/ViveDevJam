using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{

    public GameObject[] gates;

    public float[] times = { 30f, 30f, 30f, 30f, 30f};

    private int timeIndex = 0;

    private float timeLeft = 0;

    private volatile bool countdown;


    public void startCountDown()
    {
        countdown = true;
    }


    void Update()
    {
        if (countdown)
        {
            timeLeft += Time.deltaTime;
            //Debug.Log(Mathf.Round(timeLeft));
            if (timeIndex == times.Length)
            {
                return;
            }
            if (timeLeft > times[timeIndex])
            {

                GetComponent<AudioSource>().Play();
                GameObject gate = gates[timeIndex];
                gate.SetActive(false);
                countdown = false;
                timeIndex += 1;
                timeLeft = 0;
            }
        }
    }



    // Start is called before the first frame update
    void Start()
    {
       
    }

}
