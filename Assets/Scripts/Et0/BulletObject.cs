using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletObject : MonoBehaviour
{
    public float lifeTime = 6;
    public float damage = 25f;
    private void Start()
    {
        Invoke("Destroy", lifeTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyPlayer>().DealDamage(damage);
        }
        Destroy();
    }

    private void Destroy()
    {
        Destroy(this.gameObject);
    }
}
