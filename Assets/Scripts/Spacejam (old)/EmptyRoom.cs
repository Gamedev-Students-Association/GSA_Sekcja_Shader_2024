using System.Collections;
using UnityEngine;

public class EmptyRoom : MonoBehaviour
{
    public Vector3 Lower;
    public Vector3 Upper;

    [HideInInspector]
    public struct Lowerstruct
	{
        float x;
        float y;
        float z;
	}

    [HideInInspector]
    public struct Upperstruct
    {
        float x;
        float y;
        float z;
    }

    public bool GenerateWalls = true;

    public void RecalculateBounds ()
	{
        Lower = new Vector3(
            transform.position.x - transform.lossyScale.x / 2, 
            transform.position.y - transform.lossyScale.y / 2, 
            transform.position.z - transform.lossyScale.z / 2
            );
        Upper = new Vector3(
            transform.position.x + transform.lossyScale.x / 2,
            transform.position.y + transform.lossyScale.y / 2,
            transform.position.z + transform.lossyScale.z / 2
            );
    }

    /*
    public void ResynchRoom()
	{
        RoomRect[] Furnitures = gameObject.GetComponentsInChildren<RoomRect>();
        for (int i = 0; i < Furnitures.Length; i++)
		{
            Furnitures[i].SynchWithRoom(gameObject.transform);
		}
	}
    */
}
