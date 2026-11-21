using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Head accessories SPRK adds without asset bundles: each reuses an original accessory effect
// (its pivot, layer, timing and shader) and swaps the model's mesh and main texture for ones
// embedded in this assembly (SprkMasks.<Name>.mesh / .png, made by
// client/tools/AccessoryMasks/make_paper_mask.py). See docs/accessory-masks.md.
public static class SprkAccessoryMasks
{
	private sealed class Mask
	{
		public string Name;

		public string BaseCode;

		public Mesh Mesh;

		public Texture2D Texture;

		public bool Failed;
	}

	private sealed class Swap
	{
		public MeshFilter Filter;

		public Mesh Mesh;

		public Renderer Renderer;

		public Material[] Materials;

		public Material[] Created;
	}

	public const string XddCode = "Accessory.Head.Head.SprkXdd";

	private static readonly Dictionary<string, Mask> masks = new Dictionary<string, Mask>(StringComparer.Ordinal)
	{
		{
			XddCode,
			new Mask
			{
				Name = "Xdd",
				BaseCode = "Accessory.Head.Head.Goblinmask"
			}
		}
	};

	// Pooled model instances currently showing a mask, by instance ID.
	private static readonly Dictionary<int, Swap> swaps = new Dictionary<int, Swap>();

	public static bool IsMask(string code)
	{
		if (code != null)
		{
			return masks.ContainsKey(code);
		}
		return false;
	}

	// The effect whose assets a requested accessory code loads (BuilderContainer.Get).
	public static string EffectCode(string code)
	{
		if (code != null && masks.TryGetValue(code, out var value))
		{
			return value.BaseCode;
		}
		return code;
	}

	// Called right after an effect model is taken from its pool, before its renderers are wrapped,
	// so material clones made later (alpha, grayscale, outlines) start from the mask's material.
	public static void Apply(string code, GameObject model)
	{
		if (model == null || code == null || !masks.TryGetValue(code, out var value) || !Load(value))
		{
			return;
		}
		Restore(model);
		MeshFilter filter = model.GetComponentInChildren<MeshFilter>(includeInactive: true);
		Renderer renderer = ((filter != null) ? filter.GetComponent<Renderer>() : null);
		if (filter == null || renderer == null)
		{
			Debug.LogWarning("[SPRK] Accessory mask " + code + ": base model has no mesh renderer");
			return;
		}
		Material[] sharedMaterials = renderer.sharedMaterials;
		Material[] array = new Material[sharedMaterials.Length];
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if (!(sharedMaterials[i] == null))
			{
				Material material = new Material(sharedMaterials[i]);
				material.name = "SPRK " + value.Name + " mask";
				material.hideFlags = HideFlags.DontSave;
				material.mainTexture = value.Texture;
				array[i] = material;
			}
		}
		swaps[model.GetInstanceID()] = new Swap
		{
			Filter = filter,
			Mesh = filter.sharedMesh,
			Renderer = renderer,
			Materials = sharedMaterials,
			Created = array
		};
		filter.sharedMesh = value.Mesh;
		renderer.sharedMaterials = array;
	}

	// Called before a model returns to its pool: the pool also serves the base accessory.
	public static void Restore(GameObject model)
	{
		if (model == null || swaps.Count == 0 || !swaps.TryGetValue(model.GetInstanceID(), out var value))
		{
			return;
		}
		swaps.Remove(model.GetInstanceID());
		if (value.Filter != null)
		{
			value.Filter.sharedMesh = value.Mesh;
		}
		if (value.Renderer != null)
		{
			value.Renderer.sharedMaterials = value.Materials;
		}
		Material[] created = value.Created;
		foreach (Material material in created)
		{
			if (material != null)
			{
				UnityEngine.Object.Destroy(material);
			}
		}
	}

	private static bool Load(Mask mask)
	{
		if (mask.Mesh != null && mask.Texture != null)
		{
			return true;
		}
		if (mask.Failed)
		{
			return false;
		}
		try
		{
			Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);
			if (!texture2D.LoadImage(Resource("SprkMasks." + mask.Name + ".png"), markNonReadable: true))
			{
				throw new InvalidDataException("texture");
			}
			texture2D.name = "SPRK " + mask.Name + " mask";
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.anisoLevel = 4;
			texture2D.hideFlags = HideFlags.HideAndDontSave;
			mask.Texture = texture2D;
			mask.Mesh = ReadMesh(Resource("SprkMasks." + mask.Name + ".mesh"), mask.Name);
			return true;
		}
		catch (Exception ex)
		{
			mask.Failed = true;
			Debug.LogError("[SPRK] Accessory mask " + mask.Name + " failed to load: " + ex.Message);
			return false;
		}
	}

	private static byte[] Resource(string name)
	{
		return SprkEmbeddedResources.Read(typeof(SprkAccessoryMasks).Assembly, name) ?? throw new FileNotFoundException(name);
	}

	// Format: "SPKM", int32 version 1, vertex count, index count, per vertex position/normal/uv
	// (8 float32), then int32 indices; little endian (make_paper_mask.py).
	public static Mesh ReadMesh(byte[] data, string name)
	{
		using BinaryReader binaryReader = new BinaryReader(new MemoryStream(data));
		if (binaryReader.ReadInt32() != 0x4D4B5053 || binaryReader.ReadInt32() != 1)
		{
			throw new InvalidDataException("mesh header");
		}
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		if (num <= 0 || num > 65535 || num2 <= 0 || num2 % 3 != 0 || data.Length != 16 + num * 32 + num2 * 4)
		{
			throw new InvalidDataException("mesh size");
		}
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = new Vector3[num];
		Vector2[] array3 = new Vector2[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			array2[i] = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
			array3[i] = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
		}
		int[] array4 = new int[num2];
		for (int j = 0; j < num2; j++)
		{
			int num3 = binaryReader.ReadInt32();
			if (num3 < 0 || num3 >= num)
			{
				throw new InvalidDataException("mesh index");
			}
			array4[j] = num3;
		}
		Mesh mesh = new Mesh();
		mesh.name = "SPRK " + name + " mask";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		mesh.vertices = array;
		mesh.normals = array2;
		mesh.uv = array3;
		mesh.triangles = array4;
		mesh.RecalculateBounds();
		// Keep the mesh readable: SprkFreeCamera copies accessory meshes (Object.Instantiate) to
		// adjust their shadow bounds in the Dressing Room, and a copy of an unreadable mesh is empty.
		mesh.UploadMeshData(markNoLongerReadable: false);
		return mesh;
	}
}
