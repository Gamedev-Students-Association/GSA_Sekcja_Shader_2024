using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : ComputeMaterial
{

[HideInInspector]
public EngineEnums.ShaderType shaderType = EngineEnums.ShaderType.Shape;
[HideInInspector]
public string[] kernelNames = new string[] { "dist", "vol" };
[HideInInspector]
public int[] kernelIDs = new int[2];
private int[] propertyIDs = new int[2];
public Color asds;
public List<float> twojaStara;

	//here all buffers are referenced
	private void OnEnable()
	{
kernelIDs[0] = shader.FindKernel("dist");
kernelIDs[1] = shader.FindKernel("vol");
propertyIDs[0] = Shader.PropertyToID("asds");
propertyIDs[1] = Shader.PropertyToID("twojaStara");
	}

	public override EngineEnums.ShaderType GetShaderType()
	{
		return shaderType;
	}

	public override int GetKernelID(int index)
	{
		return kernelIDs[index];
	}

	public override string[] GetKernelNames()
	{
		return kernelNames;
	}

	public override void RunProgram(int kernel, Vector4 resolution)
	{
shader.SetVector(propertyIDs[0], asds);
shader.SetFloats(propertyIDs[1], twojaStara.ToArray());
shader.Dispatch(kernelIDs[kernel], (int)resolution.x / 8, (int)resolution.y / 8, 1);


	}
}
