using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;

[ExecuteAlways]
public class RenderEngine : MonoBehaviour
{
    public Camera MainCamera;
    //public ShapeRenderer[] RenderObjects;
    [SerializeField]
    private List<ShapeRenderer> RenderObjects = new List<ShapeRenderer>();

    //setted from outside of engine
    public Vector4 Resolution; //to do: convert this to int only once

    public Material MergeMat;
    public ComputeMaterial[] Prymitives;
    //later make private
    public RenderTexture SceneDistTex;
    public RenderTexture DistTex;
    public RenderTexture CameraPosTex;
    public RenderTexture PosTex;
    public RenderTexture ColTex;
    public RenderTexture SceneColTex; //becomes final result
    public ComputeShader Clear;
    public ComputeShader CameraPreprocessor;
    public ComputeShader ObjectPreprocessor;
    public ComputeShader ObjectCombiner;
    public ComputeShader ColorCombiner;
    private int ClearAllKernel;
    private int Plane2DKernel;
    private int Raymarch3DKernel;
    private int LocalSpaceTransformKernel;
    [SerializeField]
    private int[] CombineKernels;
    private int MaskByDistanceKernel;
    private int ClearByDistanceKernel;

    //general kernels (kernels have same id if their names are the same, so most shaders will use same kernel id's
    private int DistKernel;
    private int VolKernel;

    //Depricated!!
    public Vector4 CameraPosition; //fourth one is field of view and is setted manually
    public Vector4 CameraScale;
    public float CameraRotation; //In deegres

    //values passed to shaders:
    public Vector4[] CameraFrustum = new Vector4[4];

    //only for testing
    public ComputeShader CameraTest;
    public int CameraTestKernel;
    public ComputeShader ErrorObject;
    public int ErrorDistKernel;
    public int ErrorVolKernel;
    public ComputeShader DistOutput;
    public int DistOutputVolKernel;

    //camera preprocessor
    private int FrustumProperty;
    private int ResolutionProperty;
    //object preprocessor
    private int PositionProperty;
    private int RotationProperty;
    private int ScaleProperty;
    //object combiner
    private int FactorProperty;

    //object subscription
    public void RemoveRenderObject(ShapeRenderer renderObject)
	{
        RenderObjects.Remove(renderObject);
	}

    public void AddRenderObjectRelative(ShapeRenderer renderObject)
	{
        //Scene scene = SceneManager.GetActiveScene();

    }

