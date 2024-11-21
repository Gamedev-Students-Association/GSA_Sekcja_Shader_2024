using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpaceConnector : MonoBehaviour
{
	public int ConnectionType;
	//public Vector2 EndpointDistance;
	public Quaternion FacingDirection;
	public Vector3 Streching;

	public Transform ConnectingObject;
	public Transform FirstAnchor;
	public Transform SecondAnchor;


    void Update()
    {

		Vector3 MinPosition = new Vector3(Mathf.Min(FirstAnchor.position.x, SecondAnchor.position.x), Mathf.Min(FirstAnchor.position.y, SecondAnchor.position.y), Mathf.Min(FirstAnchor.position.z, SecondAnchor.position.z));

		Vector3 DifferncePosition = new Vector3(Mathf.Abs(FirstAnchor.position.x - SecondAnchor.position.x) / 2, Mathf.Abs(FirstAnchor.position.y - SecondAnchor.position.y) / 2, Mathf.Abs(FirstAnchor.position.z - SecondAnchor.position.z) / 2);

		gameObject.transform.position = new Vector3(MinPosition.x + DifferncePosition.x, MinPosition.y + DifferncePosition.y, MinPosition.z + DifferncePosition.z);


		if (ConnectionType == 0) // centered
		{
			
		} else if (ConnectionType == 1) // centered & faced
		{
			gameObject.transform.LookAt(SecondAnchor, Vector3.forward);
		} else if (ConnectionType == 2) // centered & streched (contrapted) (bounding box)
		{

		} else if (ConnectionType == 3) // centered & faced & streched
		{
			gameObject.transform.LookAt(SecondAnchor);
			ConnectingObject.localScale = new Vector3(
				Vector3.Distance(FirstAnchor.position, SecondAnchor.position) * 2 * Streching.x + (1 - Streching.x),
				Vector3.Distance(FirstAnchor.position, SecondAnchor.position) * 2 * Streching.y + (1 - Streching.y), 
				Vector3.Distance(FirstAnchor.position, SecondAnchor.position) * 2 * Streching.z + (1 - Streching.z));
		}
		

	}
}
