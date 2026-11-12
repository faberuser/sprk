using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using LitJson;
using NGame2.NUI;
using NGameAsset.NEffect;
using Spine;
using Spine.Unity;
using UnityEngine;

public sealed class SprkRecoveredLoginScene : IDisposable
{
	private readonly Dictionary<string, UnityEngine.Object> objects = new Dictionary<string, UnityEngine.Object>();

	private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

	private readonly Dictionary<string, Type> types = new Dictionary<string, Type>();

	private readonly Dictionary<TextAsset, byte[]> textBytes = new Dictionary<TextAsset, byte[]>();

	private readonly Dictionary<Texture2D, string> textureFiles = new Dictionary<Texture2D, string>();

	private readonly HashSet<Texture2D> rgbOnly = new HashSet<Texture2D>();

	private readonly HashSet<Texture2D> exactWrap = new HashSet<Texture2D>();

	private readonly Dictionary<Texture2D, Texture2D> colorAlpha = new Dictionary<Texture2D, Texture2D>();

	private readonly Dictionary<string, Texture2D> combinedAlpha = new Dictionary<string, Texture2D>();

	private const string EffectShader = "Vespa/Effect2/NormalNoZ";

	private readonly string folder;

	private GameObject bank;

	private string stage = "start";

	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public GameObject Root { get; private set; }

	private static bool Has(JsonData data, string key)
	{
		if (data != null && data.IsObject())
		{
			return data.Contains(key);
		}
		return false;
	}

	private static string Text(JsonData data, string key, string fallback = "")
	{
		if (!Has(data, key) || data[key] == null)
		{
			return fallback;
		}
		return data[key].valueStr;
	}

	private static float Number(JsonData data, string key, float fallback = 0f)
	{
		if (!Has(data, key) || data[key] == null)
		{
			return fallback;
		}
		JsonData jsonData = data[key];
		if (jsonData.IsObject())
		{
			if (Has(jsonData, "value"))
			{
				return Number(jsonData, "value", fallback);
			}
			if (Has(jsonData, "scalar"))
			{
				return Number(jsonData, "scalar", fallback);
			}
			return fallback;
		}
		if (jsonData.IsBoolean())
		{
			return jsonData.valueBool ? 1 : 0;
		}
		return float.Parse(jsonData.valueStr, CultureInfo.InvariantCulture);
	}

	private static bool Bool(JsonData data, string key, bool fallback = false)
	{
		if (!Has(data, key))
		{
			return fallback;
		}
		return data[key].valueBool;
	}

	private static bool Flag(JsonData data, string key, bool fallback = false)
	{
		if (!Has(data, key) || data[key] == null)
		{
			return fallback;
		}
		JsonData jsonData = data[key];
		if (!jsonData.IsBoolean())
		{
			return Number(data, key) != 0f;
		}
		return jsonData.valueBool;
	}

	private Type FindType(string name)
	{
		if (types.TryGetValue(name, out var value))
		{
			return value;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			value = assemblies[i].GetType(name, throwOnError: false);
			if (value != null)
			{
				break;
			}
		}
		if (value == null)
		{
			throw new InvalidOperationException("Unknown recovered scene type: " + name);
		}
		types[name] = value;
		return value;
	}

	private UnityEngine.Object Ref(JsonData value)
	{
		if (value == null || !Has(value, "ref") || !objects.TryGetValue(value["ref"].valueStr, out var value2))
		{
			return null;
		}
		return value2;
	}

	private GameObject Go(JsonData value)
	{
		return Ref(value) as GameObject;
	}

	private T Keep<T>(T item) where T : UnityEngine.Object
	{
		owned.Add(item);
		return item;
	}

	public SprkRecoveredLoginScene(string path, Transform parent)
	{
		folder = Path.GetDirectoryName(path);
		bank = new GameObject("SPRK recovered scene assets");
		bank.transform.SetParent(parent, worldPositionStays: false);
		bank.SetActive(value: false);
		try
		{
			Build(JsonMapper.ToObject(File.ReadAllText(path)), parent);
		}
		catch (Exception innerException)
		{
			Dispose();
			throw new InvalidOperationException("Recovered scene " + Path.GetFileName(Path.GetDirectoryName(path)) + " at " + stage, innerException);
		}
	}

