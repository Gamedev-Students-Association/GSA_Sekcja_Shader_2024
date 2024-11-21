using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterPosing : MonoBehaviour
{
	public bool ManualRepose;
	public int CurrentPose;
	public GameObject AnimatedObject;
	public GameObject[] Poses;

	public void Repose()
	{
		Transform[] Bones = AnimatedObject.GetComponentsInChildren<Transform>();

		Transform[] PoseBones = Poses[CurrentPose].GetComponentsInChildren<Transform>();

		//secure transform
		for (int i = 0; i < Bones.Length; i++)
		{
			for (int j = 0; j < PoseBones.Length; j++)
			{
				if (Bones[i].gameObject.name == PoseBones[j].gameObject.name)
				{
					Bones[i].transform.localPosition = PoseBones[j].transform.localPosition;
					Bones[i].transform.localRotation = PoseBones[j].transform.localRotation;
					Bones[i].transform.localScale = PoseBones[j].transform.localScale;
				}
			}
		}

		/*
		for (int i = 0; i < Bones.Length; i++)
		{
			Bones[i].transform.localPosition = PoseBones[i].transform.localPosition;
			Bones[i].transform.localRotation = PoseBones[i].transform.localRotation;
			Bones[i].transform.localScale = PoseBones[i].transform.localScale;
		}
		*/
	}

	void Update()
    {
		if (ManualRepose == true)
		{
			ManualRepose = false;
			Repose();
		}

        if (Application.isPlaying == true)
		{
			
		}
    }
}
