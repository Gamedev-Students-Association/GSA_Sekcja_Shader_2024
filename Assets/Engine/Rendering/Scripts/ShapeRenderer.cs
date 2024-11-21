using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//signalizes for render engine that it should
//take the object that has this component into account
//also handels textures and stuff....
[ExecuteAlways]
[RequireComponent(typeof(ShapeObject))]
public class ShapeRenderer : MonoBehaviour
{
	private RenderEngine renderEngine;

	public EngineEnums.TextureConnectType TextureConnectType;

	[Tooltip("uses prymitives if set as null")]
	public ComputeMaterial ShapeMaterial;
	[Tooltip("uses error shader (beautiful magenta) if set as null")]
	public ComputeMaterial VolumeMaterial;

	//used by engine debugger
	//for now like that since c# does not contains "friend" mechanism from c++, awaiting ideas for better security
	[HideInInspector]
	public GameObject preview;

	private void Awake()
	{
		renderEngine = GameObject.FindFirstObjectByType<RenderEngine>();
	}

	private void OnEnable()
	{
		//subscribe to rendered objects
		renderEngine.RefreshRenderObjectsQueue();
	}

	private void OnDisable()
	{
		//unsubscribe from rendered objects
		//renderEngine.RefreshRenderObjectsQueue();
		renderEngine.RemoveRenderObject(this);
	}

	private void OnDestroy()
	{
		//unsubscribe from rendered objects
		//renderEngine.RefreshRenderObjectsQueue();
		renderEngine.RemoveRenderObject(this);
	}

	private void OnTransformParentChanged()
	{
		renderEngine.RefreshRenderObjectsQueue();
	}
}
