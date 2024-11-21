using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
	[SerializeField]
	protected Camera mainCamera;
	public InputActionMap actionMap;
	[SerializeField]
	protected float movementSpeed;
	[SerializeField]
	protected float rotationSpeed;


	public enum ControlType //currently unimplemented
	{
		discrete = 0, //tapping the button changes axis to next avaiable
		continous = 1 //rotation is constant, player snaps to closest axis when ended
	}

	//also has to make this to work for z and x axis

	private void OnEnable()
	{
		for (int i = 0; i < actionMap.actions.Count; ++i)
		{
			actionMap.actions[i].Enable();
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < actionMap.actions.Count; ++i)
		{
			actionMap.actions[i].Disable();
		}

	}

	private void Start()
	{
		//by default all players will not rotate by physics
		rigidbody.constraints = rigidbody.constraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
	}

}
