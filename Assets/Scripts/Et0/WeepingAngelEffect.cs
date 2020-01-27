using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using Tobii.XR;
using UnityEngine;

public class WeepingAngelEffect : MonoBehaviour,IGazeFocusable
{
    // How far (metres) to move towards player during one blind period
    [SerializeField]
    private float moveSpeed = 3f;
    public float playerDist = 20f;
    // How long can 
    [SerializeField]
    private float _blindTimeThreshold = 0.2f;
    public AudioSource audio;
    public Color attackCol = Color.red;
    public Color peaceCol = Color.green;
    public MeshRenderer eye;
    private float _timePassedInvisible = 0f;

    private Camera _playerCamera;
    private Vector3 playerDir;
    private bool _isActive = false;
    private Vector3 startPos;

    private void Awake()
    {
        _playerCamera = Camera.main;
        // _renderer = GetComponent<Renderer>();
        startPos = transform.position;
        audio.Stop();
        transform.GetChild(0).LookAt(_playerCamera.transform.position);
    }

    public void ToggleEyeColor(bool peace)
    {
        if (peace) eye.material.color = peaceCol;
        else eye.material.color = attackCol;
    }

    private void OnEnable()
    {
        ETInputHandler.playerDied += ETInputHandler_playerDied;
    }

    private void ETInputHandler_playerDied()
    {
        _isActive = false;
        transform.position = startPos;
    }
    void OnDisable()
    {
        ETInputHandler.playerDied -= ETInputHandler_playerDied;
    }

    private void Update()
    {
        playerDir = _playerCamera.transform.position - transform.position;
        if (!_isActive)
        {
            if (playerDir.sqrMagnitude <= playerDist)
                _isActive = true;
            return;
        }

        if (IsOutOfView())
        {
            _timePassedInvisible += Time.deltaTime;
            //Vector3 distDir = _playerCamera.transform.position - transform.position;

            if (playerDir.sqrMagnitude > playerDist) { _isActive = false;return; }

            if (_timePassedInvisible >= _blindTimeThreshold)
            {
                _timePassedInvisible = 0f;
                transform.Translate(playerDir.normalized * moveSpeed * Time.deltaTime);
            }

            if(playerDir.sqrMagnitude<2f)
            {
                Debug.Log("Kill Player... Weeping bot wins");
                ETInputHandler.Instance.ResetPlayerPosition();
            }
            if (!audio.isPlaying)
                audio.Play();
            transform.GetChild(0).LookAt(_playerCamera.transform.position);
            ToggleEyeColor(false);
        }
        else
        {
            _timePassedInvisible = 0f;
            if (audio.isPlaying)
                audio.Stop();
        }
    }

    private bool IsOutOfView()
    {
        TobiiXR_EyeTrackingData eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        Vector3 toAngel = transform.position - _playerCamera.transform.position;
        bool outsideFov = (Vector3.Angle(_playerCamera.transform.forward, toAngel) > _playerCamera.fieldOfView / 2f);

        return (eyeData.IsLeftEyeBlinking && eyeData.IsRightEyeBlinking) || outsideFov;
    }

    //private void StepForward()
    //{
    //    if ((_playerCamera.transform.position - transform.position).sqrMagnitude < 1f)
    //    {
    //        Debug.Log("You're dead");
    //        return;
    //    }
    //    Vector3 toPlayer = (_playerCamera.transform.position - transform.position).normalized;
    //    transform.position += toPlayer * _stepSize;

    //    float distanceLeft = Vector3.Distance(transform.position, _playerCamera.transform.position);
    //    _renderer.material.color = Color.Lerp(Color.red, Color.black, distanceLeft / _startDistance);

    //    if (distanceLeft < _stepSize)
    //    {
    //        // TODO: Kill player
    //    }

    //}

    public void GazeFocusChanged(bool hasFocus)
    {
        if (_isActive && hasFocus)
        {
            
        }
    }
}
