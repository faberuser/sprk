using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NLoading;
using NGameAsset;
using NGameAsset.NNguiAtlas;
using NShared;
using NShared.NJit;
using NVespa;
using UnityEngine;

public static class SprkImageRecovery
{
	private sealed class PortraitLayout
	{
		public int Width;

		public int Height;

		public Vector4 DrawRegion;

		public UITexture Fade;

		public Texture2D FadeTexture;

		public int FadeHeight;

		public int FadeCardHeight;
	}

	private sealed class State
	{
		public string Path;

		public int Attempts;

		public float Poll;

		public float MissingSince;

		public float ReloadAfter;

		public bool Released;

		public bool Disabled;
	}

	private sealed class AtlasRetry
	{
		public AtlasLoad Atlas;

		public float At;
	}

	private static readonly Dictionary<int, string> lightFallbackPortraits = new Dictionary<int, string>
	{
		{ 580, "Portrait/Monster/LightGau" },
		{ 680, "Icon/Creature/Monster/Ordeal/LightPhillop_5" },
		{ 780, "Icon/Creature/Monster/Ordeal/LightLakrak_5" },
		{ 880, "Icon/Creature/Monster/Ordeal/LightKaulah_5" },
		{ 980, "Icon/Creature/Monster/Ordeal/LightEpis_5" },
		{ 1080, "Portrait/Monster/Ordeal/LightSelene" },
		{ 1180, "Portrait/Monster/Ordeal/LightMaria" },
		{ 1280, "Icon/Creature/Monster/Ordeal/LightMiruru_5" },
		{ 1380, "Icon/Creature/Monster/Ordeal/LightLorraine_5" },
		{ 1680, "Icon/Creature/Monster/Ordeal/LightRephy_5" },
		{ 1780, "Icon/Creature/Monster/Ordeal/LightDimael_5" },
		{ 1880, "Icon/Creature/Monster/Ordeal/LightReina_5" },
		{ 1980, "Icon/Creature/Monster/Ordeal/LightPavel_5" },
		{ 2080, "Icon/Creature/Monster/Ordeal/LightDemia_5" },
		{ 2180, "Icon/Creature/Monster/Ordeal/LightNaila_5" },
		{ 2280, "Icon/Creature/Monster/Ordeal/LightRodina_5" },
		{ 2380, "Icon/Creature/Monster/Ordeal/LightMorrah_5" },
		{ 2480, "Icon/Creature/Monster/Ordeal/LightBaudouin_5" },
		{ 2680, "Icon/Creature/Monster/Ordeal/LightAisha_5" },
		{ 2780, "Icon/Creature/Monster/Ordeal/LightLeo_5" },
		{ 2880, "Icon/Creature/Monster/Ordeal/LightFluss_5" },
		{ 2980, "Icon/Creature/Monster/Ordeal/LightLuna_5" },
		{ 3180, "Portrait/Monster/Ordeal/LightArch" },
		{ 3280, "Icon/Creature/Monster/Ordeal/LightLewsia_5" },
		{ 3380, "Icon/Creature/Monster/Ordeal/LightNicx_5" },
		{ 3580, "Icon/Creature/Monster/Ordeal/LightMitra_5" },
		{ 3680, "Icon/Creature/Monster/Ordeal/LightRicardo_5" },
		{ 3780, "Icon/Creature/Monster/Ordeal/LightTanya_5" },
		{ 3880, "Icon/Creature/Monster/Ordeal/LightEzekiel_5" },
		{ 3980, "Icon/Creature/Monster/Ordeal/LightCassandra_5" },
		{ 4080, "Icon/Creature/Monster/Ordeal/LightTheo_5" },
		{ 4180, "Icon/Creature/Monster/Ordeal/LightMediana_5" },
		{ 4280, "Icon/Creature/Monster/Ordeal/LightOddy_5" },
		{ 4380, "Icon/Creature/Monster/Ordeal/LightViska_5" },
		{ 4480, "Icon/Creature/Monster/Ordeal/LightPriscilla_5" },
		{ 4680, "Icon/Creature/Monster/Ordeal/LightZafir_5" },
		{ 4880, "Portrait/Monster/Ordeal/LightRehartna" },
		{ 4980, "Icon/Creature/Monster/Ordeal/LightOphelia_5" },
		{ 5080, "Icon/Creature/Monster/Ordeal/LightRequina_5" },
		{ 5280, "Icon/Creature/Monster/Ordeal/LightCrow_5" },
		{ 5780, "Icon/Creature/Monster/Ordeal/LightLilia_5" },
		{ 5880, "Icon/Creature/Monster/Ordeal/LightLaudia_5" },
		{ 5980, "Portrait/Monster/LightLucias" },
		{ 6080, "Portrait/Monster/LightChrisha" },
		{ 6180, "Portrait/Monster/LightNeraxis" },
		{ 6380, "Icon/Creature/Monster/Ordeal/LightSonia_5" },
		{ 6580, "Portrait/Monster/Ordeal/LightMirianne" },
		{ 6780, "Icon/Creature/Monster/Ordeal/LightKara_5" },
		{ 6880, "Portrait/Monster/Ordeal/LightNia" },
		{ 6980, "Portrait/Monster/Ordeal/LightEsker" },
		{ 7080, "Portrait/Monster/Ordeal/LightCecilia" },
		{ 7180, "Portrait/Monster/Ordeal/LightKirze" },
		{ 7280, "Portrait/Monster/Ordeal/LightPancironne" },
		{ 7480, "Portrait/Monster/Ordeal/LightKibera" },
		{ 7580, "Portrait/Monster/Ordeal/LightDakaris" },
		{ 7680, "Portrait/Monster/Ordeal/LightTaily" },
		{ 7780, "Portrait/Monster/Ordeal/LightXerah" },
		{ 7880, "Portrait/Monster/Ordeal/LightBernheim" },
		{ 7980, "Portrait/Monster/Ordeal/LightEvan" },
		{ 8080, "Portrait/Monster/Ordeal/LightShakmeh" },
		{ 8180, "Portrait/Monster/Ordeal/LightHilda" },
		{ 8280, "Portrait/Monster/Ordeal/LightIsolet" },
		{ 8780, "Portrait/Monster/Ordeal/LightGlenwys" },
		{ 8880, "Portrait/Monster/Ordeal/LightRiheet" },
		{ 8980, "Portrait/Monster/Ordeal/LightRipine" },
		{ 9180, "Icon/Creature/Monster/Ordeal/LightGladi_5" },
		{ 9280, "Icon/Creature/Monster/Ordeal/LightVeronica_5" },
		{ 9380, "Icon/Creature/Monster/Ordeal/LightLoman_5" },
		{ 9480, "Icon/Creature/Monster/Ordeal/LightJuno_5" },
		{ 9880, "Portrait/Monster/Ordeal/LightKain" },
		{ 10080, "Portrait/Monster/Ordeal/LightZLucikiel" },
		{ 10180, "Portrait/Monster/Ordeal/LightEstelle" },
		{ 10280, "Portrait/Monster/Ordeal/LightIsaiah" },
		{ 11106, "Portrait/Monster/Ordeal/LightValance" }
	};

