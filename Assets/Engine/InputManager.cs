using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputActionAsset actionsAsset;

	private void Awake()
	{
		PlayerControllerShip playerShip = GameObject.FindObjectOfType<PlayerControllerShip>(includeInactive: true);
		PlayerControllerHuman playerHuman = GameObject.FindObjectOfType<PlayerControllerHuman>(includeInactive: true);

		playerShip.actionMap = actionsAsset.FindActionMap("PlayerShip");
		playerHuman.actionMap = actionsAsset.FindActionMap("PlayerHuman");
	}
}