    public void RefreshRenderObjectsQueue()
    {
        RenderObjects.Clear();
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; ++i)
		{
            if (rootObjects[i].activeSelf) //small optimisation, if not active, non of children will be
			{
                ShapeRenderer[] childRenderers = rootObjects[i].GetComponentsInChildren<ShapeRenderer>(includeInactive: false);
                RenderObjects.AddRange(childRenderers);
            }
            
		}
    }

    //yup, because unity cannot easily convert those
    private Vector4 Vect3To4(Vector3 vect3)
    {
        return new Vector4(vect3.x, vect3.y, vect3.z, 0);
    }

    private static int LocateSubstring(string org, string substring)
    {
        if (substring.Length > org.Length)
        {
            return -1;
        }
        int j = 0;
        for (int i = 0; i < org.Length; ++i)
        {
            while (j < substring.Length && i < org.Length && org[i] == substring[j])
            {
                j++;
                i++;
            }
            if (j >= substring.Length) //substring found
            {
                return i;
            }
            j = 0;
        }
        //substring not found
        return -1;
    }

    private void Awake()
	{
        //texture creation
        SceneDistTex = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
        SceneDistTex.format = RenderTextureFormat.RFloat;
        SceneDistTex.enableRandomWrite = true;
        SceneDistTex.Create();

        DistTex = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
        DistTex.format = RenderTextureFormat.RFloat;
        DistTex.enableRandomWrite = true;
        DistTex.Create();

        CameraPosTex = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
        CameraPosTex.format = RenderTextureFormat.ARGBFloat;
        CameraPosTex.enableRandomWrite = true;
        CameraPosTex.Create();

        PosTex = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
        PosTex.format = RenderTextureFormat.ARGBFloat;
        PosTex.enableRandomWrite = true;
        PosTex.Create();

        ColTex = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
        ColTex.format = RenderTextureFormat.ARGBFloat;
        ColTex.enableRandomWrite = true;
        ColTex.Create();

        SceneColTex = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
        SceneColTex.format = RenderTextureFormat.ARGBFloat;
        SceneColTex.enableRandomWrite = true;
        SceneColTex.Create();

        //texture constants setup across all supported shaders (yeah must be done even though all compute shaders are assets not instances)

        //find ALL scriptable objects
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (LocateSubstring(path, "Resources") < 0) //if inside resources folder then ignore
            {
                //if scriptable object can be cast to ComputeMaterial then recompile
                UnityEngine.Object curAsset = AssetDatabase.LoadAllAssetsAtPath(path)[0];
                if (typeof(ComputeMaterial).IsAssignableFrom(curAsset.GetType()))
                {
                    ComputeMaterial curMaterial = (ComputeMaterial)curAsset;
                    ComputeShader shader = curMaterial.shader;
                    if (curMaterial.GetShaderType() == EngineEnums.ShaderType.Shape)
					{
                        shader.SetTexture(curMaterial.GetKernelID(0), "Dist", DistTex);
                        shader.SetTexture(curMaterial.GetKernelID(0), "Pos", PosTex);
                        shader.SetTexture(curMaterial.GetKernelID(1), "Pos", PosTex);
                        shader.SetTexture(curMaterial.GetKernelID(1), "Col", ColTex);
                    }
                    else if (curMaterial.GetShaderType() == EngineEnums.ShaderType.Distance)
					{
                        shader.SetTexture(curMaterial.GetKernelID(0), "Dist", DistTex);
                        shader.SetTexture(curMaterial.GetKernelID(0), "Pos", PosTex);
                    }
                    else if (curMaterial.GetShaderType() == EngineEnums.ShaderType.Volume)
					{
                        shader.SetTexture(curMaterial.GetKernelID(0), "Pos", PosTex);
                        shader.SetTexture(curMaterial.GetKernelID(0), "Col", ColTex);
                    } //else is unsupported
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //auto set camera to main if not set:
        if (MainCamera == null)
		{
            MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (MainCamera == null) //if no "main camera" present take ANY camera
			{
                MainCamera = GameObject.FindFirstObjectByType<Camera>().GetComponent<Camera>();
			}
		}

        //finding constant kernels:
        DistKernel = ErrorObject.FindKernel("dist");
        VolKernel = ErrorObject.FindKernel("vol");
        ClearAllKernel = Clear.FindKernel("ClearAll");
        Plane2DKernel = CameraPreprocessor.FindKernel("Plane2D");
        Raymarch3DKernel = CameraPreprocessor.FindKernel("Raymarch3D");
        LocalSpaceTransformKernel = ObjectPreprocessor.FindKernel("LocalSpaceTransform");
        for (int i = 0; i < CombineKernels.Length; ++i)
		{
            //CombineKernels[i] = ObjectCombiner.FindKernel(EngineEnums.ConnectType.Add.ToString().ToLower());
		}
        string[] connectTypes = Enum.GetNames(typeof(EngineEnums.ConnectType));
        CombineKernels = new int[connectTypes.Length];
        for (int i = 0; i < CombineKernels.Length; ++i)
		{
            CombineKernels[i] = ObjectCombiner.FindKernel(connectTypes[i].ToLower());
        }
        MaskByDistanceKernel = ColorCombiner.FindKernel("MaskByDistance");
        ClearByDistanceKernel = ColorCombiner.FindKernel("ClearByDistance");

        //texture merging for better aliging with unity internal renderer
        MergeMat.SetTexture("_RenderTex", SceneColTex);

        //set all texture references that does not need to be set every frame
        Clear.SetTexture(ClearAllKernel, "Dist", DistTex);
        Clear.SetTexture(ClearAllKernel, "ResDist", SceneDistTex);
        Clear.SetTexture(ClearAllKernel, "Pos", PosTex);
        Clear.SetTexture(ClearAllKernel, "CameraPos", CameraPosTex);
        Clear.SetTexture(ClearAllKernel, "Col", ColTex);
        Clear.SetTexture(ClearAllKernel, "ResCol", SceneColTex);
        ObjectPreprocessor.SetTexture(LocalSpaceTransformKernel, "Pos", CameraPosTex);
        ObjectPreprocessor.SetTexture(LocalSpaceTransformKernel, "ResPos", PosTex);
        for (int i = 0; i < CombineKernels.Length; ++i)
		{
            ObjectCombiner.SetTexture(CombineKernels[i], "Dist", DistTex);
            ObjectCombiner.SetTexture(CombineKernels[i], "ResDist", SceneDistTex);
        }
        ColorCombiner.SetTexture(MaskByDistanceKernel, "Dist", DistTex);
        ColorCombiner.SetTexture(MaskByDistanceKernel, "Col", ColTex);
        ColorCombiner.SetTexture(MaskByDistanceKernel, "ResCol", SceneColTex);

        ColorCombiner.SetTexture(ClearByDistanceKernel, "ResDist", SceneDistTex);
        ColorCombiner.SetTexture(ClearByDistanceKernel, "ResCol", SceneColTex);

        //get id's for shader properties;
        //camera preprocessor
        FrustumProperty = Shader.PropertyToID("Frustum");
        ResolutionProperty = Shader.PropertyToID("Resolution");
        //object preprocessor
        PositionProperty = Shader.PropertyToID("Position");
        RotationProperty = Shader.PropertyToID("Rotation");
        ScaleProperty = Shader.PropertyToID("Scale");
        //object combiner
        FactorProperty = Shader.PropertyToID("Factor");

        //testing
        CameraTestKernel = CameraTest.FindKernel("vol");
        DistOutputVolKernel = DistOutput.FindKernel("vol");
        ErrorDistKernel = ErrorObject.FindKernel("dist");
        ErrorVolKernel = ErrorObject.FindKernel("vol");
        CameraTest.SetTexture(CameraTestKernel, "Pos", CameraPosTex);
        CameraTest.SetTexture(CameraTestKernel, "Col", ColTex);
        DistOutput.SetTexture(DistOutputVolKernel, "Dist", SceneDistTex);
        DistOutput.SetTexture(DistOutputVolKernel, "Col", SceneColTex);
        ErrorObject.SetTexture(ErrorDistKernel, "Dist", DistTex);
        ErrorObject.SetTexture(ErrorDistKernel, "Pos", PosTex);
        ErrorObject.SetTexture(ErrorVolKernel, "Pos", PosTex);
        ErrorObject.SetTexture(ErrorVolKernel, "Col", ColTex);

        //refreshing object queue
        //this is required due to incorrect refreshing when exiting play mode
        RefreshRenderObjectsQueue();
    }

	// Update is called once per frame
	//void OnRenderImage(RenderTexture source, RenderTexture destination)
    void Update()
    {
        //clear last frame
        Clear.Dispatch(ClearAllKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

        //camera setup
        GetCameraData(MainCamera);

        //dispatch camera
        CameraPreprocessor.SetTexture(Plane2DKernel, "Pos", CameraPosTex);
        CameraPreprocessor.SetVectorArray(FrustumProperty, CameraFrustum);
        CameraPreprocessor.SetVector(ResolutionProperty, Resolution);
        CameraPreprocessor.Dispatch(Plane2DKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);


        //output camera position
        //CameraTest.Dispatch(CameraTestKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);
        //*
        //main object loop
        for (int i = 0; i < RenderObjects.Count; ++i)
		{
            ShapeObject objectSpecification = RenderObjects[i].GetComponent<ShapeObject>();
            Transform renderTransform = RenderObjects[i].transform;
            //preprocessObject
            ObjectPreprocessor.SetVector(PositionProperty, Vect3To4(renderTransform.position));
            ObjectPreprocessor.SetVector(RotationProperty, Vect3To4(renderTransform.localEulerAngles));
            ObjectPreprocessor.SetVector(ScaleProperty, Vect3To4(renderTransform.lossyScale));

            ObjectPreprocessor.Dispatch(LocalSpaceTransformKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

            //calculate object shape
            //interpret object specification
            ComputeMaterial curDistMaterial = RenderObjects[i].ShapeMaterial;
            if (curDistMaterial == null) //no set material, check shape object
			{
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.Empty)
				{
                    Prymitives[0].RunProgram(0, Resolution);
				}
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.Sphere)
                {
                    Prymitives[1].RunProgram(0, Resolution);
                }
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.Cube)
                {
                    Prymitives[2].RunProgram(0, Resolution);
                }
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.Cylinder)
                {
                    Prymitives[3].RunProgram(0, Resolution);
                }
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.Octahedron)
                {
                    Prymitives[4].RunProgram(0, Resolution);
                }
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.DoubleCone)
                {
                    Prymitives[5].RunProgram(0, Resolution);
                }
                if (objectSpecification.ShapeType == EngineEnums.ShapeType.Star)
                {
                    Prymitives[6].RunProgram(0, Resolution);
                }
            }
            else if (curDistMaterial.GetShaderType() == EngineEnums.ShaderType.Shape || curDistMaterial.GetShaderType() == EngineEnums.ShaderType.Distance)
			{
                curDistMaterial.RunProgram(0, Resolution);
			}
            else
			{
                //yup, error is internal and not taken from material
                ErrorObject.Dispatch(ErrorDistKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);
            }

            //calculate object volume (actually works across whole screen and then gets discarded afterwards)
            ComputeMaterial curVolMaterial = RenderObjects[i].VolumeMaterial;
            if (curVolMaterial == null) //no specified volume mat, use one from distance
			{
                if (curDistMaterial || curDistMaterial.GetShaderType() == EngineEnums.ShaderType.Shape)
				{
                    curDistMaterial.RunProgram(1, Resolution);
                }
				else //error, vol mat not set
				{
                    ErrorObject.Dispatch(ErrorVolKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);
                }
			}
			else
			{
                curVolMaterial.RunProgram(0, Resolution);
			}

            //CombineObject
            if (objectSpecification.ConnectType == EngineEnums.ConnectType.SmoothAdd)
			{
                ObjectCombiner.SetFloat(FactorProperty, objectSpecification.ConnectFactor);
			}
            ObjectCombiner.Dispatch(CombineKernels[(int)objectSpecification.ConnectType], (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

            //combine colors
            ColorCombiner.Dispatch(MaskByDistanceKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

        }
        //*/

        //at end clear everything that is outside of negative distance field
        ColorCombiner.Dispatch(ClearByDistanceKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);

        //at end return distance as debug
        //DistOutput.Dispatch(DistOutputVolKernel, (int)Resolution.x / 8, (int)Resolution.y / 8, 1);
    }

    void GetCameraData(Camera curCamera)
	{
        Transform camTransform = curCamera.transform;

        //Depricated!!!
        CameraPosition = new Vector4(camTransform.position.x, camTransform.position.y, camTransform.position.z, CameraPosition.w);
        //CameraRotation = new Vector4(curCamera.eulerAngles.x, curCamera.eulerAngles.y, curCamera.eulerAngles.z, 0);
        CameraRotation = camTransform.eulerAngles.y;


        //better camera frustum handling
        Vector3[] frustum = new Vector3[4];

        //must have made it manually
        //because function provided by Unity works only for perspective view AND does not mentions it in documentation!
        frustum[0] = new Vector3(-curCamera.orthographicSize * curCamera.aspect, -curCamera.orthographicSize, curCamera.nearClipPlane);
        frustum[1] = new Vector3(-frustum[0].x, frustum[0].y, curCamera.nearClipPlane);
        frustum[2] = new Vector3(frustum[0].x, -frustum[0].y, curCamera.nearClipPlane);
        frustum[3] = new Vector3(-frustum[0].x, -frustum[0].y, curCamera.nearClipPlane);
        //curCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), curCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustum);
        //local frustum to global conversion
        for (int i = 0; i < 4; ++i)
        {
            frustum[i] = camTransform.TransformPoint(frustum[i]);
        }

        for (int i = 0; i < frustum.Length; ++i)
        {
            CameraFrustum[i] = Vect3To4(frustum[i]);
        }
    }
}
