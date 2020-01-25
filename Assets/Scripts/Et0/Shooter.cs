using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject firePrefab;
    public float force = 10;

    public void Shoot(Vector3 direction)
    {
        GameObject go = Instantiate(firePrefab, transform);

        go.GetComponent<Rigidbody>().AddForce(direction * force);
    }
}
