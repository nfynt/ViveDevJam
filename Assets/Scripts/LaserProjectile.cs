using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    private TrailRenderer _trail;

    private Vector3 _direction;

    private float _speed;

    private bool _isDead = true;

    private void Awake()
    {
        _trail = GetComponentInChildren<TrailRenderer>();
        _trail.emitting = false;
    }

    public void Fire(Vector3 direction, float speed)
    {
        _direction = direction;
        _speed = speed;
        _trail.emitting = true;
        _isDead = false;

        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }
        transform.position += _speed * _direction * Time.deltaTime;
    }


    private void OnTriggerEnter(Collider other)
    {
        _trail.emitting = false;
    }
}
