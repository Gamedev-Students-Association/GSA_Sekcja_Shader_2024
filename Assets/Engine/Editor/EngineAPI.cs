using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

//this script serves as a bridge between custom engine part and default unity editor
//it is a source of most quality of life and debugging
public class EngineAPI : MonoBehaviour
{

	//for some unknown reason "Hierarchy" tab is not called hierarchy, it is called "GameObject" seriously, naming it like that places it in hierarchy

	//general debugging functions:


	//new objects addition
	//loads prefabs from assets and initialize them
	[MenuItem("GameObject/2dw3d/Empty3d")]
	private static void Empty3d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Empty"));
	}

	[MenuItem("GameObject/2dw3d/Cube3d")]
   private static void Cube3d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Cube"));
	}

	[MenuItem("GameObject/2dw3d/Sphere3d")]
	private static void Sphere3d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Sphere"));
	}

	[MenuItem("GameObject/2dw3d/Cylinder3d")]
	private static void Cylinder3d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Cylinder"));
	}

	[MenuItem("GameObject/2dw3d/Octahedron3d")]
	private static void Octahedron3d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Octahedron"));
	}

	[MenuItem("GameObject/2dw3d/Cone3d")]
	private static void Cone3d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Cone"));
	}

	[MenuItem("GameObject/2dw3d/Empty25d")]
	private static void Empty25d()
	{
		GameObject.Instantiate(Resources.Load("Primitives/Empty25d"));
	}

	[MenuItem("2dw3d/Generate3dPreviews")]
	private static void Generate3dPreview()
	{
		ShapeRenderer[] realObjects = GameObject.FindObjectsOfType<ShapeRenderer>();

		//load preview meshes into memory
		MeshFilter[] previewMeshes = ((GameObject)Resources.Load("Primitives/ShapesPreviews")).GetComponentsInChildren<MeshFilter>();

		for (int i = 0; i < realObjects.Length; ++i)
		{
			//if not already having preview
			if (!realObjects[i].preview)
			{
				realObjects[i].preview = (GameObject)GameObject.Instantiate(Resources.Load("Engine/Rendering/FakeRenderer"), realObjects[i].transform);
				//set correct preview shape
				if (realObjects[i].GetComponent<ShapeObject>().ShapeType != EngineEnums.ShapeType.Empty)
				{
					realObjects[i].preview.GetComponent<MeshFilter>().mesh = previewMeshes[(int)realObjects[i].GetComponent<ShapeObject>().ShapeType].sharedMesh;
				}
				
			}

		}
	}

	[MenuItem("2dw3d/Remove3dPreviews")]
	private static void Remove3dPreview()
	{
		ShapeRenderer[] realObjects = GameObject.FindObjectsOfType<ShapeRenderer>();

		for (int i = 0; i < realObjects.Length; ++i)
		{
			if (realObjects[i].preview)
			{
				GameObject.DestroyImmediate(realObjects[i].preview);
				//destroyed is missing not null
				realObjects[i].preview = null;
			}

		}
	}


	//------------------------------------------------
	//METAPROGRAMMING


	//unity post that makes it like asset creation in unity
	//(totally not copied from the post)
	//https://discussions.unity.com/t/how-to-implement-create-new-asset/761565
	//WARNING! Reference path must be from inside "resources" folder
	private static void CopyFile(string referencePath, string extension, string desiredName)
	{
		UnityEngine.Object target = Selection.activeObject;
		string path = AssetDatabase.GetAssetPath(target);
		string folder = File.Exists(path) ? Path.GetDirectoryName(path) : path;

		// Get all existing files of matching type in target folder
		List<string> existingNames = new List<string>();
		foreach (string p in Directory.GetFiles(folder, "*" + extension, SearchOption.TopDirectoryOnly))
			existingNames.Add(Path.GetFileNameWithoutExtension(p));

		//generate unique name for duplicate creations
		string uniqueName = ObjectNames.GetUniqueName(existingNames.ToArray(), desiredName);
		string outputPath = Path.Combine(folder, uniqueName + extension).Replace("\\", "/");

		//File.WriteAllText(outputPath, textAsset.text);
		File.WriteAllText(outputPath, File.ReadAllText("Assets/" + referencePath + extension));

		AssetDatabase.ImportAsset(outputPath);
	}

	//SHADER CREATIONS
	[MenuItem("Assets/Create/Shader/ShapeShader")]
	private static void CreateShapeShader()
	{
		CopyFile("Resources/Rendering/ShapeShaderStandard", ".compute", "NewShapeShader");
	}
	
	[MenuItem("Assets/Create/Shader/DistanceShader")]
	private static void CreateDistanceShader()
	{
		CopyFile("Resources/Rendering/DistanceShaderStandard", ".compute", "NewDistanceShader");
	}

	[MenuItem("Assets/Create/Shader/VolumeShader")]
	private static void CreateVolumeShader()
	{
		CopyFile("Resources/Rendering/VolumeShaderStandard", ".compute", "NewVolumeShader");
	}

	//how to create instances of scriptable objects
	//https://discussions.unity.com/t/how-to-create-a-scriptableobject-file-with-specific-path-through-code/239303

	//https://discussions.unity.com/t/loading-a-file-with-a-custom-extension-as-a-textasset/731294/4

	//----------------------------------------------
	//material creation

	private static int LocateSubstring(string org, string substring)
	{
		if (substring.Length > org.Length)
		{
			return -1;
		}
		int j = 0;
		for (int i = 0; i < org.Length; ++i)
		{
			while(j < substring.Length && i < org.Length && org[i] == substring[j])
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

	private static string GetNextWord(string org, int begin, char separator)
	{
		StringBuilder result = new StringBuilder(); //why...
		int i = begin;
		while (i < org.Length && org[i] == separator) //ignore multiple separators
		{
			i++;
		}
		for (; i < org.Length; ++i)
		{
			if (org[i] == separator)
			{
				break;
			}
			result.Append(org[i]);
		}

		return result.ToString();
	}

	private static void GenerateComputeMaterial(string shaderPath, string outputPath, string desiredName)
	{
		List<string> kernelNames = new List<string>();
		List<string> propertyTypes = new List<string>();
		List<bool> propertyArray = new List<bool>();
		List<string> propertyNames = new List<string>();

		//file reading loop (data extraction)
		foreach(string line in File.ReadLines(shaderPath))
		{
			//Debug.Log(line);
			int lineIndex = 0;
			//register kernel
			lineIndex = LocateSubstring(line, "kernel ");
			if (lineIndex >= 0) //kernel exists in line
			{
				kernelNames.Add(GetNextWord(line, lineIndex, ' '));
				//Debug.Log(GetNextWord(line, lineIndex, ' '));
			}
			//register property
			lineIndex = LocateSubstring(line, "uniform ");
			if (lineIndex >= 0) //property exists in line
			{
				//add support for texture properties!!!!
				string propertyType = GetNextWord(line, lineIndex, ' ');
				//types check:
				if (LocateSubstring(propertyType, "vector") >= 0)
				{
					//because no one wants to set color in vector form
					if (LocateSubstring(line, "//color") >= 0)
					{
						propertyTypes.Add("Color");
					}
					else
					{
						propertyTypes.Add("Vector4");
					}

				}
				else if (LocateSubstring(propertyType, "float") >= 0)
				{
					propertyTypes.Add("float");
				}
				else if (LocateSubstring(propertyType, "int") >= 0)
				{
					propertyTypes.Add("int");
				}
				lineIndex = lineIndex + propertyType.Length;
				string propertyName = GetNextWord(line, lineIndex, ' ');

				//array or not?
				if (propertyName[propertyName.Length - 2] == ']')
				{
					propertyName = GetNextWord(propertyName, 0, '[');
					propertyArray.Add(true);
					//Debug.Log(" array");
				}
				else
				{
					propertyName = GetNextWord(propertyName, 0, ';');
					propertyArray.Add(false);
					//Debug.Log("not array");
				}
				propertyNames.Add(propertyName);
			}

			//other things if needed in future
		}

		//----------------------------------------------------------
		//FILE CREATION

		//original file to copy is hardcoded (and there is no reason for it to be different)
		StringBuilder resultContent = new StringBuilder();
		IEnumerator<string> lines = File.ReadLines("Assets/Resources/AutomaticMaterials/Presets/AutomationBase.txt").GetEnumerator();
		lines.MoveNext(); //for some reason first element is null

		while(LocateSubstring(lines.Current, "//*") < 0) //copy all lines that are not marked
		{
			resultContent.Append(lines.Current);
			resultContent.Append(Environment.NewLine);
			lines.MoveNext();
		}
		lines.MoveNext();
		//first mark, class name and inheritance
		lines.MoveNext(); //we skip original declaration line from template file
		resultContent.Append("public class " + desiredName + " : " + "ComputeMaterial");
		resultContent.Append(Environment.NewLine);

		while (LocateSubstring(lines.Current, "//*") < 0) //copy all lines that are not marked
		{
			resultContent.Append(lines.Current);
			resultContent.Append(Environment.NewLine);
			lines.MoveNext();
		}
		lines.MoveNext();
		//second mark, kernel and properties declaration

		//shader type used to determine if shader is standard or not
		resultContent.Append("[HideInInspector]");
		resultContent.Append(Environment.NewLine);
		resultContent.Append("public EngineEnums.ShaderType shaderType = EngineEnums.ShaderType.");
		if (kernelNames.Count > 0)
		{
			if (LocateSubstring(kernelNames[0], "dist") >= 0)
			{
				if (kernelNames.Count > 1 && LocateSubstring(kernelNames[1], "vol") >= 0)
				{
					resultContent.Append("Shape;");
				}
				else
				{
					resultContent.Append("Distance;");
				}
			}
			else if (LocateSubstring(kernelNames[0], "vol") >= 0)
			{
				resultContent.Append("Volume;");
			}
			else
			{
				resultContent.Append("Other;");
			}
		}
		else
		{
			resultContent.Append("Other;");
		}
		resultContent.Append(Environment.NewLine);

		resultContent.Append("[HideInInspector]");
		resultContent.Append(Environment.NewLine);
		resultContent.Append("public string[] kernelNames = new string[] { ");
		for (int i = 0; i < kernelNames.Count; i++) //cast kernel names fully static
		{
			resultContent.Append("\"" + kernelNames[i] + "\"");
			if (i < kernelNames.Count - 1)
			{
				resultContent.Append(", ");
			}
		}
		resultContent.Append(" };");
		resultContent.Append(Environment.NewLine);
		resultContent.Append("[HideInInspector]");
		resultContent.Append(Environment.NewLine);
		resultContent.Append("public int[] kernelIDs = new int[" + kernelNames.Count + "];");
		resultContent.Append(Environment.NewLine);

		resultContent.Append("private int[] propertyIDs = new int[" + propertyNames.Count + "];");
		resultContent.Append(Environment.NewLine);

		for (int i = 0; i < propertyNames.Count; i++)
		{
			if (!propertyArray[i])
			{
				resultContent.Append("public " + propertyTypes[i] + " " + propertyNames[i] + ";");
			}
			else
			{
				resultContent.Append("public " + "List<" + propertyTypes[i] + "> " + propertyNames[i] + ";");
			}
			resultContent.Append(Environment.NewLine);
		}

		while (LocateSubstring(lines.Current, "//*") < 0) //copy all lines that are not marked
		{
			resultContent.Append(lines.Current);
			resultContent.Append(Environment.NewLine);
			lines.MoveNext();
		}
		lines.MoveNext();
		//third mark, start function
		for (int i = 0; i < kernelNames.Count; i++)
		{
			resultContent.Append("kernelIDs[" + i.ToString() + "] = shader.FindKernel(\"" + kernelNames[i] + "\");");
			resultContent.Append(Environment.NewLine);
		}

		for (int i = 0; i < propertyNames.Count; i++)
		{
			resultContent.Append("propertyIDs[" + i.ToString() + "] = Shader.PropertyToID(\"" + propertyNames[i] + "\");");
			resultContent.Append(Environment.NewLine);
		}

		while (LocateSubstring(lines.Current, "//*") < 0) //copy all lines that are not marked
		{
			resultContent.Append(lines.Current);
			resultContent.Append(Environment.NewLine);
			lines.MoveNext();
		}
		lines.MoveNext();
		//4 mark, run program (set values)
		for (int i = 0; i < propertyNames.Count; i++)
		{
			//hmmm maybe it should be an int?
			if (LocateSubstring(propertyTypes[i], "Vector4") >= 0 || LocateSubstring(propertyTypes[i], "Color") >= 0)
			{
				if (propertyArray[i])
				{
					resultContent.Append("shader.SetVectorArray(propertyIDs[" + i.ToString() + "], " + propertyNames[i] + ".ToArray());");
					resultContent.Append(Environment.NewLine);
				}
				else
				{
					resultContent.Append("shader.SetVector(propertyIDs[" + i.ToString() + "], " + propertyNames[i] + ");");
					resultContent.Append(Environment.NewLine);
				}
				
			}
			else if (LocateSubstring(propertyTypes[i], "float") >= 0)
			{
				if (propertyArray[i])
				{
					resultContent.Append("shader.SetFloats(propertyIDs[" + i.ToString() + "], " + propertyNames[i] + ".ToArray());");
					resultContent.Append(Environment.NewLine);
				}
				else
				{
					resultContent.Append("shader.SetFloat(propertyIDs[" + i.ToString() + "], " + propertyNames[i] + ");");
					resultContent.Append(Environment.NewLine);
				}
			}
			else if (LocateSubstring(propertyTypes[i], "int") >= 0)
			{
				if (propertyArray[i])
				{
					resultContent.Append("shader.SetInts(propertyIDs[" + i.ToString() + "], " + propertyNames[i] + ".ToArray());");
					resultContent.Append(Environment.NewLine);
				}
				else
				{
					resultContent.Append("shader.SetInt(propertyIDs[" + i.ToString() + "], " + propertyNames[i] + ");");
					resultContent.Append(Environment.NewLine);
				}
			}
			
		}
		//dispatch
		//for now 8 everywhere make to correspond to shader later
		resultContent.Append("shader.Dispatch(kernelIDs[kernel], (int)resolution.x / 8, (int)resolution.y / 8, 1);");
		resultContent.Append(Environment.NewLine);

		//copy rest of the file
		while (lines.Current != null)
		{
			resultContent.Append(lines.Current);
			resultContent.Append(Environment.NewLine);
			lines.MoveNext();
		}

		//save to new file
		outputPath = outputPath + "/" + desiredName + ".cs";
		File.WriteAllText(outputPath, resultContent.ToString());
		AssetDatabase.ImportAsset(outputPath);
	}

	private static void CompileComputeMaterial(UnityEngine.Object target)
	{
		string orgPath = AssetDatabase.GetAssetPath(target);

		//first get shader of material
		if (((ComputeMaterial)target).shader == null)
		{
			Debug.LogError("Cannot recompile material with null shader!" + Environment.NewLine + "Material: \"" + target.name + "\" at path:" + Environment.NewLine + "\"" + orgPath + "\"");
			return;
		}
		ComputeShader shader = ((ComputeMaterial)target).shader;
		//find material class reference of selected shader
		string[] guids = AssetDatabase.FindAssets(shader.name, new string[] { "Assets/Resources/AutomaticMaterials" });
		if (guids.Length <= 0)
		{
			Debug.LogError("Cannot find material class for selected shader: \"" + shader.name + "\"" + Environment.NewLine + "while recompiling material: \"" + target.name + "\"" + Environment.NewLine + "Make sure to bake all shaders before compiling materials");
			return;
		}

		//open meta file for associated scriptable instance
		StringBuilder result = new StringBuilder();
		foreach (string line in File.ReadLines(orgPath))
		{
			//replace guid only in referenced script
			if (LocateSubstring(line, "m_Script:") >= 0)
			{
				int guidLocation = LocateSubstring(line, "guid: ");
				result.Append(line.Substring(0, guidLocation));
				result.Append(guids[0]); //should always find only one file
				result.Append(line.Substring(LocateSubstring(line, "type:") - 7)); //i know, -7 isn't good if unity ever changes the convention for writing scriptable object data, but f* this, gonna find this long ass comment if something goes terribly wrong
			}
			else
			{
				result.Append(line);
			}
			result.Append(Environment.NewLine);
		}

		//write over and reimport asset
		File.WriteAllText(orgPath, result.ToString());
		AssetDatabase.ImportAsset(orgPath);

		//"repair" bug that sets wrong shader after recompiling material
		ComputeMaterial mat = (ComputeMaterial)AssetDatabase.LoadAllAssetsAtPath(orgPath)[0];
		mat.shader = shader;
	}

	[MenuItem("Assets/RecompileMaterial")]
	private static void RecomplieSingleComputeMaterial()
	{
		CompileComputeMaterial(Selection.activeObject);
	}

	[MenuItem("Assets/RecompileMaterial", true)]
	static bool ValidateRecompileMaterial()
	{
		//if (Selection.activeObject.GetType().Equals(typeof(ComputeMaterial)))
		if (typeof(ComputeMaterial).IsAssignableFrom(Selection.activeObject.GetType()))
		{
			return true;
		}
		return false;
	}

	[MenuItem("Assets/RecompileAllMaterials")]
	private static void RecompileAllComputeMaterials()
	{
		//find ALL scriptable objects
		string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (LocateSubstring(path, "Resources") < 0) //if inside resources folder then ignore
			{
				//if scriptable object can be cast to ComputeMaterial then recompile
				UnityEngine.Object curMaterial = AssetDatabase.LoadAllAssetsAtPath(path)[0];
				if (typeof(ComputeMaterial).IsAssignableFrom(curMaterial.GetType()))
				{
					CompileComputeMaterial(curMaterial);
				}
			}
		}

	}

	[MenuItem("Assets/BakeComputeMaterial")]
	private static void BakeComputeMaterial()
	{
		UnityEngine.Object target = Selection.activeObject;
		string path = AssetDatabase.GetAssetPath(target);

		GenerateComputeMaterial(path, "Assets/Resources/AutomaticMaterials", target.name);
	}

	[MenuItem("Assets/BakeComputeMaterial", true)]
	static bool ValidateBakeComputeMaterial()
	{
		if (Selection.activeObject.GetType().Equals(typeof(ComputeShader)))
		{
			return true;
		}
		return false;
	}

	private static string GetFileNameFromPath(string path)
	{
		StringBuilder result = new StringBuilder();
		int i = path.Length - 1;
		while (path[i] != '.') //is operating system dependant
		{
			--i;
		}
		//file name end located
		int end = i;
		while (path[i] != '/') //is operating system dependant
		{
			--i;
		}
		++i;
		//file name start located
		while(i < end)
		{
			result.Append(path[i]);
			++i;
		}

		return result.ToString();
	}

	[MenuItem("Assets/BakeAllComputeMaterials")]
	static void BakeAllComputeMaterials ()
	{
		//find all compute materials excluding resources folder:
		string[] guids = AssetDatabase.FindAssets("t:ComputeShader", null);
		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (LocateSubstring(path, "Resources") < 0) //if inside resources folder then ignore
			{
				GenerateComputeMaterial(path, "Assets/Resources/AutomaticMaterials", GetFileNameFromPath(path));
			}
		}

		Debug.Log("Succesfully baked " + guids.Length + " Materials");
	}
}
