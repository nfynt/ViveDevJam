using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public AudioSource aud;
    public AudioClip clip;
    public bool play;
    private void Update()
    {
        if(play)
        {
            play = false;
            aud.clip = clip;
            aud.Play();
        }

        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log(aud.isPlaying);
    }
}
