using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RayMarchingDatabase : MonoBehaviour
{
	public ComputeShader shader;
	//public Material visualizer;
	//public Camera ResultCamera;
	//public RawImage performanceTex;

	public RenderTexture result;
	public Material mergeMat;
	//public RenderTexture result2;
	//public RenderTexture PerformanceScreen;

	public GameObject SceneObject;

	//DebuggingTools
	public bool TensionMode;
	public Vector4 GraphTransform;
	public float LineThickness;
	public float FormThickness;

	//DataObjects (for now)
	public int ObjectsCap;
	//public float ItterationComplexity;
	//public float DrawQuality;
	public Camera CurrentCamera;

	//RAY MARCHING PROPERTIES
	public Vector4 Resolution;
	public Vector4 CameraPosition; //fourth one is field of view and is setted manually
	public Vector4 CameraScale;
	public float CameraRotation; //In deegres

	//public Vector4[][] ShapeID;

	public List<ShapeObject> Primitives;
	public List<int> PrimitivesID;

	public List<Vector4> ShapePosition;
	public List<Vector4> ShapeRotation;
	public List<Vector4> ShapeScale;

	public List<int> ShapeType;
	public List<int> ConnectType;
	public List<float> ConnectFactor;
	public List<int> TextureType;
	//public List<Color> TextureColor;
	public List<Vector4> TextureColorVector;
	public List<Vector4> TextureScale;

	public List<Vector4> Types;

	public List<int> RepetitionID;
	public List<int> RepetitionLoop;
	public List<int> repetitionType;
	//public List<int>

	private Vector4 Vect3To4(Vector3 vect3)
	{
		return new Vector4(vect3.x, vect3.y, vect3.z, 0);
	}

	void Start()
	{
		result = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
		result.enableRandomWrite = true;
		result.Create();


		//result2 = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
		//result2.enableRandomWrite = true;
		//result2.Create();
		//performanceTex.texture = result2;
	}

	void Update()
	{
		int kernel = shader.FindKernel("RayMarching2D");

		shader.SetTexture(kernel, "Result", result);
		shader.Dispatch(kernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);


		//kernel = shader.FindKernel("RayGraph");

		//shader.SetTexture(kernel, "Result2", result2);
		//shader.Dispatch(kernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

		CollectData();
	}

	void CollectData()
	{
		Transform camTransform = CurrentCamera.transform;

		CameraPosition = new Vector4(camTransform.position.x, camTransform.position.y, camTransform.position.z, CameraPosition.w);
		//CameraRotation = new Vector4(CurrentCamera.eulerAngles.x, CurrentCamera.eulerAngles.y, CurrentCamera.eulerAngles.z, 0);
		CameraRotation = camTransform.eulerAngles.y;

		//newly added texture merging
		mergeMat.SetTexture("_RenderTex", result);

		//better camera frustum handling
		Vector3[] frustum = new Vector3[4];

		//must have made it manually
		//because function provided by Unity works only for perspective view AND does not mentions it in documentation!
		frustum[0] = new Vector3(-CurrentCamera.orthographicSize  * CurrentCamera.aspect, -CurrentCamera.orthographicSize , CurrentCamera.nearClipPlane);
		frustum[1] = new Vector3(-frustum[0].x, frustum[0].y, CurrentCamera.nearClipPlane);
		frustum[2] = new Vector3(frustum[0].x, -frustum[0].y, CurrentCamera.nearClipPlane);
		frustum[3] = new Vector3(-frustum[0].x, -frustum[0].y, CurrentCamera.nearClipPlane);
		//CurrentCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), CurrentCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustum);
		//local frustum to global conversion
		for (int i = 0; i < 4; ++i)
		{
			frustum[i] = camTransform.TransformPoint(frustum[i]);
		}
		
		Vector4[] realFrustum = new Vector4[4];
		for (int i = 0; i < frustum.Length; ++i)
		{
			realFrustum[i] = Vect3To4(frustum[i]);
		}

		//pass frustum to shader:
		shader.SetVectorArray("Frustum", realFrustum);
		Debug.DrawRay(realFrustum[0], new Vector3(6, 0, 0));

		//ClearTheData
		Primitives.Clear();
		PrimitivesID.Clear();

		ShapePosition.Clear();
		ShapeRotation.Clear();
		ShapeScale.Clear();

		ShapeType.Clear();
		ConnectType.Clear();
		ConnectFactor.Clear();
		TextureType.Clear();
		TextureColorVector.Clear();
		TextureScale.Clear();

		Types.Clear();

		ShapeRenderer[] Renderers = SceneObject.GetComponentsInChildren<ShapeRenderer>(includeInactive: false);
		
		for (int i = 0; i < Renderers.Length; i++)
		{
			Primitives.Add(Renderers[i].GetComponent<ShapeObject>());
		}
		//Primitives.AddRange(SceneObject.GetComponentsInChildren<ShapeObject>(includeInactive: false));

		for (int i = 0; i < Primitives.Count; ++i)
		{
			//if outside of view
			//*
			if (Vector3.Distance(Primitives[i].GetComponent<Transform>().position, CameraPosition) - Primitives[i].GetComponent<Transform>().lossyScale.magnitude >= Mathf.Max(CameraScale.x, CameraScale.y) * 1.5)
			{
				Primitives.RemoveAt(i);
				--i;
			}
			//*/
		}
		ObjectsCap = Primitives.Count;

		//PhaseDataToArray()
		for (int i = 0; i < ObjectsCap; i++)
		{

			ShapePosition.Add(Primitives[i].GetComponent<Transform>().position);
			//RotationFix
			Transform[] Rotations = Primitives[i].GetComponentsInParent<Transform>();
			Vector3 FinalRotation = new Vector3(0, 0, 0);
			for (int j = 0; j < Rotations.Length; j++)
			{
				FinalRotation += Rotations[j].localEulerAngles;
			}
			ShapeRotation.Add(FinalRotation);
				ShapeScale.Add(new Vector4(Primitives[i].GetComponent<Transform>().lossyScale.x,
				Primitives[i].GetComponent<Transform>().lossyScale.y,
				Primitives[i].GetComponent<Transform>().lossyScale.z, Primitives[i].Exposure));

			//ShapeScale[i].w = Primitives[i].Exposure;
			//ShapeScale[i] = Vector4(ShapeScale);

			ShapeType.Add((int)Primitives[i].ShapeType);
			ConnectType.Add((int)Primitives[i].ConnectType);
			ConnectFactor.Add(Primitives[i].ConnectFactor);
			//make textures better:
			//TextureType.Add((int)Renderers[i].TextureType);
			//TextureColorVector.Add(Renderers[i].TextureColor);
			//TextureScale.Add(Renderers[i].TextureScale);

			//*
			for (int j = 0; j < 3; j++)
			{
				ShapeType.Add(0);
				ConnectType.Add(0);
				ConnectFactor.Add(0);
				TextureType.Add(0);
			}
			//*/

			Types.Add(new Vector4(ShapeType[i], ConnectType[i], TextureType[i], 0));
		}

		PassToRender();
		//return true;
	}

	void PassToRender()
	{
		shader.SetVector("Resolution", Resolution);

		shader.SetBool("TensionMode", TensionMode);
		//shader.SetVector(, new Vector4(GraphTransform.x, GraphTransform.y, 0, 0));
		shader.SetVector("GraphTransform", GraphTransform);
		shader.SetFloat("LineThickness", LineThickness);
		shader.SetFloat("FormThickness", FormThickness);

		shader.SetInt("ObjectsCap", ObjectsCap);
		//shader.SetFloat("ItterationComplexity", ItterationComplexity);
		//shader.SetFloat("DrawQuality", DrawQuality);

		shader.SetVector("CameraPosition", CameraPosition);
		shader.SetVector("CameraScale", CameraScale);
		shader.SetFloat("CameraRotation", CameraRotation);

		//shader.SetVectorArray("ShapeID", ShapeID);

		shader.SetVectorArray("ShapePosition", ShapePosition.ToArray());
		shader.SetVectorArray("ShapeRotation", ShapeRotation.ToArray());
		shader.SetVectorArray("ShapeScale", ShapeScale.ToArray());

		shader.SetInts("ShapeType", ShapeType.ToArray());
		shader.SetInts("ConnectType", ConnectType.ToArray());
		shader.SetFloats("ConnectFactor", ConnectFactor.ToArray());
		shader.SetInts("TextureType", TextureType.ToArray());
		shader.SetVectorArray("TextureColor", TextureColorVector.ToArray());
		shader.SetVectorArray("TextureScale", TextureScale.ToArray());

		shader.SetVectorArray("Types", Types.ToArray());


		//The only reason it is a bool function is to force the main function to wait until the end of this one
		//return true;
	}
}
