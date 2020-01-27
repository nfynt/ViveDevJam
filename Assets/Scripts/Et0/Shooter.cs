using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject firePrefab;
    public Transform leftHand;
    public Transform rightHand;
    public float force = 10;
    public MeshRenderer renderer;
    public Color normalColor = Color.white;
    public Color chargingColor = Color.blue;
    public Color hotColor = Color.red;

    private GameObject bullObj;
    private Transform parentHand;
    private bool isleft;
    public void Charging(bool parentLeft)
    {
        if (parentLeft)
        {
            bullObj = Instantiate(firePrefab, leftHand);
            parentHand = leftHand;
            bullObj.transform.localPosition = new Vector3(0.15f, 0f, 0);//leftHand.right;
            isleft = true;
        }
        else
        {
            bullObj = Instantiate(firePrefab, rightHand);
            parentHand = rightHand;
            bullObj.transform.localPosition = new Vector3(-0.15f, 0f, 0); //rightHand.right;
            isleft = false;
        }
        parentHand.GetChild(0).GetChild(0).GetComponentInChildren<Renderer>().material.color = chargingColor;
    }

    public void ResetCharge()
    {
        parentHand.GetChild(0).GetChild(0).GetComponentInChildren<Renderer>().material.color = normalColor;
        if (bullObj != null)
            Destroy(bullObj);
    }

    public void Shoot(Vector3 direction)
    {
        if (bullObj == null) return;
        bullObj.GetComponent<Rigidbody>().isKinematic = false;
        if (isleft)
            direction = parentHand.forward;
        else
            direction = parentHand.forward;
        bullObj.transform.parent = null;
        bullObj.GetComponent<BulletObject>().Shoot();
        bullObj.GetComponent<Rigidbody>().AddForce(direction * force);
        bullObj.GetComponentInChildren<Collider>().enabled = true;
        parentHand.GetChild(0).GetChild(0).GetComponentInChildren<Renderer>().material.color = hotColor;
    }
}
