using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JsonBin;
using NGame2.N3D;
using NGame2.NAccount;
using NGameAsset;
using NGameAsset.NBundleManager;
using NVespa;
using NGame2.NRender;
using NShared;
using NShared.NJit;
using UnityEngine;

// Client automation fixture (accessory-mask-probe, Login scene, accessories-* profile): renders a
// hero wearing the Goblin mask, the xdd mask, then the Goblin mask again from the same model pool,
// and writes <report>.png (close-ups) and <report>.wide.png. Checks that the xdd mask uses the
// embedded mesh and texture and that the pooled Goblin model gets its own mesh back afterwards.
public sealed class SprkAccessoryMasksProbe : MonoBehaviour
{
	private const int Layer = 31;

	private const int Size = 512;

	private string report;

	private GameObject root;

	private readonly List<RenderFacade> facades = new List<RenderFacade>();

	public static string Start(string reportPath)
	{
		GameObject gameObject = new GameObject("SPRK accessory mask probe");
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		gameObject.AddComponent<SprkAccessoryMasksProbe>().report = reportPath;
		return "STARTED: " + reportPath;
	}

	private void Start()
	{
		StartCoroutine(Observe());
	}

	private void Update()
	{
		foreach (RenderFacade facade in facades)
		{
			if (facade != null)
			{
				if (facade.RenderGroupModifier != null)
				{
					facade.RenderGroupModifier.SetExtraAlpha(1f);
				}
				facade.ChainUpdate();
			}
		}
	}

