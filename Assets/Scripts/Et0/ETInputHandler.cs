using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ETInputHandler : MonoBehaviour
{
    public Camera mainCam;
    public eteeDevice leftDevice;
    public eteeDevice rightDevice;
    public float moveSpeed = 1f;

    public float shootcharge = 100;
    public float shootChargeRate = 0.5f;
    private float chargedAmt = 0;
    private bool readyToShoot;
    private bool chargingShoot;

    private void Update()
    {
        Vector2 move = MoveDirection();
        move.Normalize();
        if(move.sqrMagnitude>0)
        {
            //Move
            Vector3 pos = (mainCam.transform.right * move.x + mainCam.transform.forward * move.y) * moveSpeed * Time.deltaTime;
            pos.y = transform.position.y;
            transform.Translate(pos);
        }

        if(readyToShoot && PointShoot())
        {
            Debug.Log("Shoooot!");
            readyToShoot = false;
            chargedAmt = 0;
        }

        if (IsSqueezed())
        {
            chargingShoot = true;
            if (chargedAmt < shootcharge)
                chargedAmt += shootChargeRate;
            else
                readyToShoot = true;
        }
        else if (chargingShoot) { chargingShoot = false; chargedAmt = 0f; }


        
    }

    Vector2 MoveDirection()
    {
        Vector2 leftCoord = leftDevice.trackPadCoordinates;
        if (leftCoord.sqrMagnitude > 0)
            return leftCoord;

        return rightDevice.trackPadCoordinates;
    }

    bool PointShoot()
    {
        return leftDevice.point || rightDevice.point;
    }

    bool IsSqueezed()
    {
        return leftDevice.squeeze || rightDevice.squeeze;
    }
}
