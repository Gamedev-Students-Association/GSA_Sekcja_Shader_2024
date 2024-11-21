using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDisplay : MonoBehaviour
{
    public bool PosRelative;
    public bool ScaleRelative;
    public float PosSpeed;
    public float ScaleSpeed;
    public Vector3 RotSpeed;
    public Vector3[] PosStates;
    public Vector3[] ScaleStates;

    private int curPosState = 0;
    private int curScaleState = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<Rigidbody>() == null)
		{
            Rigidbody rigid = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
            rigid.isKinematic = true;
		}

        if (PosRelative)
		{
            for (int i = 0; i < PosStates.Length; ++i)
			{
                PosStates[i] = new Vector3(PosStates[i].x + gameObject.transform.localPosition.x, PosStates[i].y + gameObject.transform.localPosition.y, PosStates[i].z + gameObject.transform.localPosition.z);
			}
		}

        if (ScaleRelative)
        {
            for (int i = 0; i < ScaleStates.Length; ++i)
            {
                ScaleStates[i] = new Vector3(ScaleStates[i].x + gameObject.transform.localScale.x, ScaleStates[i].y + gameObject.transform.localScale.y, ScaleStates[i].z + gameObject.transform.localScale.z);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //move
        if (PosStates.Length > 0)
		{
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, PosStates[curPosState], PosSpeed);

            if (Vector3.Distance(gameObject.transform.localPosition, PosStates[curPosState]) <= 0.1)
            {
                curPosState = (curPosState + 1) % PosStates.Length;
            }
        }


        //rot
        //Vector3 localRot = gameObject.transform.localRotation.eulerAngles;
        //gameObject.transform.localRotation = Quaternion.Euler(localRot.x + RotSpeed.x, localRot.y + RotSpeed.y, localRot.z + RotSpeed.z);

        gameObject.transform.RotateAroundLocal(new Vector3(RotSpeed.x, RotSpeed.y, RotSpeed.z), RotSpeed.magnitude / 10);

        //gameObject.transform.localRotation = gameObject.transform.localRotation. Quaternion.AngleAxis(1, new Vector3(RotSpeed.x, RotSpeed.y, RotSpeed.z));

        //gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, Quaternion.Euler(localRot.x + RotSpeed.x, localRot.y + RotSpeed.y, localRot.z + RotSpeed.z), 1);

        //scale
        if (ScaleStates.Length > 0)
        {
            gameObject.transform.localScale = Vector3.MoveTowards(gameObject.transform.localScale, ScaleStates[curScaleState], ScaleSpeed);

            if (Vector3.Distance(gameObject.transform.localScale, ScaleStates[curScaleState]) <= 0.1)
            {
                curScaleState = (curScaleState + 1) % ScaleStates.Length;
            }
        }
    }
}
