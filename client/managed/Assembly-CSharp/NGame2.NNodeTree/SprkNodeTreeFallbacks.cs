using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using NGameAsset;
using NVespa;

namespace NGame2.NNodeTree;

// Node trees (cinematics) that SPRK supplies for packs that lack them. The
// unreleased hero Maya has only her skill trees: without GetNewHero the recruit
// screen waits forever, and the soul weapon and transcendence scenes would do the
// same. Each fallback copies a released hero's tree into the pack as it loads and
// swaps the donor's assets for the hero's own where they exist. See
// docs/heroes/maya-art.md.
public static class SprkNodeTreeFallbacks
{
	private sealed class Fallback
	{
		public string Pack;

		public string Tree;

		public string DonorPack;

		public string DonorTree;

		// Exact compact-JSON fragments to replace in the copied tree.
		public string[][] Replace;

		// Optional changes to the tree's AssetEventCamera node: the hero's own camera, its field-of-view
		// scale, and a position scale (above 1 pulls the camera back along its whole path).
		public string CameraCode;

		public double CameraOrthoScale;

		public double CameraPositionScale;
	}

	private static readonly Fallback[] Fallbacks = new Fallback[5]
	{
		new Fallback
		{
			Pack = "Hero/Maya",
			Tree = "GetNewHero",
			DonorPack = "Hero/Selene",
			DonorTree = "GetNewHero",
			// Maya has her own Get motion, name text and Get sound, but no Get camera (not even in the
			// Pre-Doomsday archive): Selene's camera stays, pulled back because Maya is taller.
			CameraPositionScale = 1.3,
			Replace = new string[3][]
			{
				new string[2] { "\"Code\":\"GetHero.Selene.Text\"", "\"Code\":\"GetHero.Maya.Text\"" },
				new string[2] { "\"ModelName\":\"GetHero_Selene_Text(Clone)\"", "\"ModelName\":\"GetHero_Maya_Text(Clone)\"" },
				new string[2] { "\"Code\":\"Get.Selene\",\"PreventZombieTime\"", "\"Code\":\"Get.Maya\",\"PreventZombieTime\"" }
			}
		},
		new Fallback
		{
			Pack = "Hero/Maya",
			Tree = "Transcendence",
			DonorPack = "Hero/Selene",
			DonorTree = "Transcendence",
			// Maya has her own Transcendence motion, camera and effect; the chime sound stays Selene's.
			// Maya's camera stores the field of view in degrees (30-45) where Selene's stores a
			// multiplier (1) for the node's OrthoScale of 45, so her camera needs OrthoScale 1;
			// at 45 it renders a stretched, 1000+ degree view.
			Replace = new string[1][] { new string[2] { "\"Code\":\"Transcendence.Selene\"}", "\"Code\":\"Transcendence.Maya\"}" } },
			CameraCode = "Transcendence.Maya",
			CameraOrthoScale = 1.0
		},
		new Fallback
		{
			Pack = "Hero/Maya",
			Tree = "GetSoulWeapon",
			DonorPack = "Hero/Selene",
			DonorTree = "GetSoulWeapon",
			// Maya has no SoulWeaponGet motions, camera, soulstone or soul models: those stay
			// Selene's stand-ins (her soul weapon already shows Selene's bow). Maya herself is shown.
			Replace = new string[1][] { new string[2] { "\"CreatureCode\":\"Hero.Selene\"", "\"CreatureCode\":\"Hero.Maya\"" } }
		},
		new Fallback
		{
			Pack = "Item/3dModel_SoulWeapon",
			Tree = "SOULWEAPON_MAYA_REINFORCESUCCESS",
			DonorPack = "Item/3dModel_SoulWeapon",
			DonorTree = "SOULWEAPON_SELENE_REINFORCESUCCESS",
			Replace = new string[0][]
		},
		new Fallback
		{
			Pack = "Item/3dModel_SoulWeapon",
			Tree = "SOULWEAPON_MAYA_REINFORCEFAIL",
			DonorPack = "Item/3dModel_SoulWeapon",
			DonorTree = "SOULWEAPON_SELENE_REINFORCEFAIL",
			Replace = new string[0][]
		}
	};

