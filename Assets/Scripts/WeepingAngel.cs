using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using Tobii.XR;
using UnityEngine;
using Valve.VR;

public class WeepingAngel : MonoBehaviour, IGazeFocusable
{
    // How far (metres) to move towards player during one blind period
    [SerializeField]
    private float _stepSize = 3f;

    // How long can 
    [SerializeField]
    private float _blindTimeThreshold = 0.2f;

    private float _timePassedInvisible = 0f;

    private Camera _playerCamera;

    private Renderer _renderer;

    private float _startDistance;

    private bool _isActive = false;

    private void Awake()
    {
        _playerCamera = Camera.main;
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        if (IsOutOfView())
        {
            _timePassedInvisible += Time.deltaTime;
            if (_timePassedInvisible >= _blindTimeThreshold)
            {
                _timePassedInvisible = 0f;
                StepForward();
            }
        }
        else
        {
            _timePassedInvisible = 0f;
        }
    }

    private bool IsOutOfView()
    {
        TobiiXR_EyeTrackingData eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        Vector3 toAngel = transform.position - _playerCamera.transform.position;
        bool outsideFov = (Vector3.Angle(_playerCamera.transform.forward, toAngel) > _playerCamera.fieldOfView / 2f);

        return (eyeData.IsLeftEyeBlinking && eyeData.IsRightEyeBlinking) || outsideFov;
    }

    private void StepForward()
    {
        Vector3 toPlayer = (_playerCamera.transform.position - transform.position).normalized;
        transform.position += toPlayer * _stepSize;

        float distanceLeft = Vector3.Distance(transform.position, _playerCamera.transform.position);
        _renderer.material.color = Color.Lerp(Color.red, Color.black, distanceLeft / _startDistance);

        if (distanceLeft < _stepSize)
        {
            // TODO: Kill player
        }

    }

    public void GazeFocusChanged(bool hasFocus)
    {
        if (!_isActive && hasFocus)
        {
            _isActive = true;
            _startDistance = Vector3.Distance(_playerCamera.transform.position, transform.position);
        }
    }
}
