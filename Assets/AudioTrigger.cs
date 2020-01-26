using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    //public delegate void TestDelegate(); // This defines what type of method you're going to call.
    public DoorManager manager;

    public AudioSource source;
    public bool triggerShooter = false;
    void OnTriggerEnter(Collider collider)
    {
        //Debug.Log(collider.gameObject);
        if (collider.gameObject.CompareTag("MainCamera"))
        {
            source.Play();
            manager.startCountDown();
            GetComponent<BoxCollider>().enabled = false;
            if (triggerShooter)
                ETInputHandler.Instance.enableShooter = true;
        }
    }
}