	// Adds the missing trees to a pack's JSON before PackData is built from it.
	public static void Apply(string pathName, JsonData json)
	{
		string pack = Normalize(pathName);
		JsonData trees = json?["TreeArray"];
		if (trees == null || !trees.IsArray())
		{
			return;
		}
		Dictionary<string, JsonData> donors = null;
		foreach (Fallback fallback in Fallbacks)
		{
			if (!string.Equals(fallback.Pack, pack, StringComparison.OrdinalIgnoreCase) || FindTree(trees, fallback.Tree) != null)
			{
				continue;
			}
			try
			{
				if (donors == null)
				{
					donors = new Dictionary<string, JsonData>(StringComparer.OrdinalIgnoreCase);
				}
				if (!donors.TryGetValue(fallback.DonorPack, out var donor))
				{
					donor = (string.Equals(fallback.DonorPack, pack, StringComparison.OrdinalIgnoreCase) ? json : LoadPackJson(fallback.DonorPack));
					donors[fallback.DonorPack] = donor;
				}
				JsonData donorTree = FindTree(donor?["TreeArray"], fallback.DonorTree);
				if (donorTree == null)
				{
					VespaDebug.LogError("SPRK node tree fallback: " + fallback.DonorPack + "/" + fallback.DonorTree + " not found");
					continue;
				}
				string text = donorTree.ToJson();
				string donorCode = "\"Code\":\"" + fallback.DonorTree + "\"";
				int codeIndex = text.IndexOf(donorCode, StringComparison.Ordinal);
				if (codeIndex >= 0)
				{
					text = text.Substring(0, codeIndex) + "\"Code\":\"" + fallback.Tree + "\"" + text.Substring(codeIndex + donorCode.Length);
				}
				foreach (string[] pair in fallback.Replace)
				{
					if (text.IndexOf(pair[0], StringComparison.Ordinal) < 0)
					{
						VespaDebug.LogError("SPRK node tree fallback: " + fallback.Tree + " has no " + pair[0]);
					}
					text = text.Replace(pair[0], pair[1]);
				}
				JsonData copy = JsonMapper.ToObject(text);
				if (copy != null && copy.IsObject())
				{
					if (fallback.CameraCode != null || fallback.CameraOrthoScale > 0.0 || fallback.CameraPositionScale > 0.0)
					{
						SetCamera(copy, fallback);
					}
					trees.Add(copy);
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("SPRK node tree fallback " + fallback.Pack + "/" + fallback.Tree + " failed: " + ex.Message);
			}
		}
		if (donors == null)
		{
			return;
		}
		foreach (JsonData donor in donors.Values)
		{
			if (donor != null && donor != json)
			{
				JsonData release = donor;
				SingletonAuto<JsonPool>.instance.SafeRelease(ref release);
			}
		}
	}

	private static void SetCamera(JsonData tree, Fallback fallback)
	{
		JsonData nodes = tree["Body"]?["NodeArray"];
		int changed = 0;
		for (int i = 0; nodes != null && i < nodes.Count; i++)
		{
			JsonData contents = nodes.GetFromArray(i)?["Contents"];
			if (contents?["type"] == null || !contents["type"].TryGetString(out var type) || type != "Effect/AssetEventCamera")
			{
				continue;
			}
			if (fallback.CameraCode != null)
			{
				contents["Code"]?.SetString(fallback.CameraCode);
			}
			if (fallback.CameraOrthoScale > 0.0)
			{
				contents["OrthoScale"]?["Body"]?["Derived"]?.SetDouble(fallback.CameraOrthoScale);
			}
			if (fallback.CameraPositionScale > 0.0)
			{
				contents["PositionScale"]?["Body"]?["Derived"]?.SetDouble(fallback.CameraPositionScale);
			}
			changed++;
		}
		if (changed != 1)
		{
			VespaDebug.LogError("SPRK node tree fallback: " + fallback.Tree + " has " + changed + " cameras");
		}
	}

	private static string Normalize(string pathName)
	{
		return (pathName ?? string.Empty).Replace('.', '/').Replace('\\', '/').Trim('/');
	}

	private static JsonData FindTree(JsonData trees, string code)
	{
		if (trees == null || !trees.IsArray())
		{
			return null;
		}
		for (int i = 0; i < trees.Count; i++)
		{
			JsonData tree = trees.GetFromArray(i);
			if (tree?["Body"]?["Code"] != null && tree["Body"]["Code"].TryGetString(out var value) && value == code)
			{
				return tree;
			}
		}
		return null;
	}

	// Loads another pack's JSON the way PackLoad.Open resolves it.
	private static JsonData LoadPackJson(string pathName)
	{
		string text = String_Extension_Method.CombinePath("Text", String_Extension_Method.CombinePath("NodeTree/", pathName) + ".json");
		PlainAssetHelper.GetAssetFileInfo(PlainAssetHelper.EPathType.PlainAsset, text, out var assetPath, out var isEncrypt, out var isJsonBin);
		if (!File.Exists(assetPath))
		{
			return null;
		}
		return isJsonBin ? PlainAssetHelper.LoadLitJsonFromJsonBin(File.ReadAllBytes(assetPath), assetPath, isEncrypt) : PlainAssetHelper.LoadLitJsonFromJsonText(File.ReadAllText(assetPath), assetPath, isEncrypt);
	}
}
