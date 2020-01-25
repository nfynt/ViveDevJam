using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabableObject : MonoBehaviour
{
    public Color highlightColor = Color.yellow;
    public Color grabColor = Color.blue;
    public Color normalColor = Color.white;
    public float amplifyThrowSpeed = 150f;
    private bool leftGrab;
    private bool rightGrab;
    private Rigidbody rigidBody;
    private MeshRenderer renderer;
    private bool grabbed;
    private Vector3 lastPos;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        renderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        ETInputHandler.Grab += ETInputHandler_Grab;
        ETInputHandler.Release += ETInputHandler_Release;
    }

    private void ETInputHandler_Release(bool isLeft, bool isRight)
    {
        if (!grabbed) return;

        Debug.Log("released!");
        leftGrab = !isLeft;
        rightGrab = !isRight;
        //rigidBody.isKinematic = false;
        Vector3 vel = (transform.position - lastPos) * amplifyThrowSpeed;
        //if (leftGrab) vel = ETInputHandler.Instance.GetLeftHand.GetComponent<Rigidbody>().velocity;
        //else vel = ETInputHandler.Instance.GetRightHand.GetComponent<Rigidbody>().velocity;
        
        Debug.Log(vel);

        ETInputHandler.Instance.GetLeftHand.GetComponent<FixedJoint>().connectedBody = null;
        ETInputHandler.Instance.GetRightHand.GetComponent<FixedJoint>().connectedBody = null;

        rigidBody.velocity = vel;
        renderer.material.color = normalColor;
    }

    private void LateUpdate()
    {
        lastPos = transform.position;
    }
            
    private void ETInputHandler_Grab(bool isLeft, bool isRight)
    {
        if (!grabbed) return;

        Debug.Log("grabbed!");
        leftGrab = isLeft;
        rightGrab = isRight;
        //rigidBody.isKinematic = true;

        if (leftGrab)
            ETInputHandler.Instance.GetLeftHand.GetComponent<FixedJoint>().connectedBody = rigidBody;
        else
            ETInputHandler.Instance.GetRightHand.GetComponent<FixedJoint>().connectedBody = rigidBody;

        renderer.material.color = grabColor;
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
            grabbed = true;
            ETInputHandler.Instance.handEngaged = true;
            renderer.material.color = highlightColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit: "+other.gameObject.name);
        if (other.gameObject.tag == "Hands")
        {
            grabbed = false;
            //ETInputHandler_Release(true, true);
            ETInputHandler.Instance.handEngaged = false;
            renderer.material.color = normalColor;
        }
    }
}
