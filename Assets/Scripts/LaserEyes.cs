using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using Valve.VR;

public class LaserEyes : MonoBehaviour
{
    [SerializeField]
    private float _projectileSpeed = 1000f;

    [SerializeField]
    private GameObject _projectile;

    private GazeDot _gazeDot;

    private void Awake()
    {
        _gazeDot = FindObjectOfType<GazeDot>();
    }

    private void Update()
    {
        if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.RightHand].stateDown)
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject newProj = Instantiate(_projectile, _gazeDot.GazeRay.origin, Quaternion.identity);
        newProj.GetComponent<LaserProjectile>().Fire(_gazeDot.GazeRay.direction, _projectileSpeed);
    }

}
