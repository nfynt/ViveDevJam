using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletObject : MonoBehaviour
{
    public float lifeTime = 6;
    public float damage = 25f;
    public float explosionRad = 2f;
    public float explosionForce = 10f;
    public AudioSource aud;
    public AudioClip chargeClip;
    public AudioClip fireClip;
    public GameObject explosion;
    private bool explode;
    private float currRad;
    private void OnEnable()
    {
        explosion.SetActive(false);
        currRad = explosion.GetComponent<SphereCollider>().radius;
        if (aud == null)
            aud = GetComponent<AudioSource>();
        aud.clip = chargeClip;
        aud.Play();
    }

    public void Shoot()
    {
        Invoke("Destroy", lifeTime);
        aud.Stop();
        aud.clip = fireClip;
        aud.Play();
    }

    public void Update()
    {
        if(explode)
        {
            currRad += 0.2f;
            explosion.GetComponent<SphereCollider>().radius = currRad;

            if (currRad >= explosionRad)
                DestroyImmediate(this.gameObject);        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(explode)
        {
            if (collision.gameObject.name == "PlaySpace") return;

            Debug.Log("forcing: " + collision.gameObject.name);

            if (collision.gameObject.GetComponent<Rigidbody>() != null && !(collision.gameObject.CompareTag("MainCamera") ||collision.gameObject.CompareTag("Player")))
            {
                Debug.Log(collision.gameObject.name);
                collision.gameObject.GetComponent<Rigidbody>().AddForce((collision.transform.position - transform.position) * explosionForce);
            }
            
            return;
        }
        if(collision.gameObject.CompareTag("Enemy") && collision.gameObject.GetComponent<EnemyPlayer>()!=null)
        {
            collision.gameObject.GetComponent<EnemyPlayer>().DealDamage(damage);
        }
        if (collision.gameObject.CompareTag("bgate"))
            collision.transform.parent.gameObject.SetActive(false);
        Destroy();
    }

    private void Destroy()
    {
        explosion.SetActive(true);
        //GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponentInChildren<MeshRenderer>().enabled = false;
        explode = true;
        //Destroy(this.gameObject);
    }
}
