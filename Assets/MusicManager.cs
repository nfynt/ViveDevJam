using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] soundtrack;

    public int trackIndex = 0;

    // Use this for initialization
    void Start()
    {
        if (!GetComponent<AudioSource>().playOnAwake)
        {
            GetComponent<AudioSource>().clip = soundtrack[trackIndex];
            GetComponent<AudioSource>().Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<AudioSource>().isPlaying)
        {
            ++trackIndex;
            if (trackIndex >= soundtrack.Length)
            {
                trackIndex = 0;
            }
            GetComponent<AudioSource>().clip = soundtrack[trackIndex];
            GetComponent<AudioSource>().Play();
        }
    }
}