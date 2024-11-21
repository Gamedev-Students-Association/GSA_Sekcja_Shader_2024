using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerControllerHuman playerHuman;
	public PlayerControllerShip playerShip;
	public Transform PlayerCamera;


	public enum PlayerType
	{
		Ship = 0,
		Human = 1
	}

	public void Start()
	{
		//only for now since there is no human player anyway
		SwitchPlayer(0);
	}

	public void SwitchPlayer(int playerType)
	{
		if (playerType == 0) //to ship
		{
			playerHuman.gameObject.SetActive(false);
			playerShip.gameObject.SetActive(true);
			PlayerCamera.SetParent(playerShip.gameObject.transform);
			PlayerCamera.localPosition = new Vector3(0, 0, 0);
			PlayerCamera.localRotation = new Quaternion();
			PlayerCamera.localScale = new Vector3(1, 1, 1);
		}
		else //to human
		{
			playerShip.gameObject.SetActive(false);
			playerHuman.gameObject.SetActive(true);
			PlayerCamera.SetParent(playerHuman.gameObject.transform);
			PlayerCamera.localPosition = new Vector3(0, 0, 0);
			PlayerCamera.localRotation = new Quaternion();
			PlayerCamera.localScale = new Vector3(1, 1, 1);
		}
	}
}
