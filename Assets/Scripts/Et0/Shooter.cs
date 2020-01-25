using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject firePrefab;
    public Transform leftHand;
    public Transform rightHand;
    public float force = 10;

    private GameObject bullObj;
    private Transform parentHand;
    public void Charging(bool parentLeft)
    {
        if (parentLeft)
        {
            bullObj = Instantiate(firePrefab, leftHand);
            parentHand = leftHand;
            bullObj.transform.position = leftHand.position + leftHand.forward;
        }
        else
        {
            bullObj = Instantiate(firePrefab, rightHand);
            parentHand = rightHand;
            bullObj.transform.position = rightHand.position + rightHand.forward;
        }
    }

    public void ResetCharge()
    {
        if (bullObj != null)
            Destroy(bullObj);
    }

    public void Shoot(Vector3 direction)
    {
        if (bullObj == null) return;
        bullObj.GetComponent<Rigidbody>().isKinematic = false;
        direction = parentHand.forward;
        bullObj.transform.parent = null;
        bullObj.GetComponent<Rigidbody>().AddForce(direction * force);
        bullObj.GetComponentInChildren<Collider>().enabled = true;
    }
}
