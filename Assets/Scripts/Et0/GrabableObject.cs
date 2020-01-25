using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabableObject : MonoBehaviour
{
    public bool heavyGrab;

    private bool leftGrab;
    private bool rightGrab;
    private Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ETInputHandler.Grab += ETInputHandler_Grab;
        ETInputHandler.Release += ETInputHandler_Release;
    }

    private void ETInputHandler_Release(bool isLeft, bool isRight)
    {
        Debug.Log("released!");
        leftGrab = !isLeft;
        rightGrab = !isRight;
        //rigidBody.isKinematic = false;
        Vector3 vel;
        if (leftGrab) vel = ETInputHandler.Instance.GetLeftHand.GetComponent<Rigidbody>().velocity;
        else vel = ETInputHandler.Instance.GetRightHand.GetComponent<Rigidbody>().velocity;

        ETInputHandler.Instance.GetLeftHand.GetComponent<FixedJoint>().connectedBody = null;
        ETInputHandler.Instance.GetRightHand.GetComponent<FixedJoint>().connectedBody = null;

        rigidBody.velocity = vel;
    }

    private void ETInputHandler_Grab(bool isLeft, bool isRight)
    {
        Debug.Log("grabbed!");
        leftGrab = isLeft;
        rightGrab = isRight;
        //rigidBody.isKinematic = true;

        if (leftGrab)
            ETInputHandler.Instance.GetLeftHand.GetComponent<FixedJoint>().connectedBody = rigidBody;
        else
            ETInputHandler.Instance.GetRightHand.GetComponent<FixedJoint>().connectedBody = rigidBody;
    }

    private void OnDisable()
    {
        ETInputHandler.Grab -= ETInputHandler_Grab;
        ETInputHandler.Release -= ETInputHandler_Release;

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter: "+other.gameObject.name);
        if (other.gameObject.tag == "Hands")
        {
            ETInputHandler.Instance.handEngaged = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit: "+other.gameObject.name);
        if (other.gameObject.tag == "Hands")
        {
            //ETInputHandler_Release(true, true);
            ETInputHandler.Instance.handEngaged = false;
        }
    }
}
