using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ETInputHandler : MonoBehaviour
{
	public Camera mainCam;
	[Header("Etee API")]
	public CSharpSerial eteeCSharp;
	public eteeDevice leftDevice;
	public eteeDevice rightDevice;

	[Header("Player props")]
	public float moveSpeed = 1f;

	[Header("Shoot params")]
	public Shooter shooter;
	public float shootcharge = 100;
	public float shootChargeRate = 0.5f;
	public float coolTime = 2f;
	private bool chargeToLeft;
	private bool chargeToRight;
	private bool chargingShoot;
	private float chargedAmt = 0;
	private bool readyToShoot;
	private float shooterHot;

	private void Start()
	{
		//eteeCSharp.sendCommandToDe
	}

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
			if (chargeToLeft)
				shooter.Shoot(rightDevice.transform.forward);
			else
				shooter.Shoot(leftDevice.transform.forward);
			readyToShoot = false;
			chargedAmt = 0;
			shooterHot = coolTime;
		}

		if (shooterHot <= 0 && IsSqueezed())
		{
			if (chargedAmt < shootcharge)
			{
				chargedAmt += shootChargeRate;
				if (!chargingShoot)
				{
					shooter.Charging(!chargeToLeft);
					//if (chargeToLeft)
					//	eteeCSharp.SendVibrationCommand("left");
					//else
					//	eteeCSharp.SendVibrationCommand("right");
				}
			}
			else
				readyToShoot = true;
			chargingShoot = true;
		}
		else if (shooterHot <= 0 && chargingShoot)
		{
			chargingShoot = false; chargedAmt = 0f; shooter.ResetCharge();

			//if (chargeToLeft)
			//	eteeCSharp.SendVibrationCommand("left");
			//else
			//	eteeCSharp.SendVibrationCommand("right");
		}
		else if (shooterHot > 0) { shooterHot -= Time.deltaTime; }

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
		if (chargeToLeft && rightDevice.ring < 30) return true;
		else if (chargeToRight && leftDevice.ring < 30) return true;

		return false;
		//return leftDevice.point || rightDevice.point;
	}

	bool IsSqueezed()
	{
		if (leftDevice.squeeze) chargeToLeft = true;
		else chargeToLeft = false;

		if (!chargeToLeft && rightDevice.squeeze) chargeToRight = true;
		else if (!rightDevice.squeeze) chargeToRight = false;

		return leftDevice.squeeze || rightDevice.squeeze;
	}
}
