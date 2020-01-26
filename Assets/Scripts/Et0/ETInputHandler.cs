using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ETInputHandler : MonoBehaviour
{
	public static ETInputHandler Instance;

	public delegate void Grabbed(bool isLeft, bool isRight);
	public delegate void Released(bool isLeft, bool isRight);
	public delegate void PlayerDied();
	public static event Grabbed Grab;
	public static event Grabbed Release;
	public static event PlayerDied playerDied;

	public Camera mainCam;
	public Transform leftHand;
	public Transform rightHand;
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

	public bool handEngaged;
	private bool grabEventRaised=false;
	private Vector3 startPos;

	public Transform GetRightHand
	{
		get { return rightHand; }
	}

	public Transform GetLeftHand
	{
		get { return leftHand; }
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		//eteeCSharp.sendCommandToDe
		startPos = transform.position;
	}

	public void ResetPlayerPosition()
	{
		if (playerDied != null)
			playerDied.Invoke();
		transform.position = startPos;
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

		if(!handEngaged && readyToShoot && PointShoot())
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

		if (!handEngaged)
		{
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
		else
		{
			if (!grabEventRaised && (leftDevice.squeeze || rightDevice.squeeze))
			{
				Grab.Invoke(leftDevice.squeeze, rightDevice.squeeze);
				grabEventRaised = true;
			}
			else if (grabEventRaised && !leftDevice.squeeze && !rightDevice.squeeze)
			{
				grabEventRaised = false;
				Release.Invoke(leftDevice.squeeze, rightDevice.squeeze);
			}
		}
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