	private IEnumerator Observe()
	{
		IEnumerator test = Check();
		string result = null;
		while (true)
		{
			object current = null;
			bool more;
			try
			{
				more = test.MoveNext();
				if (more)
				{
					current = test.Current;
				}
				else
				{
					result = test.Current as string;
				}
			}
			catch (Exception ex)
			{
				result = "FAIL: " + ex;
				more = false;
			}
			if (!more)
			{
				break;
			}
			yield return current;
		}
		foreach (RenderFacade facade in facades)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(facade);
		}
		facades.Clear();
		if (root != null)
		{
			UnityEngine.Object.Destroy(root);
		}
		File.WriteAllText(report, result ?? "FAIL: no result");
	}

	// The Login scene has not loaded the asset index yet; do what LoaderPlaying does after patching
	// (same setup as the automation maria-bird-probe).
	private static void LoadAssetIndex()
	{
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		string patch = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows");
		Dictionary<string, AssetInfo> files = new Dictionary<string, AssetInfo>();
		MethodInfo entryName = typeof(AssetInfo).GetProperty("EntryName").GetSetMethod(nonPublic: true);
		foreach (string file in Directory.GetFiles(Path.Combine(patch, "Assetbundle"), "*", SearchOption.AllDirectories))
		{
			string key = file.Substring(patch.Length + 1).Replace('\\', '/');
			AssetInfo info = new AssetInfo();
			entryName.Invoke(info, new object[1] { key });
			files[key] = info;
		}
		SingletonAuto<AssetFileManager>.instance.Setup(files);
		typeof(UnityCache).GetProperty("persistentDataPath").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { Application.dataPath });
		typeof(NGameAsset.Define).GetProperty("PatchDownloaded").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { true });
		if (!JsonRead.CreateRoot(File.ReadAllBytes(Path.Combine(patch, "Assetbundle/BundlePack.bin")), out var root))
		{
			throw new Exception("Bundle pack unavailable");
		}
		SingletonUpdater<LoaderPlaying>.instance.InfoContainer.LoadFromJson(root["BundleFileInfo"]);
		SingletonAuto<NGameAsset.NCreature.BuilderContainer>.instance.Load(root["Creature"]);
		SingletonAuto<NGameAsset.NEffect.BuilderContainer>.instance.Load(root["Effect"]);
	}

	private IEnumerator Check()
	{
		LoadAssetIndex();
		root = new GameObject("SPRK accessory mask stage");
		root.layer = Layer;
		root.transform.position = new Vector3(5000f, 0f, 0f);
		Texture2D close = new Texture2D(Size * 5, Size, TextureFormat.RGB24, mipChain: false);
		Texture2D wide = new Texture2D(Size, Size, TextureFormat.RGB24, mipChain: false);
		string[] names = new string[3] { "Goblin", "Xdd", "Goblin after Xdd" };
		int[] indices = new int[3] { 3102510, AccessoryCostumeData.SprkXddMaskIndex, 3102510 };
		string[] meshes = new string[3] { "Accessory_Head_Head_Goblinmask", "SPRK Xdd mask", "Accessory_Head_Head_Goblinmask" };
		List<string> notes = new List<string>();
		for (int i = 0; i < 3; i++)
		{
			HeroInfo heroInfo = new HeroInfo
			{
				HeroIndex = 1,
				Star = 5
			};
			RenderFacadeBuilder builder = new RenderFacadeBuilder(root.transform, TeamType.Left, heroInfo);
			builder.SetAccessory(indices[i], heroInfo.HeroIndex, PositionInfo.Create());
			RenderFacade facade = builder.Build().DISABLEGROUNDCLIP();
			if (facade == null)
			{
				throw new Exception(names[i] + ": render facade was not built");
			}
			facades.Add(facade);
			MeshFilter mask = null;
			float until = Time.realtimeSinceStartup + 90f;
			while (mask == null)
			{
				if (Time.realtimeSinceStartup > until)
				{
					throw new Exception(names[i] + ": accessory model never loaded");
				}
				yield return null;
				foreach (MeshFilter filter in facade.GetComponentsInChildren<MeshFilter>(includeInactive: false))
				{
					if (filter.sharedMesh != null && IsAccessory(filter))
					{
						mask = filter;
					}
				}
			}
			for (int frame = 0; frame < 20; frame++)
			{
				yield return null;
			}
			string meshName = mask.sharedMesh.name;
			if (meshName != meshes[i])
			{
				throw new Exception(names[i] + ": expected mesh " + meshes[i] + ", got " + meshName);
			}
			Renderer renderer = mask.GetComponent<Renderer>();
			Texture texture = renderer.sharedMaterial.mainTexture;
			bool xdd = i == 1;
			if (xdd != (texture != null && texture.name == "SPRK Xdd mask"))
			{
				throw new Exception(names[i] + ": unexpected main texture " + ((texture != null) ? texture.name : "null"));
			}
			if (!renderer.enabled)
			{
				throw new Exception(names[i] + ": mask renderer is disabled");
			}
			// SprkFreeCamera copies accessory meshes in the Dressing Room; the copy must keep its geometry.
			Mesh copy = UnityEngine.Object.Instantiate(mask.sharedMesh);
			int copied = copy.vertexCount;
			UnityEngine.Object.Destroy(copy);
			if (copied != mask.sharedMesh.vertexCount || copied == 0)
			{
				throw new Exception(names[i] + ": a copy of the mesh has " + copied + " of " + mask.sharedMesh.vertexCount + " vertices");
			}
			Bounds bounds = renderer.bounds;
			notes.Add(names[i] + " size " + bounds.size.ToString("F3"));
			Transform tm = mask.transform;
			Vector3 front = tm.TransformDirection(Vector3.down).normalized;
			Vector3 up = tm.TransformDirection(Vector3.forward).normalized;
			Render(close, i * Size, bounds.center + front * (bounds.extents.magnitude * 3.2f), bounds.center, up);
			if (xdd)
			{
				Vector3 body = facade.transform.position + Vector3.up * (bounds.center.y - facade.transform.position.y) * 0.6f;
				Render(wide, 0, body + front * 4.5f, body, Vector3.up);
			}
			// Release before the next hero so the Goblin after Xdd reuses the pooled model.
			facades.Remove(facade);
			RenderFacadeBuilder.ReleaseRenderFacade(facade);
			for (int frame = 0; frame < 10; frame++)
			{
				yield return null;
			}
		}
		// Dressing Room: several head accessories stored like SprkAccessories.Preview, and the hero
		// destroyed and respawned at once on every selection (HeroFullSceneViewBase.ChangeRenderFacadeOnly).
		string[] rooms = new string[2] { "GL17 + Xdd respawned", "Goblin + Xdd respawned" };
		int[] others = new int[2] { 3901601, 3102510 };
		for (int r = 0; r < 2; r++)
		{
			CachedHeroInfo hero = new CachedHeroInfo
			{
				HeroIndex = 1,
				Star = 5
			};
			List<SprkAccessoryEntry> entries = new List<SprkAccessoryEntry>();
			SprkAccessories.Put(entries, others[r], AccessoryPartType.Head, PositionInfo.Create());
			SprkAccessories.Put(entries, AccessoryCostumeData.SprkXddMaskIndex, AccessoryPartType.Head, PositionInfo.Create());
			SprkAccessories.Store(hero, entries);
			RenderFacade facade = null;
			for (int cycle = 0; cycle < 4; cycle++)
			{
				if (facade != null)
				{
					facades.Remove(facade);
					RenderFacadeBuilder.ReleaseRenderFacade(facade);
				}
				facade = new RenderFacadeBuilder(root.transform, TeamType.Left, hero).Build().DISABLEGROUNDCLIP();
				facades.Add(facade);
				for (int frame = 0; frame < cycle * 3; frame++)
				{
					yield return null;
				}
			}
			MeshFilter xddMask = null;
			int layers = 0;
			float deadline = Time.realtimeSinceStartup + 90f;
			while (xddMask == null || layers < 2)
			{
				if (Time.realtimeSinceStartup > deadline)
				{
					throw new Exception(rooms[r] + ": " + layers + " accessory layers loaded, xdd " + (xddMask != null));
				}
				yield return null;
				layers = 0;
				foreach (Transform item in facade.GetComponentsInChildren<Transform>(includeInactive: false))
				{
					if (item.name == "CustomizeItem_Transform" && item.GetComponentInChildren<Renderer>() != null)
					{
						layers++;
					}
				}
				foreach (MeshFilter filter in facade.GetComponentsInChildren<MeshFilter>(includeInactive: false))
				{
					if (filter.sharedMesh != null && filter.sharedMesh.name == "SPRK Xdd mask" && IsAccessory(filter))
					{
						xddMask = filter;
					}
				}
			}
			for (int frame = 0; frame < 20; frame++)
			{
				yield return null;
			}
			Renderer renderer = xddMask.GetComponent<Renderer>();
			Texture texture = renderer.sharedMaterial.mainTexture;
			if (texture == null || texture.name != "SPRK Xdd mask" || !renderer.enabled || xddMask.sharedMesh == null)
			{
				throw new Exception(rooms[r] + ": xdd mask lost its texture, mesh or renderer");
			}
			Bounds bounds = renderer.bounds;
			Transform tm = xddMask.transform;
			Vector3 front = tm.TransformDirection(Vector3.down).normalized;
			Render(close, (3 + r) * Size, bounds.center + front * (bounds.extents.magnitude * 3.2f), bounds.center, tm.TransformDirection(Vector3.forward).normalized);
			notes.Add(rooms[r] + ": " + layers + " layers");
			facades.Remove(facade);
			RenderFacadeBuilder.ReleaseRenderFacade(facade);
		}
		File.WriteAllBytes(report + ".png", close.EncodeToPNG());
		File.WriteAllBytes(report + ".wide.png", wide.EncodeToPNG());
		UnityEngine.Object.Destroy(close);
		UnityEngine.Object.Destroy(wide);
		yield return "PASS: Goblin, Xdd (embedded mesh and texture) and pooled Goblin restore; " + string.Join("; ", notes.ToArray());
	}

	private static bool IsAccessory(MeshFilter filter)
	{
		for (Transform transform = filter.transform; transform != null; transform = transform.parent)
		{
			if (transform.name == "CustomizeItem_Transform")
			{
				return true;
			}
		}
		return false;
	}

	private void Render(Texture2D into, int x, Vector3 eye, Vector3 target, Vector3 up)
	{
		GameObject gameObject = new GameObject("SPRK accessory mask camera");
		try
		{
			Camera camera = gameObject.AddComponent<Camera>();
			camera.enabled = false;
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = new Color(0.12f, 0.24f, 0.42f, 1f);
			camera.cullingMask = 1 << Layer;
			camera.fieldOfView = 30f;
			camera.nearClipPlane = 0.01f;
			camera.farClipPlane = 50f;
			camera.transform.position = eye;
			// Outline width/colour and rim light are globals the game sets outside the Login scene.
			NShader.ShaderVariableUpdater.UpdateGlobalVariable();
			camera.transform.LookAt(target, up);
			RenderTexture renderTexture = RenderTexture.GetTemporary(Size, Size, 24, RenderTextureFormat.ARGB32);
			camera.targetTexture = renderTexture;
			camera.Render();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = renderTexture;
			into.ReadPixels(new Rect(0f, 0f, Size, Size), x, 0);
			into.Apply();
			RenderTexture.active = active;
			camera.targetTexture = null;
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		finally
		{
			UnityEngine.Object.Destroy(gameObject);
		}
	}
}
