using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;

public class GazeDot : MonoBehaviour
{
    public Ray GazeRay { get; private set; }
    public SpriteRenderer sprite;
    // Max. distance of gaze ray
    [SerializeField]
    private float _gazeRange = 500f;

    // How many frames (n) position of gaze dot is determined from
    [SerializeField]
    private int _numFramesToSample = 30;

    private Transform _player;

    // World space pos. of gaze dot last frame
    private Vector3 _lastDotPos;

    // Sampled world space gaze pos. for last n frames (stored as ring buffer)
    private Vector3[] _sampledPoints;

    // Index in sampled points of most recent sample (always mod n)
    private int _frameIndex = 0;

    private void Awake()
    {
        sprite.enabled = false;
        _player = Camera.main.transform;
        _sampledPoints = new Vector3[_numFramesToSample];
    }

    private void Update()
    {
        Vector3 hitPos;

        TobiiXR_GazeRay ray = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World).GazeRay;

        if (Physics.Raycast(ray.Origin, ray.Direction, out RaycastHit hit))
        {
            hitPos = hit.point;
            if (hit.collider.tag == "Enemy")
            {
                sprite.enabled = true;
                if (hit.collider.gameObject.GetComponent<WeepingAngelEffect>() != null)
                    hit.collider.gameObject.GetComponent<WeepingAngelEffect>().ToggleEyeColor(true);
            }
            if (hit.collider.tag == "Drum")
            {
                sprite.enabled = true;
                if (hit.collider.gameObject.GetComponent<AudioSource>() != null)
                    hit.collider.gameObject.GetComponent<AudioSource>().Play();
            }
            else
                sprite.enabled = false;
        }
        else
        {
            Debug.Log("Looking outside range");
            hitPos = ray.Direction * _gazeRange;
            sprite.enabled = false;
        }

        _sampledPoints[_frameIndex] = hitPos;

        float eyeVelocity = Vector3.Distance(_lastDotPos, hitPos) / Time.deltaTime;

        Vector3 dotPos = Vector3.zero;
        _frameIndex = (_frameIndex + 1) % _numFramesToSample;

        int i = _frameIndex;
        Debug.Assert(i >= 0 && i < _numFramesToSample, "Position average error: i is " + i);
        int samplesUsed = 0;
        do
        {
            dotPos += _sampledPoints[i];
            i = (i + 1) % _numFramesToSample;
            samplesUsed++;
        }
        while (samplesUsed < _numFramesToSample);
        dotPos /= _numFramesToSample;
        transform.position = dotPos - (0.1f * ray.Direction);
        transform.rotation = Quaternion.LookRotation(_player.forward, _player.up);

        GazeRay = new Ray(ray.Origin, (transform.position - ray.Origin).normalized);

    }


}
