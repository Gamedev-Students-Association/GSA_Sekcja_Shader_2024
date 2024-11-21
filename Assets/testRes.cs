using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testRes : ComputeMaterial
{
public string[] kernelNames = new string[] { "dist", "vol" };
public int[] kernelIDs = new int[2];
private int[] propertyIDs = new int[2];
public Color asds;
public List<float> twojaStara;

	//here all buffers are referenced
	private void Start()
	{
kernelIDs[0] = shader.FindKernel("dist");
kernelIDs[1] = shader.FindKernel("vol");
propertyIDs[0] = Shader.PropertyToID("asds");
propertyIDs[1] = Shader.PropertyToID("twojaStara");
	}

	public override void RunProgram(int kernel, Vector4 resolution)
	{
shader.SetVector(propertyIDs[0], asds);
shader.SetFloats(propertyIDs[1], twojaStara.ToArray());
shader.Dispatch( kernelIDs[kernel], (int)resolution.x / 8, (int)resolution.y / 8, 1);


	}
}
