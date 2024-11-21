using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//* replace class name and inheritance
[CreateAssetMenu(fileName = "NewComputeMaterial", menuName = "ComputeMaterial")]
public class ComputeMaterial : ScriptableObject
{
    public ComputeShader shader;

	public virtual EngineEnums.ShaderType GetShaderType()
	{
		return EngineEnums.ShaderType.Other;
	}

	public virtual int GetKernelID(int index)
	{
		return -1;
	}

	public virtual string[] GetKernelNames()
	{
		return null;
	}

	public virtual void RunProgram(int kernel, Vector4 resolution)
	{
		//* first sets all variables

		
	}
}
