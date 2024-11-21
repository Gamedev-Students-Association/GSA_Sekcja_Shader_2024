using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PoseReseting : MonoBehaviour
{
	public bool RestoreToPose;
	public int Pose;
	public CharacterPosing PosingSpace;

    // Update is called once per frame
    void Update()
    {
        if (RestoreToPose == true)
		{
			Transform[] Bones = PosingSpace.Poses[Pose].GetComponentsInChildren<Transform>();

			for (int i = 0; i < Bones.Length; i++)
			{
				if (Bones[i].name == gameObject.name)
				{
					gameObject.transform.localPosition = Bones[i].transform.localPosition;
					gameObject.transform.localRotation = Bones[i].transform.localRotation;
					gameObject.transform.localScale = Bones[i].transform.localScale;
					break;
				}
			}

			RestoreToPose = false;
		}
    }
}
