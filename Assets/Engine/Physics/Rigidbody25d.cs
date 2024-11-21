using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

//this script does nothing on it's own but markes object as "2.5d" in physics calculations. object still can be manipulated by scripts as normal
public class Rigidbody25d : MonoBehaviour
{
	private PhysicsManager manager;
	private Rigidbody rigidbody;
	public void Start()
	{
		manager = GameObject.FindObjectOfType<PhysicsManager>();
		rigidbody = gameObject.GetComponent<Rigidbody>();
		//TODO: does not updates if colliders within rigidbody changes at runtime
		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
		for (int i = 0; i < colliders.Length; ++i)
		{
			colliders[i].hasModifiableContacts = true;
		}

		//block rotation in x and y (so level designer does not need to remember)
		rigidbody.constraints = rigidbody.constraints |RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

		//for some reason we need to create a configurable joint as child of the object as well
		GameObject jointChild = Instantiate<GameObject>(new GameObject(), transform);
		Rigidbody jointRigidbody = jointChild.AddComponent<Rigidbody>();
		jointRigidbody.isKinematic = true;
		ConfigurableJoint jointInChild = jointChild.AddComponent<ConfigurableJoint>();
		jointInChild.zMotion = ConfigurableJointMotion.Locked;
		jointInChild.connectedBody = rigidbody;
	}

	public void OnEnable()
	{
		//manager.Rigidbodies.Add(rigidbody.GetInstanceID());
	}

	public void OnDisable()
	{
		//manager.Rigidbodies.Remove(rigidbody.GetInstanceID());
	}
}
