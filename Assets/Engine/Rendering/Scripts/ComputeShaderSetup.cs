using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ComputeShaderSetup : MonoBehaviour
{

	public ComputeShader shader;
	public Material visualizer;
	public Camera ResultCamera;
	public RawImage visualizeTex;

	public RenderTexture result;

	public Vector4 Resolution;

    void Start()
    {
		
    }

    void Update()
    {
		int kernel = shader.FindKernel("CSMain");

		result = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
		result.enableRandomWrite = true;
		result.Create();

		shader.SetTexture(kernel, "Result", result);
		shader.Dispatch(kernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

		//visualizer.SetTexture("_MainTex", result);
		//ResultCamera.targetTexture
		visualizeTex.texture = result;

		PassToShader();
	}

	void PassToShader()
	{
		shader.SetVector("Resolution", Resolution);
	}
}
