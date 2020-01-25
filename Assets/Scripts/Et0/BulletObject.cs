using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletObject : MonoBehaviour
{
    public float lifeTime = 5;
    private void Start()
    {
        Invoke("Destroy", lifeTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Destroy();
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }
}