	private static FieldInfo Field(Type type, string name)
	{
		while (type != null)
		{
			FieldInfo field = type.GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field;
			}
			type = type.BaseType;
		}
		return null;
	}

	private void Fill(object target, JsonData data)
	{
		if (data == null || !data.IsObject())
		{
			return;
		}
		foreach (KeyValuePair<string, JsonData> item in data.ToDictionary())
		{
			string key = item.Key;
			JsonData value = item.Value;
			switch (key)
			{
			case "m_Name":
			case "m_Enabled":
			case "m_Script":
			case "m_GameObject":
				continue;
			}
			FieldInfo fieldInfo = Field(target.GetType(), key);
			if (fieldInfo != null && !fieldInfo.IsInitOnly)
			{
				fieldInfo.SetValue(target, ConvertValue(value, fieldInfo.FieldType));
			}
		}
		Behaviour behaviour = target as Behaviour;
		if (behaviour != null && Has(data, "m_Enabled"))
		{
			behaviour.enabled = Number(data, "m_Enabled", 1f) != 0f;
		}
	}

	private object ConvertValue(JsonData value, Type type)
	{
		if (value == null)
		{
			if (!type.IsValueType)
			{
				return null;
			}
			return Activator.CreateInstance(type);
		}
		if (Has(value, "ref"))
		{
			return Ref(value);
		}
		if (type == typeof(string))
		{
			if (!value.IsString())
			{
				return value.ToJson();
			}
			return value.valueStr;
		}
		if (type.IsEnum)
		{
			return Enum.ToObject(type, (int)NumberValue(value));
		}
		if (type == typeof(bool))
		{
			return value.IsBoolean() ? value.valueBool : (NumberValue(value) != 0.0);
		}
		if (type.IsPrimitive || type == typeof(decimal))
		{
			return Convert.ChangeType(NumberValue(value), type, CultureInfo.InvariantCulture);
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			int num = (value.IsArray() ? value.Count : 0);
			Array array = Array.CreateInstance(elementType, num);
			for (int i = 0; i < num; i++)
			{
				array.SetValue(ConvertValue(value.GetFromArray(i), elementType), i);
			}
			return array;
		}
		if (typeof(IList).IsAssignableFrom(type))
		{
			IList list = (IList)Activator.CreateInstance(type);
			Type type2 = type.GetGenericArguments()[0];
			if (value.IsArray())
			{
				for (int j = 0; j < value.Count; j++)
				{
					list.Add(ConvertValue(value.GetFromArray(j), type2));
				}
			}
			return list;
		}
		object obj;
		try
		{
			obj = Activator.CreateInstance(type, nonPublic: true);
		}
		catch
		{
			obj = FormatterServices.GetUninitializedObject(type);
		}
		Fill(obj, value);
		return obj;
	}

	private static double NumberValue(JsonData value)
	{
		return double.Parse(value.valueStr, CultureInfo.InvariantCulture);
	}

	private Vector3 Vector(JsonData value)
	{
		return new Vector3(Number(value, "x"), Number(value, "y"), Number(value, "z"));
	}

	private Quaternion Rotation(JsonData value)
	{
		return new Quaternion(Number(value, "x"), Number(value, "y"), Number(value, "z"), Number(value, "w", 1f));
	}

	private static bool AdditiveEffect(JsonData row)
	{
		if (Text(row, "shader") == "Sprites/Default" && Has(row, "floats") && Has(row["floats"], "_BlendModeSrc"))
		{
			return Number(row["floats"], "_BlendModeDst") == 1f;
		}
		return false;
	}

	private static Shader FindShader(string name)
	{
		string[] array = new string[4]
		{
			name,
			name.Replace(" - ", "/"),
			"Vespa/Effect2/" + name,
			"Vespa/" + name
		};
		for (int i = 0; i < array.Length; i++)
		{
			Shader shader = Shader.Find(array[i]);
			if (shader != null)
			{
				return shader;
			}
		}
		return null;
	}

	private Material Material(JsonData row)
	{
		string text = Text(row, "shader");
		if (AdditiveEffect(row) && Shader.Find("Vespa/Effect2/NormalNoZ") != null)
		{
			text = "Vespa/Effect2/NormalNoZ";
		}
		Shader shader = FindShader(text);
		if (shader == null || text.IndexOf("SplitAlpha", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			shader = Shader.Find("Sprites/Default");
		}
		if (shader == null)
		{
			shader = Shader.Find("Sprites/Default");
		}
		if (shader == null)
		{
			throw new InvalidOperationException("Login rendering shader unavailable");
		}
		Material material = Keep(new Material(shader)
		{
			name = Text(row, "name")
		});
		if (Has(row, "keywords") && shader.name != "Sprites/Default")
		{
			string[] array = (string[])ConvertValue(row["keywords"], typeof(string[]));
			if (text != Text(row, "shader"))
			{
				array = array.Select((string k) => (!(k == "ALPHATEX_ON")) ? k : "ALPHATEX_OFF").ToArray();
			}
			material.shaderKeywords = array;
		}
		int num = (int)Number(row, "renderQueue", -1f);
		if (num >= 0)
		{
			material.renderQueue = num;
		}
		return material;
	}

	private Texture2D Readable(Texture2D source)
	{
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
		if (!texture2D.LoadImage(File.ReadAllBytes(textureFiles[source]), markNonReadable: false))
		{
			throw new InvalidOperationException("Login texture failed: " + source.name);
		}
		return texture2D;
	}

	private Texture2D CombineAlpha(Texture2D main, Texture2D alpha)
	{
		string key = ((main == null) ? "white" : main.GetInstanceID().ToString()) + "/" + alpha.GetInstanceID();
		if (combinedAlpha.TryGetValue(key, out var value))
		{
			return value;
		}
		Texture2D texture2D = Readable(alpha);
		Texture2D texture2D2 = ((main != null && textureFiles.ContainsKey(main)) ? Readable(main) : null);
		int width = texture2D.width;
		int height = texture2D.height;
		Color32[] pixels = texture2D.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		Color32[] array2 = ((texture2D2 != null && texture2D2.width == width && texture2D2.height == height) ? texture2D2.GetPixels32() : null);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int num = i * width + j;
				Color32 color = ((array2 != null) ? array2[num] : ((texture2D2 != null) ? ((Color32)texture2D2.GetPixelBilinear(((float)j + 0.5f) / (float)width, ((float)i + 0.5f) / (float)height)) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
				color.a = pixels[num].r;
				array[num] = color;
			}
		}
		UnityEngine.Object.Destroy(texture2D);
		if (texture2D2 != null)
		{
			UnityEngine.Object.Destroy(texture2D2);
		}
		value = Keep(new Texture2D(width, height, TextureFormat.RGBA32, mipChain: true));
		value.name = alpha.name + " (alpha)";
		value.SetPixels32(array);
		value.wrapModeU = alpha.wrapModeU;
		value.wrapModeV = alpha.wrapModeV;
		value.filterMode = FilterMode.Trilinear;
		value.anisoLevel = 2;
		value.Apply(updateMipmaps: true, makeNoLongerReadable: true);
		combinedAlpha[key] = value;
		return value;
	}

	private Texture2D AlphaFromColor(Texture2D source)
	{
		if (colorAlpha.TryGetValue(source, out var value))
		{
			return value;
		}
		Texture2D texture2D = Readable(source);
		Color32[] pixels = texture2D.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			color.a = Math.Max(color.r, Math.Max(color.g, color.b));
			pixels[i] = color;
		}
		value = Keep(new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, mipChain: true));
		value.name = source.name + " (alpha)";
		value.SetPixels32(pixels);
		value.wrapModeU = source.wrapModeU;
		value.wrapModeV = source.wrapModeV;
		value.filterMode = FilterMode.Trilinear;
		value.anisoLevel = 2;
		value.Apply(updateMipmaps: true, makeNoLongerReadable: true);
		UnityEngine.Object.Destroy(texture2D);
		colorAlpha[source] = value;
		return value;
	}

	private void FixEffectTextures(Material material, JsonData row)
	{
		string name = material.shader.name;
		if (Has(row, "floats") && row["floats"].ToDictionary().Any((KeyValuePair<string, JsonData> p) => p.Key.Contains("Speed") && NumberValue(p.Value) != 0.0) && name.StartsWith("Vespa/") && name.IndexOf("Clamp", StringComparison.OrdinalIgnoreCase) < 0 && name != "Vespa/Effect2/NormalNoZ")
		{
			string[] texturePropertyNames = material.GetTexturePropertyNames();
			foreach (string name2 in texturePropertyNames)
			{
				Texture2D texture2D = material.GetTexture(name2) as Texture2D;
				if (texture2D != null && !exactWrap.Contains(texture2D))
				{
					texture2D.wrapMode = TextureWrapMode.Repeat;
				}
			}
		}
		bool num2 = name.StartsWith("Vespa/") && !name.StartsWith("Vespa/NGUI");
		bool flag = name.IndexOf("Addtive", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Additive", StringComparison.OrdinalIgnoreCase) >= 0 || (Has(row, "floats") && Has(row["floats"], "_BlendModeDst") && Number(row["floats"], "_BlendModeDst") == 1f);
		if (num2 && !flag && material.HasProperty("_MainTex") && !material.IsKeywordEnabled("ALPHATEX_ON"))
		{
			Texture2D texture2D2 = material.GetTexture("_MainTex") as Texture2D;
			if (texture2D2 != null && rgbOnly.Contains(texture2D2))
			{
				material.SetTexture("_MainTex", AlphaFromColor(texture2D2));
			}
		}
		if (material.IsKeywordEnabled("ALPHATEX_ON") && material.HasProperty("_AlphaTex"))
		{
			Texture2D texture2D3 = material.GetTexture("_AlphaTex") as Texture2D;
			if (texture2D3 != null && textureFiles.ContainsKey(texture2D3))
			{
				Texture2D texture2D4 = (material.HasProperty("_MainTex") ? (material.GetTexture("_MainTex") as Texture2D) : null);
				Vector2 value = ((texture2D4 != null) ? material.GetTextureScale("_MainTex") : material.GetTextureScale("_AlphaTex"));
				Vector2 value2 = ((texture2D4 != null) ? material.GetTextureOffset("_MainTex") : material.GetTextureOffset("_AlphaTex"));
				material.SetTexture("_MainTex", CombineAlpha(texture2D4, texture2D3));
				material.SetTextureScale("_MainTex", value);
				material.SetTextureOffset("_MainTex", value2);
				material.SetTexture("_AlphaTex", null);
				material.DisableKeyword("ALPHATEX_ON");
				material.EnableKeyword("ALPHATEX_OFF");
			}
		}
	}

	private void Build(JsonData scene, Transform parent)
	{
		JsonData jsonData = scene["objects"];
		for (int i = 0; i < jsonData.Count; i++)
		{
			JsonData fromArray = jsonData.GetFromArray(i);
			string text = Text(fromArray, "id");
			string text2 = Text(fromArray, "kind");
			UnityEngine.Object obj = null;
			stage = "create " + text2 + " " + text;
			switch (text2)
			{
			case "GameObject":
			{
				GameObject gameObject = new GameObject(Text(fromArray, "name"));
				gameObject.transform.SetParent(bank.transform, worldPositionStays: false);
				gameObject.layer = parent.gameObject.layer;
				obj = gameObject;
				break;
			}
			case "Texture2D":
			{
				Texture2D texture2D = Keep(new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true));
				texture2D.name = Text(fromArray, "name");
				string text3 = Path.Combine(folder, Text(fromArray, "file"));
				if (!texture2D.LoadImage(File.ReadAllBytes(text3), markNonReadable: true))
				{
					throw new InvalidOperationException("Login texture failed: " + texture2D.name);
				}
				texture2D.wrapMode = TextureWrapMode.Clamp;
				texture2D.filterMode = FilterMode.Trilinear;
				texture2D.anisoLevel = 2;
				textureFiles[texture2D] = text3;
				obj = texture2D;
				if (Has(fromArray, "wrapU"))
				{
					texture2D.wrapModeU = (TextureWrapMode)Number(fromArray, "wrapU");
					texture2D.wrapModeV = (TextureWrapMode)Number(fromArray, "wrapV", Number(fromArray, "wrapU"));
					exactWrap.Add(texture2D);
				}
				if (Flag(fromArray, "rgbOnly"))
				{
					rgbOnly.Add(texture2D);
				}
				break;
			}
			case "TextAsset":
			{
				byte[] array = File.ReadAllBytes(Path.Combine(folder, Text(fromArray, "file")));
				TextAsset textAsset = Keep(new TextAsset(Encoding.UTF8.GetString(array))
				{
					name = Text(fromArray, "name")
				});
				textBytes[textAsset] = array;
				obj = textAsset;
				break;
			}
			case "Material":
				obj = Material(fromArray);
				break;
			case "Mesh":
				obj = Keep(new Mesh
				{
					name = Text(fromArray, "name")
				});
				break;
			case "AnimationClip":
				obj = Keep(new AnimationClip
				{
					legacy = true,
					name = Text(fromArray["data"], "m_Name")
				});
				break;
			}
			if (obj != null)
			{
				objects[text] = obj;
			}
		}
		for (int j = 0; j < jsonData.Count; j++)
		{
			JsonData fromArray2 = jsonData.GetFromArray(j);
			if (Text(fromArray2, "kind") == "Transform")
			{
				objects[Text(fromArray2, "id")] = Go(fromArray2["go"]).transform;
			}
		}
		for (int k = 0; k < jsonData.Count; k++)
		{
			JsonData fromArray3 = jsonData.GetFromArray(k);
			string text4 = Text(fromArray3, "kind");
			string text5 = Text(fromArray3, "id");
			stage = "component " + text4 + " " + text5;
			object obj2;
			switch (text4)
			{
			case "MonoBehaviour":
			{
				Type type = FindType(Text(fromArray3, "type"));
				if (typeof(ScriptableObject).IsAssignableFrom(type))
				{
					objects[text5] = Keep(ScriptableObject.CreateInstance(type));
					continue;
				}
				GameObject gameObject2 = Go(fromArray3["go"]);
				if (gameObject2 != null)
				{
					objects[text5] = gameObject2.GetComponent(type) ?? gameObject2.AddComponent(type);
				}
				continue;
			}
			default:
				obj2 = null;
				break;
			case "Animation":
				obj2 = typeof(UnityEngine.Animation);
				break;
			case "ParticleSystemRenderer":
				obj2 = typeof(ParticleSystemRenderer);
				break;
			case "ParticleSystem":
				obj2 = typeof(ParticleSystem);
				break;
			case "MeshFilter":
				obj2 = typeof(MeshFilter);
				break;
			case "MeshRenderer":
				obj2 = typeof(MeshRenderer);
				break;
			}
			Type type2 = (Type)obj2;
			if (type2 != null)
			{
				GameObject gameObject3 = Go(fromArray3["go"]);
				if (gameObject3 != null)
				{
					objects[text5] = gameObject3.GetComponent(type2) ?? gameObject3.AddComponent(type2);
				}
			}
		}
		for (int l = 0; l < jsonData.Count; l++)
		{
			JsonData fromArray4 = jsonData.GetFromArray(l);
			string text6 = Text(fromArray4, "id");
			string text7 = Text(fromArray4, "kind");
			if (!objects.TryGetValue(text6, out var value))
			{
				continue;
			}
			stage = "fill " + text7 + " " + text6;
			switch (text7)
			{
			case "Transform":
			{
				Transform obj4 = (Transform)value;
				Transform transform = Ref(fromArray4["parent"]) as Transform;
				obj4.SetParent((transform != null) ? transform : bank.transform, worldPositionStays: false);
				obj4.localPosition = Vector(fromArray4["position"]);
				obj4.localRotation = Rotation(fromArray4["rotation"]);
				obj4.localScale = Vector(fromArray4["scale"]);
				break;
			}
			case "MonoBehaviour":
				Fill(value, fromArray4["data"]);
				break;
			case "Material":
			{
				Material material = (Material)value;
				foreach (KeyValuePair<string, JsonData> item in fromArray4["textures"].ToDictionary())
				{
					JsonData value2 = item.Value;
					Texture texture = Ref(value2["m_Texture"]) as Texture;
					if (texture != null)
					{
						material.SetTexture(item.Key, texture);
						material.SetTextureOffset(item.Key, new Vector2(Number(value2["m_Offset"], "x"), Number(value2["m_Offset"], "y")));
						material.SetTextureScale(item.Key, new Vector2(Number(value2["m_Scale"], "x", 1f), Number(value2["m_Scale"], "y", 1f)));
					}
				}
				foreach (KeyValuePair<string, JsonData> item2 in fromArray4["floats"].ToDictionary())
				{
					if (material.HasProperty(item2.Key))
					{
						material.SetFloat(item2.Key, (float)NumberValue(item2.Value));
					}
				}
				string[] array5 = new string[2] { "Src", "Dst" };
				foreach (string text8 in array5)
				{
					if (Has(fromArray4["floats"], "_BlendMode" + text8) && material.HasProperty("_" + text8 + "Blend"))
					{
						material.SetFloat("_" + text8 + "Blend", Number(fromArray4["floats"], "_BlendMode" + text8));
					}
				}
				if (Text(fromArray4, "shader").Contains("NoZ") && material.HasProperty("_ZWrite"))
				{
					material.SetFloat("_ZWrite", 0f);
				}
				foreach (KeyValuePair<string, JsonData> item3 in fromArray4["colors"].ToDictionary())
				{
					JsonData value3 = item3.Value;
					if (material.HasProperty(item3.Key))
					{
						material.SetColor(item3.Key, new Color(Number(value3, "r"), Number(value3, "g"), Number(value3, "b"), Number(value3, "a", 1f)));
					}
				}
				FixEffectTextures(material, fromArray4);
				break;
			}
			case "Mesh":
			{
				Mesh mesh = (Mesh)value;
				JsonData jsonData3 = fromArray4["vertices"];
				Vector3[] array2 = new Vector3[jsonData3.Count / 3];
				for (int m = 0; m < array2.Length; m++)
				{
					array2[m] = new Vector3((float)NumberValue(jsonData3.GetFromArray(m * 3)), (float)NumberValue(jsonData3.GetFromArray(m * 3 + 1)), (float)NumberValue(jsonData3.GetFromArray(m * 3 + 2)));
				}
				mesh.vertices = array2;
				JsonData jsonData4 = (Has(fromArray4, "colors") ? fromArray4["colors"] : null);
				if (jsonData4 != null && jsonData4.Count == array2.Length * 4)
				{
					Color[] array3 = new Color[array2.Length];
					for (int n = 0; n < array3.Length; n++)
					{
						array3[n] = new Color((float)NumberValue(jsonData4.GetFromArray(n * 4)), (float)NumberValue(jsonData4.GetFromArray(n * 4 + 1)), (float)NumberValue(jsonData4.GetFromArray(n * 4 + 2)), (float)NumberValue(jsonData4.GetFromArray(n * 4 + 3)));
					}
					mesh.colors = array3;
				}
				else
				{
					mesh.colors = Enumerable.Repeat(Color.white, array2.Length).ToArray();
				}
				JsonData jsonData5 = fromArray4["uv"];
				Vector2[] array4 = new Vector2[jsonData5.Count / 2];
				for (int num = 0; num < array4.Length; num++)
				{
					array4[num] = new Vector2((float)NumberValue(jsonData5.GetFromArray(num * 2)), (float)NumberValue(jsonData5.GetFromArray(num * 2 + 1)));
				}
				if (array4.Length == array2.Length)
				{
					mesh.uv = array4;
				}
				mesh.subMeshCount = fromArray4["triangles"].Count;
				for (int num2 = 0; num2 < mesh.subMeshCount; num2++)
				{
					mesh.SetTriangles((int[])ConvertValue(fromArray4["triangles"].GetFromArray(num2), typeof(int[])), num2);
				}
				mesh.RecalculateBounds();
				break;
			}
			case "MeshFilter":
				((MeshFilter)value).sharedMesh = Ref(fromArray4["data"]["m_Mesh"]) as Mesh;
				break;
			case "AnimationClip":
				ConfigureAnimation((AnimationClip)value, fromArray4["data"]);
				break;
			case "MeshRenderer":
			case "ParticleSystemRenderer":
			{
				Renderer obj3 = (Renderer)value;
				JsonData jsonData2 = fromArray4["data"];
				obj3.sharedMaterials = (Material[])ConvertValue(jsonData2["m_Materials"], typeof(Material[]));
				obj3.sortingOrder = (int)Number(jsonData2, "m_SortingOrder");
				ParticleSystemRenderer particleSystemRenderer = obj3 as ParticleSystemRenderer;
				if (particleSystemRenderer != null)
				{
					ConfigureParticleRenderer(particleSystemRenderer, jsonData2);
				}
				break;
			}
			case "ParticleSystem":
				ConfigureParticle((ParticleSystem)value, fromArray4["data"]);
				break;
			}
		}
		for (int num4 = 0; num4 < jsonData.Count; num4++)
		{
			JsonData fromArray5 = jsonData.GetFromArray(num4);
			if (Text(fromArray5, "kind") != "Animation")
			{
				continue;
			}
			UnityEngine.Animation animation = (UnityEngine.Animation)objects[Text(fromArray5, "id")];
			JsonData jsonData6 = fromArray5["data"];
			for (int num5 = 0; num5 < jsonData6["m_Animations"].Count; num5++)
			{
				AnimationClip animationClip = Ref(jsonData6["m_Animations"].GetFromArray(num5)) as AnimationClip;
				if (animationClip != null)
				{
					animation.AddClip(animationClip, animationClip.name);
				}
			}
			animation.clip = Ref(jsonData6["m_Animation"]) as AnimationClip;
			animation.playAutomatically = Bool(jsonData6, "m_PlayAutomatically", fallback: true);
			animation.wrapMode = (WrapMode)Number(jsonData6, "m_WrapMode");
		}
		foreach (SkeletonDataAsset item4 in objects.Values.OfType<SkeletonDataAsset>())
		{
			stage = "Spine " + item4.name;
			if (!(item4.skeletonJSON == null) && item4.skeletonJSON.name.ToLowerInvariant().Contains(".skel"))
			{
				SkeletonData skeletonData = new SkeletonBinary(new AtlasAttachmentLoader(item4.atlasAssets.Select((AtlasAssetBase a) => a.GetAtlas()).ToArray()))
				{
					Scale = item4.scale
				}.ReadSkeletonData(new MemoryStream(textBytes[item4.skeletonJSON], writable: false));
				typeof(SkeletonDataAsset).GetMethod("InitializeWithData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(item4, new object[1] { skeletonData });
			}
		}
		foreach (SkeletonAnimation item5 in objects.Values.OfType<SkeletonAnimation>())
		{
			if (item5.skeletonDataAsset == null)
			{
				item5.enabled = false;
			}
		}
		foreach (UIPanel item6 in objects.Values.OfType<UIPanel>())
		{
			item6.depth -= 1000;
		}
		for (int num6 = 0; num6 < jsonData.Count; num6++)
		{
			if (Text(jsonData.GetFromArray(num6), "kind") == "GameObject")
			{
				((GameObject)objects[Text(jsonData.GetFromArray(num6), "id")]).SetActive(Bool(jsonData.GetFromArray(num6), "active", fallback: true) && !Bool(jsonData.GetFromArray(num6), "hidden"));
			}
		}
		Root = Go(scene["root"]);
		if (Has(scene, "attachments"))
		{
			for (int num7 = 0; num7 < scene["attachments"].Count; num7++)
			{
				GameObject gameObject4 = Go(scene["attachments"].GetFromArray(num7));
				if (gameObject4 != null)
				{
					if (!gameObject4.transform.IsChildOf(Root.transform))
					{
						gameObject4.transform.SetParent(Root.transform, worldPositionStays: false);
					}
					gameObject4.SetActive(value: true);
				}
			}
		}
		Root.transform.SetParent(parent, worldPositionStays: false);
		Root.SetActive(value: true);
		UIGame3DWidgetFromRes[] componentsInChildren = Root.GetComponentsInChildren<UIGame3DWidgetFromRes>(includeInactive: true);
		foreach (UIGame3DWidgetFromRes uIGame3DWidgetFromRes in componentsInChildren)
		{
			if (uIGame3DWidgetFromRes.EntryList.Any((UIGame3DWidgetFromRes.EntryData e) => e.ResourceObject is OpenDataFromResource && (((OpenDataFromResource)e.ResourceObject).Code ?? "").EndsWith("Bi", StringComparison.OrdinalIgnoreCase)))
			{
				uIGame3DWidgetFromRes.gameObject.SetActive(value: false);
			}
			else if (uIGame3DWidgetFromRes.gameObject.activeInHierarchy && uIGame3DWidgetFromRes.GetComponent<UIGame3DWidgetFromResAutoPlayer>() == null)
			{
				uIGame3DWidgetFromRes.Play(null);
			}
		}
		Root.AddComponent<SprkRecoveredLoginPhase>();
	}

	private static float CurveValue(JsonData data, string key, float fallback)
	{
		if (!Has(data, key))
		{
			return fallback;
		}
		JsonData data2 = data[key];
		if (!Has(data2, "scalar"))
		{
			return fallback;
		}
		return Number(data2, "scalar", fallback);
	}

	private static void ConfigureAnimation(AnimationClip clip, JsonData data)
	{
		clip.frameRate = Number(data, "m_SampleRate", 30f);
		clip.wrapMode = (WrapMode)Number(data, "m_WrapMode");
		string[][] array = new string[4][]
		{
			new string[3] { "m_PositionCurves", "localPosition", "xyz" },
			new string[3] { "m_ScaleCurves", "localScale", "xyz" },
			new string[3] { "m_RotationCurves", "localRotation", "xyzw" },
			new string[3] { "m_EulerCurves", "localEulerAnglesRaw", "xyz" }
		};
		foreach (string[] array2 in array)
		{
			if (!Has(data, array2[0]))
			{
				continue;
			}
			JsonData jsonData = data[array2[0]];
			for (int j = 0; j < jsonData.Count; j++)
			{
				JsonData fromArray = jsonData.GetFromArray(j);
				string text = array2[2];
				for (int k = 0; k < text.Length; k++)
				{
					char c = text[k];
					AnimationCurve animationCurve = new AnimationCurve();
					JsonData jsonData2 = fromArray["curve"]["m_Curve"];
					for (int l = 0; l < jsonData2.Count; l++)
					{
						JsonData fromArray2 = jsonData2.GetFromArray(l);
						animationCurve.AddKey(new Keyframe(Number(fromArray2, "time"), Number(fromArray2["value"], c.ToString()), Number(fromArray2["inSlope"], c.ToString()), Number(fromArray2["outSlope"], c.ToString())));
					}
					clip.SetCurve(Text(fromArray, "path"), typeof(Transform), array2[1] + "." + c, animationCurve);
				}
			}
		}
		clip.EnsureQuaternionContinuity();
	}

	private static AnimationCurve Curve(JsonData data)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		if (!Has(data, "m_Curve"))
		{
			return animationCurve;
		}
		for (int i = 0; i < data["m_Curve"].Count; i++)
		{
			JsonData fromArray = data["m_Curve"].GetFromArray(i);
			animationCurve.AddKey(new Keyframe(Number(fromArray, "time"), Number(fromArray, "value"), Number(fromArray, "inSlope"), Number(fromArray, "outSlope")));
		}
		return animationCurve;
	}

	private static ParticleSystem.MinMaxCurve MinMax(JsonData data)
	{
		if (data == null)
		{
			return new ParticleSystem.MinMaxCurve(0f);
		}
		if (!data.IsObject())
		{
			return new ParticleSystem.MinMaxCurve(data.IsBoolean() ? ((float)(data.valueBool ? 1 : 0)) : ((float)NumberValue(data)));
		}
		int num = (int)Number(data, "minMaxState");
		float num2 = Number(data, "scalar", 1f);
		float min = Number(data, "minScalar", num2);
		return num switch
		{
			1 => new ParticleSystem.MinMaxCurve(num2, Curve(data["maxCurve"])), 
			2 => new ParticleSystem.MinMaxCurve(num2, Curve(data["minCurve"]), Curve(data["maxCurve"])), 
			3 => new ParticleSystem.MinMaxCurve(min, num2), 
			_ => new ParticleSystem.MinMaxCurve(num2), 
		};
	}

	private static Color ReadColor(JsonData data)
	{
		return new Color(Number(data, "r", 1f), Number(data, "g", 1f), Number(data, "b", 1f), Number(data, "a", 1f));
	}

	private static Gradient ReadGradient(JsonData data)
	{
		GradientColorKey[] array = new GradientColorKey[(int)Number(data, "m_NumColorKeys", 2f)];
		GradientAlphaKey[] array2 = new GradientAlphaKey[(int)Number(data, "m_NumAlphaKeys", 2f)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new GradientColorKey(ReadColor(data["key" + i]), Number(data, "ctime" + i) / 65535f);
		}
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = new GradientAlphaKey(Number(data["key" + j], "a", 1f), Number(data, "atime" + j) / 65535f);
		}
		Gradient gradient = new Gradient();
		gradient.SetKeys(array, array2);
		return gradient;
	}

	private static ParticleSystem.MinMaxGradient ReadMinMaxGradient(JsonData data)
	{
		int num = (int)Number(data, "minMaxState");
		switch (num)
		{
		case 1:
		case 4:
		{
			ParticleSystem.MinMaxGradient result = new ParticleSystem.MinMaxGradient(ReadGradient(data["maxGradient"]));
			result.mode = (ParticleSystemGradientMode)num;
			return result;
		}
		case 2:
			return new ParticleSystem.MinMaxGradient(ReadColor(data["minColor"]), ReadColor(data["maxColor"]));
		case 3:
			return new ParticleSystem.MinMaxGradient(ReadGradient(data["minGradient"]), ReadGradient(data["maxGradient"]));
		default:
			return new ParticleSystem.MinMaxGradient(ReadColor(data["maxColor"]));
		}
	}

	private static Vector3 Vector3Of(JsonData data, string key, float fallback = 0f)
	{
		if (!Has(data, key) || data[key] == null)
		{
			return new Vector3(fallback, fallback, fallback);
		}
		return new Vector3(Number(data[key], "x", fallback), Number(data[key], "y", fallback), Number(data[key], "z", fallback));
	}

	private static JsonData Enabled(JsonData data, string name)
	{
		if (!Has(data, name) || !Flag(data[name], "enabled"))
		{
			return null;
		}
		return data[name];
	}

	private static ParticleSystemSimulationSpace Space(JsonData data, string key)
	{
		if (!Flag(data, key))
		{
			return ParticleSystemSimulationSpace.Local;
		}
		return ParticleSystemSimulationSpace.World;
	}

	private void ConfigureParticle(ParticleSystem system, JsonData data)
	{
		system.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		ParticleSystem.MainModule main = system.main;
		main.duration = Mathf.Max(0.05f, Number(data, "lengthInSec", 5f));
		main.loop = Flag(data, "looping", fallback: true);
		main.prewarm = Flag(data, "prewarm");
		main.playOnAwake = true;
		main.simulationSpeed = (Has(data, "simulationSpeed") ? Number(data, "simulationSpeed", 1f) : Number(data, "speed", 1f));
		main.useUnscaledTime = Flag(data, "useUnscaledTime");
		main.scalingMode = (ParticleSystemScalingMode)Number(data, "scalingMode", 1f);
		main.simulationSpace = (((int)Number(data, "moveWithTransform") == 1) ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local);
		if (Has(data, "startDelay"))
		{
			main.startDelay = MinMax(data["startDelay"]);
		}
		if (Has(data, "autoRandomSeed") && !Flag(data, "autoRandomSeed", fallback: true))
		{
			system.useAutoRandomSeed = false;
			system.randomSeed = (uint)Number(data, "randomSeed");
		}
		if (Has(data, "InitialModule"))
		{
			JsonData jsonData = data["InitialModule"];
			main.startLifetime = MinMax(jsonData["startLifetime"]);
			main.startSpeed = MinMax(jsonData["startSpeed"]);
			main.startColor = ReadMinMaxGradient(jsonData["startColor"]);
			main.startSize3D = Flag(jsonData, "size3D");
			if (main.startSize3D)
			{
				main.startSizeX = MinMax(jsonData["startSize"]);
				main.startSizeY = MinMax(jsonData["startSizeY"]);
				main.startSizeZ = MinMax(jsonData["startSizeZ"]);
			}
			else
			{
				main.startSize = MinMax(jsonData["startSize"]);
			}
			main.startRotation3D = Flag(jsonData, "rotation3D");
			if (main.startRotation3D)
			{
				main.startRotationX = MinMax(jsonData["startRotationX"]);
				main.startRotationY = MinMax(jsonData["startRotationY"]);
				main.startRotationZ = MinMax(jsonData["startRotation"]);
			}
			else
			{
				main.startRotation = MinMax(jsonData["startRotation"]);
			}
			main.flipRotation = Number(jsonData, "randomizeRotationDirection");
			if (Has(jsonData, "gravityModifier"))
			{
				main.gravityModifier = MinMax(jsonData["gravityModifier"]);
			}
			main.maxParticles = (int)Number(jsonData, "maxNumParticles", 1000f);
		}
		ParticleSystem.EmissionModule emission = system.emission;
		JsonData jsonData2 = (Has(data, "EmissionModule") ? data["EmissionModule"] : null);
		emission.enabled = jsonData2 != null && Flag(jsonData2, "enabled", fallback: true);
		if (jsonData2 != null)
		{
			if (Has(jsonData2, "rateOverTime"))
			{
				emission.rateOverTime = MinMax(jsonData2["rateOverTime"]);
			}
			else
			{
				emission.rateOverTime = CurveValue(jsonData2, "rate", 10f);
			}
			if (Has(jsonData2, "rateOverDistance"))
			{
				emission.rateOverDistance = MinMax(jsonData2["rateOverDistance"]);
			}
			List<ParticleSystem.Burst> list = new List<ParticleSystem.Burst>();
			if (Has(jsonData2, "m_Bursts") && jsonData2["m_Bursts"] != null)
			{
				for (int i = 0; i < jsonData2["m_Bursts"].Count; i++)
				{
					JsonData fromArray = jsonData2["m_Bursts"].GetFromArray(i);
					ParticleSystem.Burst item = new ParticleSystem.Burst(Number(fromArray, "time"), MinMax(fromArray["countCurve"]), (int)Number(fromArray, "cycleCount", 1f), Mathf.Max(0.0001f, Number(fromArray, "repeatInterval", 0.01f)));
					item.probability = Number(fromArray, "probability", 1f);
					list.Add(item);
				}
			}
			emission.SetBursts(list.ToArray());
		}
		ParticleSystem.ShapeModule shape = system.shape;
		JsonData jsonData3 = (Has(data, "ShapeModule") ? data["ShapeModule"] : null);
		shape.enabled = jsonData3 != null && Flag(jsonData3, "enabled");
		if (shape.enabled)
		{
			JsonData jsonData4 = jsonData3;
			shape.shapeType = (ParticleSystemShapeType)Number(jsonData4, "type");
			shape.angle = Number(jsonData4, "angle", 25f);
			shape.length = Number(jsonData4, "length", 5f);
			if (Has(jsonData4, "radius") && jsonData4["radius"].IsObject())
			{
				shape.radius = Number(jsonData4["radius"], "value", 1f);
				shape.radiusMode = (ParticleSystemShapeMultiModeValue)Number(jsonData4["radius"], "mode");
				shape.radiusSpread = Number(jsonData4["radius"], "spread");
			}
			else
			{
				shape.radius = Number(jsonData4, "radius", 1f);
			}
			if (Has(jsonData4, "arc") && jsonData4["arc"].IsObject())
			{
				shape.arc = Number(jsonData4["arc"], "value", 360f);
				shape.arcMode = (ParticleSystemShapeMultiModeValue)Number(jsonData4["arc"], "mode");
				shape.arcSpread = Number(jsonData4["arc"], "spread");
			}
			else
			{
				shape.arc = Number(jsonData4, "arc", 360f);
			}
			shape.radiusThickness = Number(jsonData4, "radiusThickness", 1f);
			shape.donutRadius = Number(jsonData4, "donutRadius", 0.2f);
			shape.boxThickness = Vector3Of(jsonData4, "boxThickness");
			shape.position = Vector3Of(jsonData4, "m_Position");
			shape.rotation = Vector3Of(jsonData4, "m_Rotation");
			shape.scale = (Has(jsonData4, "m_Scale") ? Vector3Of(jsonData4, "m_Scale", 1f) : new Vector3(Number(jsonData4, "boxX", 1f), Number(jsonData4, "boxY", 1f), Number(jsonData4, "boxZ", 1f)));
			shape.alignToDirection = Flag(jsonData4, "alignToDirection");
			shape.randomDirectionAmount = (Has(jsonData4, "randomDirectionAmount") ? Number(jsonData4, "randomDirectionAmount") : ((float)(Flag(jsonData4, "randomDirection") ? 1 : 0)));
			shape.sphericalDirectionAmount = Number(jsonData4, "sphericalDirectionAmount");
			shape.randomPositionAmount = Number(jsonData4, "randomPositionAmount");
		}
		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
		JsonData jsonData5 = Enabled(data, "ColorModule");
		colorOverLifetime.enabled = jsonData5 != null;
		if (jsonData5 != null)
		{
			colorOverLifetime.color = ReadMinMaxGradient(jsonData5["gradient"]);
		}
		ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;
		JsonData jsonData6 = Enabled(data, "SizeModule");
		sizeOverLifetime.enabled = jsonData6 != null;
		if (jsonData6 != null)
		{
			sizeOverLifetime.separateAxes = Flag(jsonData6, "separateAxes");
			if (sizeOverLifetime.separateAxes)
			{
				sizeOverLifetime.x = MinMax(jsonData6["curve"]);
				sizeOverLifetime.y = MinMax(jsonData6["y"]);
				sizeOverLifetime.z = MinMax(jsonData6["z"]);
			}
			else
			{
				sizeOverLifetime.size = MinMax(jsonData6["curve"]);
			}
		}
		ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = system.rotationOverLifetime;
		JsonData jsonData7 = Enabled(data, "RotationModule");
		rotationOverLifetime.enabled = jsonData7 != null;
		if (jsonData7 != null)
		{
			rotationOverLifetime.separateAxes = Flag(jsonData7, "separateAxes");
			if (rotationOverLifetime.separateAxes)
			{
				rotationOverLifetime.x = MinMax(jsonData7["x"]);
				rotationOverLifetime.y = MinMax(jsonData7["y"]);
			}
			rotationOverLifetime.z = MinMax(jsonData7["curve"]);
		}
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = system.velocityOverLifetime;
		JsonData jsonData8 = Enabled(data, "VelocityModule");
		velocityOverLifetime.enabled = jsonData8 != null;
		if (jsonData8 != null)
		{
			velocityOverLifetime.space = Space(jsonData8, "inWorldSpace");
			velocityOverLifetime.x = MinMax(jsonData8["x"]);
			velocityOverLifetime.y = MinMax(jsonData8["y"]);
			velocityOverLifetime.z = MinMax(jsonData8["z"]);
			if (Has(jsonData8, "orbitalX"))
			{
				velocityOverLifetime.orbitalX = MinMax(jsonData8["orbitalX"]);
				velocityOverLifetime.orbitalY = MinMax(jsonData8["orbitalY"]);
				velocityOverLifetime.orbitalZ = MinMax(jsonData8["orbitalZ"]);
			}
			if (Has(jsonData8, "orbitalOffsetX"))
			{
				velocityOverLifetime.orbitalOffsetX = MinMax(jsonData8["orbitalOffsetX"]);
				velocityOverLifetime.orbitalOffsetY = MinMax(jsonData8["orbitalOffsetY"]);
				velocityOverLifetime.orbitalOffsetZ = MinMax(jsonData8["orbitalOffsetZ"]);
			}
			if (Has(jsonData8, "radial"))
			{
				velocityOverLifetime.radial = MinMax(jsonData8["radial"]);
			}
			if (Has(jsonData8, "speedModifier"))
			{
				velocityOverLifetime.speedModifier = MinMax(jsonData8["speedModifier"]);
			}
		}
		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = system.limitVelocityOverLifetime;
		JsonData jsonData9 = Enabled(data, "ClampVelocityModule");
		limitVelocityOverLifetime.enabled = jsonData9 != null;
		if (jsonData9 != null)
		{
			limitVelocityOverLifetime.separateAxes = Flag(jsonData9, "separateAxis");
			limitVelocityOverLifetime.space = Space(jsonData9, "inWorldSpace");
			if (limitVelocityOverLifetime.separateAxes)
			{
				limitVelocityOverLifetime.limitX = MinMax(jsonData9["x"]);
				limitVelocityOverLifetime.limitY = MinMax(jsonData9["y"]);
				limitVelocityOverLifetime.limitZ = MinMax(jsonData9["z"]);
			}
			else
			{
				limitVelocityOverLifetime.limit = MinMax(jsonData9["magnitude"]);
			}
			limitVelocityOverLifetime.dampen = Number(jsonData9, "dampen");
			if (Has(jsonData9, "drag"))
			{
				limitVelocityOverLifetime.drag = MinMax(jsonData9["drag"]);
			}
			limitVelocityOverLifetime.multiplyDragByParticleSize = Flag(jsonData9, "multiplyDragByParticleSize", fallback: true);
			limitVelocityOverLifetime.multiplyDragByParticleVelocity = Flag(jsonData9, "multiplyDragByParticleVelocity", fallback: true);
		}
		ParticleSystem.ForceOverLifetimeModule forceOverLifetime = system.forceOverLifetime;
		JsonData jsonData10 = Enabled(data, "ForceModule");
		forceOverLifetime.enabled = jsonData10 != null;
		if (jsonData10 != null)
		{
			forceOverLifetime.space = Space(jsonData10, "inWorldSpace");
			forceOverLifetime.x = MinMax(jsonData10["x"]);
			forceOverLifetime.y = MinMax(jsonData10["y"]);
			forceOverLifetime.z = MinMax(jsonData10["z"]);
			forceOverLifetime.randomized = Flag(jsonData10, "randomizePerFrame");
		}
		ParticleSystem.NoiseModule noise = system.noise;
		JsonData jsonData11 = Enabled(data, "NoiseModule");
		noise.enabled = jsonData11 != null;
		if (jsonData11 != null)
		{
			noise.separateAxes = Flag(jsonData11, "separateAxes");
			if (noise.separateAxes)
			{
				noise.strengthX = MinMax(jsonData11["strength"]);
				noise.strengthY = MinMax(jsonData11["strengthY"]);
				noise.strengthZ = MinMax(jsonData11["strengthZ"]);
			}
			else
			{
				noise.strength = MinMax(jsonData11["strength"]);
			}
			noise.frequency = Number(jsonData11, "frequency", 0.5f);
			noise.damping = Flag(jsonData11, "damping", fallback: true);
			noise.octaveCount = (int)Number(jsonData11, "octaves", 1f);
			noise.octaveMultiplier = Number(jsonData11, "octaveMultiplier", 0.5f);
			noise.octaveScale = Number(jsonData11, "octaveScale", 2f);
			noise.quality = (ParticleSystemNoiseQuality)Number(jsonData11, "quality", 2f);
			if (Has(jsonData11, "scrollSpeed"))
			{
				noise.scrollSpeed = MinMax(jsonData11["scrollSpeed"]);
			}
			noise.remapEnabled = Flag(jsonData11, "remapEnabled");
			if (noise.remapEnabled)
			{
				noise.remap = MinMax(jsonData11["remap"]);
				noise.remapY = MinMax(jsonData11["remapY"]);
				noise.remapZ = MinMax(jsonData11["remapZ"]);
			}
			if (Has(jsonData11, "positionAmount"))
			{
				noise.positionAmount = MinMax(jsonData11["positionAmount"]);
			}
			if (Has(jsonData11, "rotationAmount"))
			{
				noise.rotationAmount = MinMax(jsonData11["rotationAmount"]);
			}
			if (Has(jsonData11, "sizeAmount"))
			{
				noise.sizeAmount = MinMax(jsonData11["sizeAmount"]);
			}
		}
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = system.textureSheetAnimation;
		JsonData jsonData12 = Enabled(data, "UVModule");
		textureSheetAnimation.enabled = jsonData12 != null;
		if (jsonData12 != null)
		{
			textureSheetAnimation.mode = ParticleSystemAnimationMode.Grid;
			textureSheetAnimation.numTilesX = Mathf.Max(1, (int)Number(jsonData12, "tilesX", 1f));
			textureSheetAnimation.numTilesY = Mathf.Max(1, (int)Number(jsonData12, "tilesY", 1f));
			textureSheetAnimation.animation = (ParticleSystemAnimationType)Number(jsonData12, "animationType");
			if (Has(jsonData12, "rowMode"))
			{
				textureSheetAnimation.rowMode = (ParticleSystemAnimationRowMode)Number(jsonData12, "rowMode");
			}
			else if (Has(jsonData12, "randomRow"))
			{
				textureSheetAnimation.rowMode = (Flag(jsonData12, "randomRow", fallback: true) ? ParticleSystemAnimationRowMode.Random : ParticleSystemAnimationRowMode.Custom);
			}
			textureSheetAnimation.rowIndex = (int)Number(jsonData12, "rowIndex");
			textureSheetAnimation.timeMode = (ParticleSystemAnimationTimeMode)Number(jsonData12, "timeMode");
			textureSheetAnimation.fps = Number(jsonData12, "fps", 30f);
			textureSheetAnimation.frameOverTime = MinMax(jsonData12["frameOverTime"]);
			textureSheetAnimation.startFrame = MinMax(jsonData12["startFrame"]);
			textureSheetAnimation.cycleCount = Mathf.Max(1, (int)Number(jsonData12, "cycles", 1f));
			textureSheetAnimation.flipU = Number(jsonData12, "flipU");
			textureSheetAnimation.flipV = Number(jsonData12, "flipV");
		}
	}

	private void ConfigureParticleRenderer(ParticleSystemRenderer renderer, JsonData data)
	{
		ParticleSystemRenderMode particleSystemRenderMode = (ParticleSystemRenderMode)Number(data, "m_RenderMode");
		Mesh mesh = (Has(data, "m_Mesh") ? (Ref(data["m_Mesh"]) as Mesh) : null);
		if (particleSystemRenderMode == ParticleSystemRenderMode.Mesh && mesh == null)
		{
			particleSystemRenderMode = ParticleSystemRenderMode.Billboard;
		}
		renderer.renderMode = particleSystemRenderMode;
		if (mesh != null)
		{
			renderer.mesh = mesh;
		}
		renderer.minParticleSize = Number(data, "m_MinParticleSize");
		renderer.maxParticleSize = Number(data, "m_MaxParticleSize", 0.5f);
		renderer.velocityScale = Number(data, "m_VelocityScale");
		renderer.lengthScale = Number(data, "m_LengthScale", 2f);
		renderer.cameraVelocityScale = Number(data, "m_CameraVelocityScale");
		renderer.alignment = (ParticleSystemRenderSpace)Number(data, "m_RenderAlignment");
		renderer.sortMode = (ParticleSystemSortMode)Number(data, "m_SortMode");
		renderer.sortingFudge = Number(data, "m_SortingFudge");
		renderer.pivot = Vector3Of(data, "m_Pivot");
		renderer.normalDirection = Number(data, "m_NormalDirection", 1f);
	}

	public void Dispose()
	{
		if (Root != null)
		{
			UIGame3DWidgetFromRes[] componentsInChildren = Root.GetComponentsInChildren<UIGame3DWidgetFromRes>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop(immediatelyClear: true);
			}
			UnityEngine.Object.DestroyImmediate(Root);
			Root = null;
		}
		if (bank != null)
		{
			UnityEngine.Object.DestroyImmediate(bank);
		}
		bank = null;
		for (int num = owned.Count - 1; num >= 0; num--)
		{
			if (owned[num] != null)
			{
				UnityEngine.Object.DestroyImmediate(owned[num]);
			}
		}
		owned.Clear();
		objects.Clear();
	}
}
