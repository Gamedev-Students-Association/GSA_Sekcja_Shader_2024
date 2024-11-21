using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineEnums : MonoBehaviour
{
	[Tooltip("internal enum used by engine to determine shader standard")]
	public enum ShaderType
	{
		Shape = 0,
		Distance = 1,
		Volume = 2,
		Other = 3
	}


	public enum ShapeType
	{
		Empty = 0,
		Sphere = 1,
		Cube = 2,
		Cylinder = 3,
		Octahedron = 4,
		DoubleCone = 5,
		Star = 6
	}

	public enum ConnectType
	{
		Add = 0,
		Subtract = 1,
		Intersect = 2,
		SmoothAdd = 3
	}

	public enum TextureType
	{
		SolidColor = 0
	}

	public enum TextureConnectType
	{
		Hard = 0,
		Soft = 1
	}


}
