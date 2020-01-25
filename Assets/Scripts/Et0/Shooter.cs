using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject firePrefab;
    public Transform leftHand;
    public Transform rightHand;
    public float force = 10;
    public Color normalColor = Color.white;
    public Color chargingColor = Color.blue;
    public Color hotColor = Color.red;

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
        parentHand.GetChild(0).GetComponent<Renderer>().material.color = chargingColor;
    }

    public void ResetCharge()
    {
        parentHand.GetChild(0).GetComponent<Renderer>().material.color = normalColor;
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
        parentHand.GetChild(0).GetComponent<Renderer>().material.color = hotColor;
    }
}