	private static readonly HashSet<string> lightFallbackPaths = new HashSet<string>(lightFallbackPortraits.Values, StringComparer.OrdinalIgnoreCase);

	private static readonly ConditionalWeakTable<UIBundleLoader, PortraitLayout> portraitLayouts = new ConditionalWeakTable<UIBundleLoader, PortraitLayout>();

	private static readonly PropertyInfo currentAssign = typeof(UIBundleLoader).GetProperty("_currentAssign", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo defaultAssign = typeof(UIBundleLoader).GetField("_assetTexAssign", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly Dictionary<int, Texture2D> tpIcons = new Dictionary<int, Texture2D>();

	private static readonly ConditionalWeakTable<UIBundleLoader, State> states = new ConditionalWeakTable<UIBundleLoader, State>();

	private static readonly Dictionary<string, AtlasRetry> atlases = new Dictionary<string, AtlasRetry>();

	private const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

	private static readonly FieldInfo texture = typeof(UIBundleLoader).GetField("_assetTex", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo releaseOnClose = typeof(UIBundleLoader).GetField("_releaseAssetOnClose", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo release = typeof(UIBundleLoader).GetMethod("ReleaseAssetTexture", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo load = typeof(UIBundleLoader).GetMethod("LoadAssetTexture", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo complete = typeof(UIBundleLoader).GetMethod("OnComplete", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo bundleCache = typeof(AssetCacheService).GetField("_assetBundleDict", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo assetCache = typeof(AssetCacheService).GetField("_assetDict", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo bundleTimes = typeof(AssetCacheService).GetField("_cacheDict", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo bundleOrder = typeof(AssetCacheService).GetField("_sortedCachedTime", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo resourceCache = typeof(AssetCacheService).GetField("_resourceDict", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo resourceTimes = typeof(AssetCacheService).GetField("_resourceCacheDict", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo resourceOrder = typeof(AssetCacheService).GetField("_sortedResourceCachedTime", BindingFlags.Instance | BindingFlags.NonPublic);

	private static bool portraitRenderStarted;

	private static bool portraitProbeStarted;

	private static bool loadingProbeStarted;

	private static bool loadingProbeAssets;

	private static bool probedTpIcons;

	public static string BattlePortraitFallback(string heroCode, int star, int costumeIndex)
	{
		return SprkLightBattlePortrait.Resolve(heroCode, star, costumeIndex);
	}

	private static void RemovePortraitFade(PortraitLayout saved)
	{
		if (saved.Fade != null)
		{
			saved.Fade.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(saved.Fade.gameObject);
			saved.Fade = null;
		}
		if (saved.FadeTexture != null)
		{
			UnityEngine.Object.Destroy(saved.FadeTexture);
			saved.FadeTexture = null;
		}
		saved.FadeHeight = 0;
	}

	private static void UpdatePortraitFade(UITexture ui, PortraitLayout saved, int fittedHeight)
	{
		int num = saved.Height - fittedHeight;
		if (num <= 0)
		{
			RemovePortraitFade(saved);
			return;
		}
		if (saved.Fade == null)
		{
			GameObject gameObject = new GameObject("SPRK Light portrait edge fade");
			gameObject.transform.SetParent(ui.transform, worldPositionStays: false);
			saved.Fade = gameObject.AddComponent<UITexture>();
			saved.FadeTexture = new Texture2D(2, 256, TextureFormat.RGBA32, mipChain: false);
			saved.FadeTexture.name = "SPRK Light portrait edge fade";
			saved.FadeTexture.wrapMode = TextureWrapMode.Clamp;
			saved.FadeTexture.filterMode = FilterMode.Bilinear;
			saved.Fade.mainTexture = saved.FadeTexture;
		}
		if (saved.FadeHeight != fittedHeight || saved.FadeCardHeight != saved.Height)
		{
			Color32[] array = new Color32[512];
			float num2 = (float)num * 0.5f;
			float num3 = Mathf.Max(1f, Mathf.Min((float)fittedHeight * 0.15f, (float)saved.Height * 0.06f));
			for (int i = 0; i < 256; i++)
			{
				float num4 = ((float)i + 0.5f) * (float)saved.Height / 256f;
				float num5 = Mathf.Min(num4, (float)saved.Height - num4);
				byte a = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01((num2 + num3 - num5) / num3));
				array[i * 2] = (array[i * 2 + 1] = new Color32(0, 0, 0, a));
			}
			saved.FadeTexture.SetPixels32(array);
			saved.FadeTexture.Apply();
			saved.FadeHeight = fittedHeight;
			saved.FadeCardHeight = saved.Height;
		}
		UITexture fade = saved.Fade;
		fade.gameObject.layer = ui.gameObject.layer;
		fade.keepAspectRatio = UIWidget.AspectRatioSource.Free;
		fade.pivot = ui.pivot;
		fade.width = saved.Width;
		fade.height = saved.Height;
		fade.depth = ui.depth + 1;
		fade.transform.localPosition = Vector3.zero;
	}

	private static void RestorePortrait(UIBundleLoader loader)
	{
		if (!(loader == null) && portraitLayouts.TryGetValue(loader, out var value))
		{
			UITexture uiTex = loader.UiTex;
			if (uiTex != null)
			{
				uiTex.drawRegion = value.DrawRegion;
			}
			RemovePortraitFade(value);
			portraitLayouts.Remove(loader);
		}
	}

	public static void BeforePortraitPathChange(UIBundleLoader loader, string next)
	{
		if (loader != null && loader.AssetPath != next)
		{
			RestorePortrait(loader);
		}
	}

	public static void BeforePortraitChange(UIBundleLoader loader, string next, UIHelper.EAssetTextureAssign assign)
	{
		if (!(loader == null) && portraitLayouts.TryGetValue(loader, out var _))
		{
			UIHelper.EAssetTextureAssign eAssetTextureAssign = ((assign == UIHelper.EAssetTextureAssign.__Ignore__) ? ((UIHelper.EAssetTextureAssign)defaultAssign.GetValue(loader)) : assign);
			if (loader.AssetPath != next || (UIHelper.EAssetTextureAssign)currentAssign.GetValue(loader, null) != eAssetTextureAssign)
			{
				RestorePortrait(loader);
			}
		}
	}

	public static void FitHeroPortrait(UIBundleLoader loader, AssetTexture.TextureInfo info)
	{
		if (loader == null || info == null || info.Width <= 0 || info.Height <= 0 || !lightFallbackPaths.Contains(loader.AssetPath))
		{
			return;
		}
		HeroPortraitComponent componentInParent = loader.GetComponentInParent<HeroPortraitComponent>();
		if (componentInParent == null || componentInParent.HeroBundleTex != loader || componentInParent.PortraitMode == HeroPortraitComponent.EPortraitMode.Icon)
		{
			return;
		}
		int key = ((componentInParent.HeroInfo == null) ? componentInParent.HeroCostumeIndex : componentInParent.HeroInfo.CostumeIndex);
		if (!lightFallbackPortraits.TryGetValue(key, out var value) || !string.Equals(loader.AssetPath, value, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		UITexture uiTex = loader.UiTex;
		if (uiTex == null || uiTex.width <= 0 || uiTex.height <= 0)
		{
			return;
		}
		if (!portraitLayouts.TryGetValue(loader, out var value2))
		{
			if (uiTex.width <= 0 || uiTex.height <= 0)
			{
				return;
			}
			value2 = new PortraitLayout
			{
				DrawRegion = uiTex.drawRegion
			};
			portraitLayouts.Add(loader, value2);
		}
		value2.Width = uiTex.width;
		value2.Height = uiTex.height;
		float num = Mathf.Min((float)value2.Width / (float)info.Width, (float)value2.Height / (float)info.Height);
		int num2 = Mathf.Max(1, Mathf.RoundToInt((float)info.Width * num));
		int num3 = Mathf.Max(1, Mathf.RoundToInt((float)info.Height * num));
		float num4 = (float)(value2.Width - num2) * 0.5f / (float)value2.Width;
		float num5 = (float)(value2.Height - num3) * 0.5f / (float)value2.Height;
		uiTex.drawRegion = new Vector4(num4, num5, 1f - num4, 1f - num5);
		uiTex.uvRect = info.UvRect;
		UpdatePortraitFade(uiTex, value2, num3);
	}

	public static void TickPortrait(UIBundleLoader loader)
	{
		if (loader == null || !loader.isActiveAndEnabled || !lightFallbackPaths.Contains(loader.AssetPath))
		{
			return;
		}
		UITexture uiTex = loader.UiTex;
		if (!(uiTex == null) && uiTex.enabled && !(uiTex.mainTexture == null))
		{
			AssetTexture.TextureInfo textureInfo = ((AssetTexture)texture.GetValue(loader))?.GetTextureInfo();
			if (textureInfo != null && textureInfo.Tex == uiTex.mainTexture)
			{
				FitHeroPortrait(loader, textureInfo);
			}
		}
	}

	private static readonly Dictionary<string, Texture2D> iconOverrides = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	// Icons SPRK ships as PNGs embedded in this assembly, named SprkIcons.<Folder>.<Name>.png for
	// the sprite path Icon/<Folder>/<Name>. Used for art the game never had, such as Maya's
	// framed perk icons (docs/heroes/maya-art.md). Returns false for every other path.
	public static bool TryIconTexture(string spritePath, out Texture2D texture)
	{
		texture = null;
		if (spritePath == null || !spritePath.StartsWith("Icon/", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (iconOverrides.TryGetValue(spritePath, out texture))
		{
			return texture != null;
		}
		string resource = "SprkIcons." + spritePath.Substring(5).Replace('/', '.') + ".png";
		using (Stream stream = typeof(SprkImageRecovery).Assembly.GetManifestResourceStream(resource))
		{
			if (stream != null)
			{
				byte[] bytes = new byte[(int)stream.Length];
				int read;
				for (int i = 0; i < bytes.Length; i += read)
				{
					read = stream.Read(bytes, i, bytes.Length - i);
					if (read == 0)
					{
						throw new EndOfStreamException();
					}
				}
				texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
				if (texture.LoadImage(bytes, markNonReadable: true))
				{
					texture.name = "SPRK " + spritePath;
					texture.hideFlags = HideFlags.HideAndDontSave;
				}
				else
				{
					UnityEngine.Object.Destroy(texture);
					texture = null;
				}
			}
		}
		iconOverrides[spritePath] = texture;
		return texture != null;
	}

	public static bool TryTpIcon(UIBundleLoader loader)
	{
		if (loader == null)
		{
			return false;
		}
		string assetPath = loader.AssetPath;
		int num = (string.Equals(assetPath, "Icon/Item/TRANSCEND_10POINT", StringComparison.OrdinalIgnoreCase) ? 10 : (string.Equals(assetPath, "Icon/Item/TRANSCEND_15POINT", StringComparison.OrdinalIgnoreCase) ? 15 : (string.Equals(assetPath, "Icon/Item/SPRK_XDD", StringComparison.OrdinalIgnoreCase) ? 9900001 : 0)));
		if (num == 0)
		{
			return false;
		}
		if (!tpIcons.TryGetValue(num, out var value) || value == null)
		{
			using Stream stream = typeof(SprkImageRecovery).Assembly.GetManifestResourceStream("SprkTpIcons." + num + ".png");
			if (stream == null)
			{
				throw new InvalidOperationException("Missing TP icon: " + num);
			}
			byte[] array = new byte[(int)stream.Length];
			int num2;
			for (int i = 0; i < array.Length; i += num2)
			{
				num2 = stream.Read(array, i, array.Length - i);
				if (num2 == 0)
				{
					throw new EndOfStreamException();
				}
			}
			value = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
			if (!value.LoadImage(array, markNonReadable: true))
			{
				throw new InvalidOperationException("Invalid TP icon: " + num);
			}
			value.name = ((num == 9900001) ? "xdd" : ("SPRK " + num + " TP"));
			value.hideFlags = HideFlags.HideAndDontSave;
			tpIcons[num] = value;
		}
		UITexture uiTex = loader.UiTex;
		if (uiTex != null)
		{
			uiTex.material = null;
			uiTex.mainTexture = value;
			uiTex.uvRect = new Rect(0f, 0f, 1f, 1f);
		}
		return true;
	}

	private static void RemoveTime(object owner, FieldInfo times, FieldInfo order, string key)
	{
		((IDictionary)times.GetValue(owner)).Remove(key);
		SortedList sortedList = (SortedList)order.GetValue(owner);
		for (int num = sortedList.Count - 1; num >= 0; num--)
		{
			if (object.Equals(sortedList.GetByIndex(num), key))
			{
				sortedList.RemoveAt(num);
			}
		}
	}

	public static void BeforeCachedBundle(AssetCacheService cache, string key)
	{
		IDictionary dictionary = (IDictionary)bundleCache.GetValue(cache);
		if (dictionary.Contains(key) && !((UnityEngine.Object)dictionary[key] != null))
		{
			dictionary.Remove(key);
			((IDictionary)assetCache.GetValue(cache)).Remove(key);
			RemoveTime(cache, bundleTimes, bundleOrder, key);
		}
	}

	public static void BeforeCachedAsset(AssetCacheService cache, string bundle, string key)
	{
		if (((IDictionary)assetCache.GetValue(cache))[bundle] is IDictionary dictionary && dictionary.Contains(key) && (UnityEngine.Object)dictionary[key] == null)
		{
			dictionary.Remove(key);
		}
	}

	public static void BeforeCachedResource(AssetCacheService cache, string key)
	{
		IDictionary dictionary = (IDictionary)resourceCache.GetValue(cache);
		if (dictionary.Contains(key) && !((UnityEngine.Object)dictionary[key] != null))
		{
			dictionary.Remove(key);
			RemoveTime(cache, resourceTimes, resourceOrder, key);
		}
	}

	public static void Reset(UIBundleLoader loader)
	{
		RestorePortrait(loader);
		states.Remove(loader);
	}

	internal static void PrepareAtlas(string path)
	{
		AtlasDataContainer.AtlasLink atlasLink = SingletonAuto<AtlasDataContainer>.instance.GetAtlasLink(path);
		if (atlasLink == null || atlases.ContainsKey(atlasLink.AtlasCode) || !(SingletonAuto<NguiAtlasContainer>.instance.Lock(atlasLink.AtlasCode) is AtlasLoad atlasLoad))
		{
			return;
		}
		bool flag = false;
		try
		{
			IAsyncLoad<UIAtlas> asyncLoad = atlasLoad;
			if (asyncLoad.IsDone && (!asyncLoad.IsValid || !(asyncLoad.Value != null) || !(asyncLoad.Value.texture != null)) && atlasLoad.AssetAsync != null)
			{
				atlasLoad.AssetAsync.Close(destroyImmediately: true);
				atlasLoad.AssetAsync = null;
				atlases.Add(atlasLink.AtlasCode, new AtlasRetry
				{
					Atlas = atlasLoad,
					At = Time.realtimeSinceStartup + 0.25f
				});
				flag = true;
			}
		}
		finally
		{
			if (!flag)
			{
				NguiAtlasContainer.ReleaseAtlas(atlasLoad, destroyImmediately: false);
			}
		}
	}

	public static void TickGlobal()
	{
		ProbePortraitRendering();
		ProbeLightPortraitFit();
		ProbeTpIcons();
		if (atlases.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, AtlasRetry> atlase in atlases)
		{
			if (Time.realtimeSinceStartup >= atlase.Value.At)
			{
				list.Add(atlase.Key);
			}
		}
		foreach (string item in list)
		{
			AtlasLoad atlas = atlases[item].Atlas;
			atlases.Remove(item);
			try
			{
				atlas.Open(item);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[SPRK Images] Atlas retry failed: " + ex.Message);
			}
			finally
			{
				NguiAtlasContainer.ReleaseAtlas(atlas, destroyImmediately: false);
			}
		}
	}

	private static void ProbePortraitRendering()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_LIGHT_PORTRAIT_RENDER_PROBE");
		if (!portraitRenderStarted && !string.IsNullOrEmpty(environmentVariable) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
		{
			portraitRenderStarted = true;
			new GameObject("SPRK portrait render fixture").AddComponent<SprkLightPortraitRenderProbe>().Run(environmentVariable);
		}
	}

	private static void ProbeLightPortraitFit()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_LIGHT_PORTRAIT_PROBE");
		if (portraitProbeStarted || string.IsNullOrEmpty(environmentVariable) || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
		{
			return;
		}
		portraitProbeStarted = true;
		GameObject gameObject = null;
		try
		{
			gameObject = new GameObject("SPRK Light portrait fixture");
			HeroPortraitComponent heroPortraitComponent = gameObject.AddComponent<HeroPortraitComponent>();
			heroPortraitComponent.HeroInfo = new CachedHeroInfo
			{
				CostumeIndex = 7280
			};
			GameObject gameObject2 = new GameObject("Portrait image");
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
			UITexture uITexture = gameObject2.AddComponent<UITexture>();
			UIBundleLoader uIBundleLoader = gameObject2.AddComponent<UIBundleLoader>();
			typeof(UIBundleLoader).GetField("_uiTex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(uIBundleLoader, uITexture);
			heroPortraitComponent.HeroBundleTex = uIBundleLoader;
			FieldInfo field = typeof(UIBundleLoader).GetField("_assetPath", BindingFlags.Instance | BindingFlags.NonPublic);
			string value = lightFallbackPortraits[7280];
			AssetTexture.TextureInfo textureInfo = new AssetTexture.TextureInfo
			{
				Width = 260,
				Height = 490,
				UvRect = new Rect(0.1f, 0.2f, 0.3f, 0.4f)
			};
			UIWidget.Pivot[] array = new UIWidget.Pivot[3]
			{
				UIWidget.Pivot.Center,
				UIWidget.Pivot.TopLeft,
				UIWidget.Pivot.BottomRight
			};
			foreach (UIWidget.Pivot pivot in array)
			{
				uITexture.pivot = pivot;
				uITexture.keepAspectRatio = UIWidget.AspectRatioSource.Free;
				uITexture.width = 200;
				uITexture.height = 450;
				uITexture.transform.localPosition = new Vector3(13f, 27f, 0f);
				field.SetValue(uIBundleLoader, value);
				Vector2 pivotOffset = uITexture.pivotOffset;
				Vector3 vector = uITexture.transform.localPosition + new Vector3((0.5f - pivotOffset.x) * 200f, (0.5f - pivotOffset.y) * 450f, 0f);
				FitHeroPortrait(uIBundleLoader, textureInfo);
				Vector4 drawingDimensions = uITexture.drawingDimensions;
				if (uITexture.width != 200 || uITexture.height != 450 || Mathf.Abs(drawingDimensions.w - drawingDimensions.y - 377f) > 0.01f || uITexture.uvRect != textureInfo.UvRect)
				{
					throw new Exception("Tall monster portrait was not fitted with original UVs and intact widget bounds");
				}
				if (!portraitLayouts.TryGetValue(uIBundleLoader, out var value2) || value2.Fade == null || value2.Fade.transform.parent != uITexture.transform || value2.Fade.width != 200 || value2.Fade.height != 450 || value2.Fade.depth != uITexture.depth + 1)
				{
					throw new Exception("Missing or incorrectly layered card-edge fade");
				}
				Color32[] pixels = value2.FadeTexture.GetPixels32();
				if (pixels[0].a != byte.MaxValue || pixels[510].a != byte.MaxValue || pixels[256].a != 0 || pixels[80].a != 0 || pixels[50].a == 0 || pixels[50].a == byte.MaxValue || pixels[460].a != pixels[50].a)
				{
					throw new Exception("Black edges must cover the gaps and fade smoothly into the portrait");
				}
				Color32[] array2 = pixels;
				for (int j = 0; j < array2.Length; j++)
				{
					Color32 color = array2[j];
					if (color.r != 0 || color.g != 0 || color.b != 0)
					{
						throw new Exception("Portrait fade is not black");
					}
				}
				if ((uITexture.transform.localPosition + new Vector3((drawingDimensions.x + drawingDimensions.z) * 0.5f, (drawingDimensions.y + drawingDimensions.w) * 0.5f, 0f) - vector).sqrMagnitude > 0.001f)
				{
					throw new Exception("Portrait pivot is not centered");
				}
				Vector3 position = value2.Fade.transform.localPosition + new Vector3((0.5f - pivotOffset.x) * (float)value2.Fade.width, (0.5f - pivotOffset.y) * (float)value2.Fade.height, 0f);
				if ((uITexture.transform.TransformPoint(position) - uITexture.transform.parent.TransformPoint(vector)).sqrMagnitude > 0.001f)
				{
					throw new Exception("Edge fade is not centered over the original card");
				}
				FitHeroPortrait(uIBundleLoader, textureInfo);
				if (uITexture.width != 200 || uITexture.height != 450 || value2.FadeTexture != value2.Fade.mainTexture)
				{
					throw new Exception("Repeated loads changed the card dimensions or fade");
				}
				UITexture fade = value2.Fade;
				uIBundleLoader.SetAssetPath("", UIHelper.EAssetTextureAssign.ClipBottom);
				if (uITexture.width != 200 || uITexture.height != 450 || uITexture.transform.localPosition != new Vector3(13f, 27f, 0f))
				{
					throw new Exception("Reused portrait failed to restore its layout");
				}
				if (fade != null && fade.gameObject.activeSelf)
				{
					throw new Exception("Fade leaked onto the next card");
				}
			}
			FieldInfo fieldInfo = null;
			FieldInfo[] fields = typeof(HeroPortraitComponent).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo2 in fields)
			{
				if (fieldInfo2.FieldType == typeof(HeroPortraitComponent.EPortraitMode))
				{
					fieldInfo = fieldInfo2;
					break;
				}
			}
			if (fieldInfo == null)
			{
				throw new Exception("Portrait mode backing field missing");
			}
			foreach (HeroPortraitComponent.EPortraitMode value4 in Enum.GetValues(typeof(HeroPortraitComponent.EPortraitMode)))
			{
				if (value4 != HeroPortraitComponent.EPortraitMode.Icon)
				{
					fieldInfo.SetValue(heroPortraitComponent, value4);
					if (heroPortraitComponent.PortraitMode != value4)
					{
						throw new Exception("Incorrect portrait mode field");
					}
					heroPortraitComponent.HeroInfo.CostumeIndex = 11106;
					field.SetValue(uIBundleLoader, lightFallbackPortraits[11106]);
					FitHeroPortrait(uIBundleLoader, textureInfo);
					if (uITexture.width != 200 || uITexture.height != 450 || Mathf.Abs(uITexture.drawingDimensions.w - uITexture.drawingDimensions.y - 377f) > 0.01f)
					{
						throw new Exception("Valance fallback did not fit in mode " + value4);
					}
					uIBundleLoader.SetAssetPath("", UIHelper.EAssetTextureAssign.ClipBottom);
				}
			}
			fieldInfo.SetValue(heroPortraitComponent, HeroPortraitComponent.EPortraitMode.Basic);
			heroPortraitComponent.HeroInfo.CostumeIndex = 680;
			field.SetValue(uIBundleLoader, lightFallbackPortraits[680]);
			AssetTexture.TextureInfo info = new AssetTexture.TextureInfo
			{
				Width = 124,
				Height = 124,
				UvRect = new Rect(0f, 0f, 0.5f, 0.5f)
			};
			FitHeroPortrait(uIBundleLoader, info);
			if (uITexture.width != 200 || uITexture.height != 450 || Mathf.Abs(uITexture.drawingDimensions.w - uITexture.drawingDimensions.y - 200f) > 0.01f)
			{
				throw new Exception("Square fallback portrait was stretched");
			}
			if (!portraitLayouts.TryGetValue(uIBundleLoader, out var value3) || value3.FadeTexture.GetPixels32()[128].a != byte.MaxValue)
			{
				throw new Exception("Square portrait gaps were not covered");
			}
			uIBundleLoader.SetAssetPath("", UIHelper.EAssetTextureAssign.ClipBottom);
			heroPortraitComponent.HeroInfo.CostumeIndex = 180;
			field.SetValue(uIBundleLoader, "Portrait/Costume/Hero/Kasel_Kasel_Trans");
			FitHeroPortrait(uIBundleLoader, textureInfo);
			if (uITexture.width != 200 || uITexture.height != 450)
			{
				throw new Exception("Dedicated Kasel portrait was modified");
			}
			if (portraitLayouts.TryGetValue(uIBundleLoader, out value3))
			{
				throw new Exception("Dedicated Kasel portrait acquired a fade");
			}
			File.WriteAllText(environmentVariable, "PASS: native centered portrait fit, opaque black gaps and smooth symmetric edge fades, multiple pivots, atlas UVs, repeat loading, pooled cleanup, square portraits and unchanged Kasel");
		}
		catch (Exception ex)
		{
			File.WriteAllText(environmentVariable, "FAIL: " + ex);
		}
		finally
		{
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			Application.Quit();
		}
	}

	public static void ProbeLoadingBackground()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_LOADING_BACKGROUND_PROBE");
		if (loadingProbeStarted || string.IsNullOrEmpty(environmentVariable))
		{
			return;
		}
		if (!loadingProbeAssets)
		{
			loadingProbeAssets = true;
			string text = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows");
			Dictionary<string, AssetInfo> dictionary = new Dictionary<string, AssetInfo>();
			string[] files = Directory.GetFiles(Path.Combine(text, "Assetbundle"), "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				string text2 = files[i].Substring(text.Length + 1).Replace('\\', '/');
				dictionary[text2] = new AssetInfo
				{
					EntryName = text2
				};
			}
			SingletonAuto<AssetFileManager>.instance.Setup(dictionary);
		}
		BundleManager instance = SingletonAuto<BundleManager>.instance;
		Define.PatchDownloaded = true;
		if (instance.IsReady && SingletonAuto<AtlasDataContainer>.instance.GetAtlasLink("LoadingTip/GamePlayTip/OrvelTown") != null)
		{
			loadingProbeStarted = true;
			new GameObject("SPRK loading background probe").AddComponent<SprkLoadingBackgroundProbe>().Run(environmentVariable);
		}
	}

	public static void WatchLoadingTip(LoadingGamePlayTipComponent view)
	{
		WatchLoading(view, view.BackgroundImageTexture, "_backgroundImageAsset");
	}

	public static void WatchLoadingHero(LoadingHeroIntroComponent view)
	{
		WatchLoading(view, view.HeroBackgroundImageTexture, "_imageAsset");
	}

	private static void WatchLoading(Component view, UITexture target, string field)
	{
		if (!(target == null))
		{
			SprkLoadingBackground sprkLoadingBackground = view.GetComponent<SprkLoadingBackground>();
			if (sprkLoadingBackground == null)
			{
				sprkLoadingBackground = view.gameObject.AddComponent<SprkLoadingBackground>();
			}
			sprkLoadingBackground.Open(view, target, view.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}

	private static void ProbeTpIcons()
	{
		string text = Environment.GetEnvironmentVariable("SPRK_COUPON_ITEM_PROBE") ?? Environment.GetEnvironmentVariable("SPRK_TP_ICON_PROBE");
		if (probedTpIcons || string.IsNullOrEmpty(text))
		{
			return;
		}
		probedTpIcons = true;
		GameObject gameObject = null;
		try
		{
			gameObject = new GameObject("SPRK TP icon probe");
			gameObject.AddComponent<UITexture>();
			UIBundleLoader uIBundleLoader = gameObject.AddComponent<UIBundleLoader>();
			typeof(UIBundleLoader).GetField("_uiTex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(uIBundleLoader, gameObject.GetComponent<UITexture>());
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_COUPON_ITEM_PROBE")))
			{
				ItemBaseDataContainer itemBaseDataContainer = new ItemBaseDataContainer();
				((JitPacker)typeof(JitDataContainer<ItemBaseData>).GetProperty("Packer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(itemBaseDataContainer, null)).Deserialize(File.ReadAllBytes(Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/TableJit/ItemTable.jit")));
				ItemBaseData data = itemBaseDataContainer.GetData(9900001);
				if (data == null || data.Code != "SPRK_XDD" || data.Type != ItemType.Etc || !data.NotForSale || data.SellGold != 0 || data.Breakable)
				{
					throw new InvalidOperationException("xdd native item flags mismatch");
				}
				if (itemBaseDataContainer.GetDataByCode("SPRK_XDD") != data || !itemBaseDataContainer.GetItemDataListByType(ItemType.Etc).Contains(data))
				{
					throw new InvalidOperationException("xdd missing from native Others index");
				}
				uIBundleLoader.SetAssetPath(UIHelper.GetItemIconPath(data.Code));
				Texture2D texture2D = uIBundleLoader.UiTex.mainTexture as Texture2D;
				if (texture2D == null || texture2D.name != "xdd" || texture2D.width != 96 || texture2D.height != 96)
				{
					throw new InvalidOperationException("xdd icon did not bind through native loader");
				}
				uIBundleLoader.SetAssetPath("");
				if (uIBundleLoader.UiTex.mainTexture != null)
				{
					throw new InvalidOperationException("Pooled icon retained xdd art");
				}
				uIBundleLoader.SetAssetPath(UIHelper.GetItemIconPath(data.Code));
				if (uIBundleLoader.UiTex.mainTexture != texture2D)
				{
					throw new InvalidOperationException("xdd icon cache mismatch");
				}
				gameObject.SetActive(value: false);
				gameObject.SetActive(value: true);
				if (uIBundleLoader.UiTex.mainTexture != texture2D)
				{
					throw new InvalidOperationException("xdd icon lost on reuse");
				}
				File.WriteAllText(text, "{\"success\":true,\"itemIndex\":9900001,\"others\":true,\"notForSale\":true,\"nativeIcon\":true,\"poolReset\":true,\"cacheReuse\":true}");
			}
			else
			{
				uIBundleLoader.SetAssetPath(UIHelper.GetItemIconPath("TRANSCEND_10POINT"));
				Texture mainTexture = uIBundleLoader.UiTex.mainTexture;
				if (mainTexture == null || mainTexture.name != "SPRK 10 TP" || mainTexture.width != 124 || mainTexture.height != 124)
				{
					throw new InvalidOperationException("10 TP did not bind through the native loader");
				}
				uIBundleLoader.SetAssetPath(UIHelper.GetItemIconPath("TRANSCEND_15POINT"));
				Texture mainTexture2 = uIBundleLoader.UiTex.mainTexture;
				if (mainTexture2 == null || mainTexture2.name != "SPRK 15 TP" || mainTexture2 == mainTexture)
				{
					throw new InvalidOperationException("15 TP did not bind a distinct icon");
				}
				uIBundleLoader.SetAssetPath("");
				if (uIBundleLoader.UiTex.mainTexture != null)
				{
					throw new InvalidOperationException("Pooled icon retained stale TP art");
				}
				uIBundleLoader.SetAssetPath(UIHelper.GetItemIconPath("TRANSCEND_10POINT"));
				if (uIBundleLoader.UiTex.mainTexture != mainTexture)
				{
					throw new InvalidOperationException("10 TP texture was not cached");
				}
				gameObject.SetActive(value: false);
				gameObject.SetActive(value: true);
				if (uIBundleLoader.UiTex.mainTexture != mainTexture)
				{
					throw new InvalidOperationException("TP icon did not survive re-enable");
				}
				File.WriteAllText(text, "{\"success\":true,\"distinctIcons\":true,\"poolReset\":true,\"cacheReuse\":true,\"reenable\":true}");
			}
		}
		catch (Exception ex)
		{
			File.WriteAllText(text, "{\"success\":false,\"error\":\"" + ex.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")
				.Replace("\r", "")
				.Replace("\n", "\\n") + "\"}");
		}
		finally
		{
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			Application.Quit();
		}
	}

	public static void Tick(UIBundleLoader loader)
	{
		if (!Application.isPlaying || loader == null || !loader.isActiveAndEnabled)
		{
			return;
		}
		State orCreateValue = states.GetOrCreateValue(loader);
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup < orCreateValue.Poll)
		{
			return;
		}
		orCreateValue.Poll = realtimeSinceStartup + 0.25f;
		string assetPath = loader.AssetPath;
		if (assetPath != orCreateValue.Path)
		{
			orCreateValue.Path = assetPath;
			orCreateValue.Attempts = 0;
			orCreateValue.MissingSince = realtimeSinceStartup;
			orCreateValue.Released = (orCreateValue.Disabled = false);
		}
		if (orCreateValue.Disabled || string.IsNullOrEmpty(assetPath))
		{
			return;
		}
		UITexture uiTex = loader.UiTex;
		if (uiTex == null || !uiTex.enabled || !uiTex.gameObject.activeInHierarchy || uiTex.finalAlpha <= 0f)
		{
			return;
		}
		try
		{
			if (orCreateValue.Released)
			{
				if (!(realtimeSinceStartup < orCreateValue.ReloadAfter))
				{
					orCreateValue.Released = false;
					orCreateValue.MissingSince = realtimeSinceStartup;
					load.Invoke(loader, null);
				}
				return;
			}
			if (uiTex.mainTexture != null)
			{
				orCreateValue.Attempts = 0;
				orCreateValue.MissingSince = realtimeSinceStartup;
				return;
			}
			AssetTexture assetTexture = (AssetTexture)texture.GetValue(loader);
			AssetTexture.TextureInfo textureInfo = assetTexture?.GetTextureInfo();
			if (textureInfo != null && textureInfo.Tex != null)
			{
				complete.Invoke(loader, null);
			}
			else if (!(realtimeSinceStartup - orCreateValue.MissingSince < ((assetTexture != null && !assetTexture.IsDone) ? 8f : 0.5f)) && orCreateValue.Attempts < 3)
			{
				orCreateValue.Attempts++;
				PrepareAtlas(assetPath);
				bool flag = (bool)releaseOnClose.GetValue(loader);
				try
				{
					releaseOnClose.SetValue(loader, true);
					release.Invoke(loader, null);
				}
				finally
				{
					releaseOnClose.SetValue(loader, flag);
				}
				orCreateValue.Released = true;
				orCreateValue.ReloadAfter = realtimeSinceStartup + ((orCreateValue.Attempts == 1) ? 0.5f : ((orCreateValue.Attempts == 2) ? 1f : 2f));
				if (orCreateValue.Attempts == 1)
				{
					Debug.Log("[SPRK Images] Retrying thumbnail: " + assetPath);
				}
			}
		}
		catch (Exception ex)
		{
			orCreateValue.Disabled = true;
			Debug.LogWarning("[SPRK Images] Thumbnail recovery stopped: " + ex.Message);
		}
	}
}
