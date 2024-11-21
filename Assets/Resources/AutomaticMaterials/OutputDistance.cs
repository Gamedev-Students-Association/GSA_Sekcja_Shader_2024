using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputDistance : ComputeMaterial
{

[HideInInspector]
public EngineEnums.ShaderType shaderType = EngineEnums.ShaderType.Volume;
[HideInInspector]
public string[] kernelNames = new string[] { "vol" };
[HideInInspector]
public int[] kernelIDs = new int[1];
private int[] propertyIDs = new int[0];

	//here all buffers are referenced
	private void OnEnable()
	{
kernelIDs[0] = shader.FindKernel("vol");
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
shader.Dispatch(kernelIDs[kernel], (int)resolution.x / 8, (int)resolution.y / 8, 1);


	}
}
