using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LitJson;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public sealed class SprkLoginPresentation : MonoBehaviour
{
	[Serializable]
	public sealed class Background
	{
		public string id;

		public string label;

		public string scene;

		public string sourceVersion;

		public string sourceSha256;

		public string logo;
	}

	[Serializable]
	public sealed class Music
	{
		public string id;

		public string label;

		public string file;
	}

	[Serializable]
	public sealed class Catalog
	{
		public int schema;

		public Background[] backgrounds;

		public Music[] music;
	}

	public const string BackgroundKey = "Sprk.Login.Background";

	public const string DefaultBackground = "x-the-final-azeta-and-valance";

	public const string MusicKey = "Sprk.Login.Music";

	public const string DefaultMusic = "e47c58d47a86adfa0c40";

	private static Catalog catalog;

	private static SprkLoginPresentation activeView;

	private static AudioClip nativeMusic;

	private AudioClip customMusic;

	private string customMusicId;

	private int musicGeneration;

	private LoginBackground view;

	private SprkRecoveredLoginScene recovered;

	private string appliedBackground;

	private readonly Dictionary<GameObject, bool> originalVisibility = new Dictionary<GameObject, bool>();

	public const string CornerLogoSprite = "Logo_Rebellion_White";

	private static Texture restoredLogoTexture;

	private bool restoringLogo;

	public static string LogoStatus = "not started";

	private static readonly string[] nativeArt = new string[9] { "LoginIllust", "LoginIllustForeGround", "LoginBackgroundSpineHeros", "LoginBackgroundSpineDay", "LoginBackgroundSpineNight", "LoginEffectBackGround", "EffectBI", "LoignLetterBoxLeft", "LoignLetterBoxRight" };

	public const string EraLogoName = "SPRK Era Logo";

	private UISprite nativeLogo;

	private UITexture eraLogo;

	private Texture2D eraLogoTexture;

	private float nativeLogoAlpha = 1f;

	public const float MaxCoverScale = 2f;

	private const int CoverSampleFrames = 45;

	private Transform coverRoot;

	private Vector3 coverBaseScale;

	private Vector3 coverBasePosition;

	private Rect coverArt;

	private bool coverHasArt;

	private int coverSamples;

	private const int PixelChecks = 3;

	private int pixelChecks;

	private bool pixelCheckRunning;

	public const float DesignHalfWidth = 960f;

	private float logoRightInset = -1f;

	private bool messageUnanchored;

	private static bool probeStarted;

	public GameObject RecoveredRoot
	{
		get
		{
			if (recovered != null)
			{
				return recovered.Root;
			}
			return null;
		}
	}

	public string LoadedMusicId => customMusicId;

	public AudioClip LoadedMusic => customMusic;

	public static string ContentRoot
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_CONTENT");
			if (!string.IsNullOrEmpty(environmentVariable) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")))
			{
				return environmentVariable;
			}
			return Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/LoginPresentation");
		}
	}

	public static Catalog Options
	{
		get
		{
			if (catalog != null)
			{
				return catalog;
			}
			JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(Path.Combine(ContentRoot, "catalog.json")));
			catalog = new Catalog
			{
				schema = jsonData["schema"].valueInt,
				backgrounds = new Background[jsonData["backgrounds"].Count],
				music = new Music[jsonData["music"].Count]
			};
			for (int i = 0; i < catalog.backgrounds.Length; i++)
			{
				JsonData fromArray = jsonData["backgrounds"].GetFromArray(i);
				catalog.backgrounds[i] = new Background
				{
					id = fromArray["id"].valueStr,
					label = fromArray["label"].valueStr,
					scene = fromArray["scene"].valueStr,
					sourceVersion = fromArray["sourceVersion"].valueStr,
					sourceSha256 = fromArray["sourceSha256"].valueStr,
					logo = (fromArray.Contains("logo") ? fromArray["logo"].valueStr : null)
				};
			}
			for (int j = 0; j < catalog.music.Length; j++)
			{
				JsonData fromArray2 = jsonData["music"].GetFromArray(j);
				catalog.music[j] = new Music
				{
					id = fromArray2["id"].valueStr,
					label = fromArray2["label"].valueStr,
					file = fromArray2["file"].valueStr
				};
			}
			if (catalog.schema != 1 || catalog.backgrounds.Length == 0)
			{
				throw new InvalidOperationException("The login presentation catalog is invalid.");
			}
			return catalog;
		}
	}

	public static bool AtLogin
	{
		get
		{
			if (SceneAutoBehaviour<LoginManagement>.isValidInstance)
			{
				return !SceneAutoBehaviour<LobbyManagement>.isValidInstance;
			}
			return false;
		}
	}

	public static string SelectedBackground
	{
		get
		{
			string text = SprkTestPrefs.GetString("Sprk.Login.Background", "x-the-final-azeta-and-valance");
			string id = NormalizeBackgroundId(text);
			if (!Options.backgrounds.Any((Background x) => x.id == id))
			{
				id = "x-the-final-azeta-and-valance";
			}
			if (id != text)
			{
				SprkTestPrefs.SetString("Sprk.Login.Background", id);
				SprkTestPrefs.Save();
			}
			return id;
		}
	}

	public static string SelectedMusic
	{
		get
		{
			string text = SprkTestPrefs.GetString("Sprk.Login.Music", "e47c58d47a86adfa0c40");
			string id = NormalizeMusicId(text);
			if (!Options.music.Any((Music x) => x.id == id))
			{
				id = "e47c58d47a86adfa0c40";
			}
			if (id != text)
			{
				SprkTestPrefs.SetString("Sprk.Login.Music", id);
				SprkTestPrefs.Save();
			}
			return id;
		}
	}

	public static string BackgroundCaption => Caption(Options.backgrounds.First((Background x) => x.id == SelectedBackground).label);

	public static string MusicCaption
	{
		get
		{
			string id = SelectedMusic;
			return Caption(Options.music.First((Music x) => x.id == id).label);
		}
	}

	// Song names are proper titles; translate their version/language suffix only.
	public static string Caption(string label)
	{
		string translated = SprkText.Get(label);
		if (translated != label) return translated;
		int separator = label.LastIndexOf(" — ", StringComparison.Ordinal);
		return separator < 0 ? label : label.Substring(0, separator + 3) + SprkText.Get(label.Substring(separator + 3));
	}

	public static bool LogoSubtitleRestored => restoredLogoTexture != null;

	public UITexture EraLogo
	{
		get
		{
			if (!(eraLogo != null) || !eraLogo.gameObject.activeInHierarchy)
			{
				return null;
			}
			return eraLogo;
		}
	}

	public UISprite NativeLogo => nativeLogo;

	public bool NativeLogoVisible
	{
		get
		{
			if (nativeLogo != null && nativeLogo.enabled)
			{
				return nativeLogo.alpha > 0f;
			}
			return false;
		}
	}

	public float CoverScale { get; private set; }

	public Rect CoverArt => coverArt;

	public Rect CoverView { get; private set; }

	public int PixelCorrections { get; private set; }

	public float MessageLineOffset { get; private set; }

	public static string NormalizeBackgroundId(string id)
	{
		switch (id)
		{
		case "13-two-characters":
		case "13-two-characters-night":
		case "native":
			return "x-the-final-azeta-and-valance";
		case "01-original-party":
			return "original-heroes";
		case "02-cathedral-party":
			return "heroes-of-orvel";
		case "03-war-table":
			return "war-council";
		case "04-fire-dragon":
			return "dragons-challenge";
		case "05-hero-ensemble":
			return "gathered-heroes";
		case "07-red-sky-layers":
		case "06-red-sky":
			return "pandemonium";
		case "08-light-and-dark":
			return "soul-weapon-light";
		case "08-light-and-dark-night":
			return "soul-weapon-darkness";
		case "09-fallen-fate-spine":
		case "09-fallen-fate-spine-night":
			return "ix-fallen-fate";
		case "11-day-night-night":
		case "11-day-night":
		case "10-ruins-four-heroes":
		case "10-ruins-four-heroes-night":
			return "x-rebellion";
		case "12-kasel-frey":
		case "12-kasel-frey-night":
			return "x-the-final-kasel-and-frey";
		default:
			return id;
		}
	}

	public static string NormalizeMusicId(string id)
	{
		if (!(id == "native") && !(id == "silent"))
		{
			return id;
		}
		return "e47c58d47a86adfa0c40";
	}

	public static void SelectBackground(string id)
	{
		id = NormalizeBackgroundId(id);
		if (!Options.backgrounds.Any((Background x) => x.id == id))
		{
			throw new ArgumentException("Unknown login background");
		}
		SprkTestPrefs.SetString("Sprk.Login.Background", id);
		SprkTestPrefs.Save();
		if (activeView != null && AtLogin && activeView.gameObject.activeInHierarchy)
		{
			activeView.ApplyBackground();
		}
	}

	public static void SelectMusic(string id)
	{
		id = NormalizeMusicId(id);
		if (!Options.music.Any((Music x) => x.id == id))
		{
			throw new ArgumentException("Unknown login music");
		}
		SprkTestPrefs.SetString("Sprk.Login.Music", id);
		SprkTestPrefs.Save();
		RefreshMusic();
	}

	public static void Configure(LoginBackground window)
	{
		// The headless battle worker (-batchmode) has no screen and ships without
		// login backgrounds or music; skip the presentation entirely.
		if (Application.isBatchMode)
		{
			return;
		}
		SprkLoginPresentation sprkLoginPresentation = window.GetComponent<SprkLoginPresentation>();
		if (sprkLoginPresentation == null)
		{
			sprkLoginPresentation = window.gameObject.AddComponent<SprkLoginPresentation>();
		}
		sprkLoginPresentation.view = window;
		activeView = sprkLoginPresentation;
		sprkLoginPresentation.ApplyBackground();
		sprkLoginPresentation.ApplyMusic();
		if (!sprkLoginPresentation.restoringLogo)
		{
			sprkLoginPresentation.StartCoroutine(sprkLoginPresentation.RestoreLogoWhenReady());
		}
	}

	private static RectInt SpriteRect(object atlas, string name)
	{
		object data = atlas.GetType().GetMethod("GetSprite", new Type[1] { typeof(string) }).Invoke(atlas, new object[1] { name });
		if (data == null)
		{
			throw new InvalidOperationException("Logo sprite missing");
		}
		Func<string, int> func = delegate(string f)
		{
			FieldInfo field = data.GetType().GetField(f, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return Convert.ToInt32(field.GetValue(data));
			}
			PropertyInfo property = data.GetType().GetProperty(f, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return Convert.ToInt32(property.GetValue(data, null));
			}
			throw new InvalidOperationException("Sprite data has no " + f + " on " + data.GetType().FullName);
		};
		return new RectInt(func("x"), func("y"), func("width"), func("height"));
	}

	private IEnumerator RestoreLogoWhenReady()
	{
		restoringLogo = true;
		LogoStatus = "waiting";
		for (int frame = 0; frame < 600; frame++)
		{
			if (!(view != null))
			{
				break;
			}
			UISprite uISprite = FindNativeLogo();
			Texture2D texture2D = ((uISprite == null || uISprite.atlas == null) ? null : (uISprite.mainTexture as Texture2D));
			if (texture2D != null)
			{
				if (texture2D != restoredLogoTexture)
				{
					try
					{
						RestoreLogoSubtitle(uISprite, texture2D);
						LogoStatus = "restored " + texture2D.name;
					}
					catch (Exception ex)
					{
						LogoStatus = "failed: " + ex.ToString().Replace(Environment.NewLine, " | ");
						Debug.LogWarning("SPRK login logo subtitle not restored: " + ex.Message);
					}
				}
				break;
			}
			yield return null;
		}
		restoringLogo = false;
	}

	private static void RestoreLogoSubtitle(UISprite sprite, Texture2D target)
	{
		string path = ((SystemOptionData.Spec == ESpecType.Low) ? "LowLargeSpec" : "HighAndMidSpec");
		AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle", path, "NguiAtlas_UI_Prefab_Login.unity3d"));
		if (assetBundle == null)
		{
			throw new InvalidOperationException("Login atlas bundle could not be opened");
		}
		try
		{
			UIAtlas uIAtlas = assetBundle.mainAsset as UIAtlas;
			if (uIAtlas == null)
			{
				uIAtlas = (from g in assetBundle.LoadAllAssets<GameObject>()
					select g.GetComponent<UIAtlas>()).FirstOrDefault((UIAtlas x) => x != null);
			}
			Texture2D texture2D = (from t in assetBundle.LoadAllAssets<Texture2D>()
				where t.width == target.width && t.height == target.height
				orderby t.name == target.name descending
				select t).FirstOrDefault();
			if (uIAtlas == null)
			{
				throw new InvalidOperationException("Login atlas bundle has no atlas");
			}
			if (texture2D == null)
			{
				throw new InvalidOperationException("Login atlas bundle has no texture");
			}
			RectInt rectInt = SpriteRect(uIAtlas, "Logo_Rebellion_White");
			RectInt rectInt2 = SpriteRect(sprite.atlas, "Logo_Rebellion_White");
			if (rectInt.width != rectInt2.width || rectInt.height != rectInt2.height || texture2D.format != target.format || SystemInfo.copyTextureSupport == CopyTextureSupport.None)
			{
				throw new InvalidOperationException("Login atlas layouts differ");
			}
			int num = System.Math.Min(texture2D.mipmapCount, target.mipmapCount);
			for (int num2 = 0; num2 < num; num2++)
			{
				int num3 = 1 << num2;
				int num4 = rectInt.width / num3;
				int num5 = rectInt.height / num3;
				if (num4 < 1 || num5 < 1)
				{
					break;
				}
				Graphics.CopyTexture(texture2D, 0, num2, rectInt.x / num3, (texture2D.height - rectInt.y - rectInt.height) / num3, num4, num5, target, 0, num2, rectInt2.x / num3, (target.height - rectInt2.y - rectInt2.height) / num3);
			}
			restoredLogoTexture = target;
		}
		finally
		{
			assetBundle.Unload(unloadAllLoadedObjects: true);
		}
	}

	private void HideNativeObject(GameObject go)
	{
		if (!(go == null))
		{
			if (!originalVisibility.ContainsKey(go))
			{
				originalVisibility.Add(go, go.activeSelf);
			}
			go.SetActive(value: false);
		}
	}

	private void HideNativeArt()
	{
		if (view.BackEffectPanelRoot != null)
		{
			HideNativeObject(view.BackEffectPanelRoot.gameObject);
		}
		if (view.WidgetEffect != null)
		{
			HideNativeObject(view.WidgetEffect.gameObject);
		}
		Transform[] componentsInChildren = view.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (nativeArt.Contains(transform.name) && (recovered == null || (!(transform == recovered.Root.transform) && !transform.IsChildOf(recovered.Root.transform))))
			{
				HideNativeObject(transform.gameObject);
			}
		}
	}

	private UISprite FindNativeLogo()
	{
		if (nativeLogo != null)
		{
			return nativeLogo;
		}
		Transform transform = view.transform.Find("UIPanel/Root/LoignLogo");
		nativeLogo = ((transform != null) ? transform.GetComponent<UISprite>() : null);
		if (nativeLogo == null)
		{
			nativeLogo = view.GetComponentsInChildren<UISprite>(includeInactive: true).FirstOrDefault((UISprite x) => x.spriteName == "Logo_Rebellion_White" && x.transform.parent != null && x.transform.parent.parent != null && x.transform.parent.parent.parent == view.transform);
		}
		return nativeLogo;
	}

	private void RemoveEraLogo()
	{
		if (eraLogo != null)
		{
			UnityEngine.Object.Destroy(eraLogo.gameObject);
		}
		eraLogo = null;
		if (eraLogoTexture != null)
		{
			UnityEngine.Object.Destroy(eraLogoTexture);
		}
		eraLogoTexture = null;
		if (nativeLogo != null)
		{
			nativeLogo.alpha = nativeLogoAlpha;
		}
	}

	private void LateUpdate()
	{
		if (!(view == null))
		{
			Camera camera = NGUITools.FindCameraForLayer(view.gameObject.layer);
			if (camera != null)
			{
				CoverScreen(camera);
				AlignLogo(camera);
			}
			AlignMessage();
			if (!(eraLogo == null) && !(nativeLogo == null))
			{
				Vector3[] worldCorners = nativeLogo.worldCorners;
				eraLogo.transform.position = (worldCorners[0] + worldCorners[2]) * 0.5f;
			}
		}
	}

	private void ResetCover()
	{
		if (coverRoot != null)
		{
			coverRoot.localScale = coverBaseScale;
			coverRoot.localPosition = coverBasePosition;
		}
		coverRoot = null;
		coverHasArt = false;
		coverSamples = 0;
		CoverScale = 1f;
		pixelChecks = 0;
		PixelCorrections = 0;
	}

	private void BeginCover()
	{
		ResetCover();
		if (recovered != null && !(recovered.Root == null))
		{
			coverRoot = recovered.Root.transform;
			coverBaseScale = coverRoot.localScale;
			coverBasePosition = coverRoot.localPosition;
		}
	}

	private void IncludeArt(Vector3 world)
	{
		Vector3 vector = coverRoot.InverseTransformPoint(world);
		if (!coverHasArt)
		{
			coverArt = new Rect(vector.x, vector.y, 0f, 0f);
			coverHasArt = true;
		}
		else
		{
			coverArt = Rect.MinMaxRect(Mathf.Min(coverArt.xMin, vector.x), Mathf.Min(coverArt.yMin, vector.y), Mathf.Max(coverArt.xMax, vector.x), Mathf.Max(coverArt.yMax, vector.y));
		}
	}

	private void SampleArt()
	{
		Renderer[] componentsInChildren = coverRoot.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.enabled && !(renderer is ParticleSystemRenderer) && !(renderer.GetComponent<UIDrawCall>() != null))
			{
				Bounds bounds = renderer.bounds;
				if (!(bounds.size.x <= 0f) && !(bounds.size.y <= 0f))
				{
					IncludeArt(bounds.min);
					IncludeArt(bounds.max);
				}
			}
		}
		UIWidget[] componentsInChildren2 = coverRoot.GetComponentsInChildren<UIWidget>();
		foreach (UIWidget uIWidget in componentsInChildren2)
		{
			if ((uIWidget is UISprite || uIWidget is UITexture) && uIWidget.enabled && !(uIWidget.finalAlpha < 0.01f) && uIWidget.name.IndexOf("LetterBox", StringComparison.OrdinalIgnoreCase) < 0 && !(uIWidget.color.maxColorComponent < 0.02f))
			{
				Vector3[] worldCorners = uIWidget.worldCorners;
				IncludeArt(worldCorners[0]);
				IncludeArt(worldCorners[2]);
			}
		}
	}

	private IEnumerator CheckEdgePixels(Camera camera, Transform root)
	{
		pixelCheckRunning = true;
		yield return new WaitForEndOfFrame();
		try
		{
			if (root != coverRoot || root == null || camera == null)
			{
				yield break;
			}
			int width = Screen.width;
			int height = Screen.height;
			if (width < 64 || height < 64)
			{
				yield break;
			}
			float[] rows = new float[8] { 0.22f, 0.3f, 0.38f, 0.46f, 0.54f, 0.62f, 0.7f, 0.78f };
			Texture2D texture2D = new Texture2D(width, rows.Length, TextureFormat.RGB24, mipChain: false);
			try
			{
				for (int i = 0; i < rows.Length; i++)
				{
					texture2D.ReadPixels(new Rect(0f, Mathf.RoundToInt((float)height * rows[i]), width, 1f), 0, i, recalculateMipMaps: false);
				}
				Color32[] pixels = texture2D.GetPixels32();
				Func<int, bool> func = delegate(int num6)
				{
					int num5 = 0;
					for (int j = 0; j < rows.Length; j++)
					{
						Color32 color = pixels[j * width + num6];
						if (color.r > 3 || color.g > 3 || color.b > 3)
						{
							num5++;
						}
					}
					return num5 * 2 >= rows.Length;
				};
				int num;
				for (num = 0; num < width && !func(num); num++)
				{
				}
				int num2 = width - 1;
				while (num2 >= 0 && !func(num2))
				{
					num2--;
				}
				if (num2 - num >= width / 3 && (num >= 2 || num2 <= width - 3))
				{
					float x = root.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(((float)num + 0.5f) / (float)width, 0.5f, 0f))).x;
					float x2 = root.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(((float)num2 + 0.5f) / (float)width, 0.5f, 0f))).x;
					float num3 = coverArt.xMin;
					float num4 = coverArt.xMax;
					if (num >= 2)
					{
						num3 = Mathf.Max(num3, Mathf.Min(x, x2));
					}
					if (num2 <= width - 3)
					{
						num4 = Mathf.Min(num4, Mathf.Max(x, x2));
					}
					if (num4 - num3 >= coverArt.width * 0.5f && (num3 != coverArt.xMin || num4 != coverArt.xMax))
					{
						coverArt = Rect.MinMaxRect(num3, coverArt.yMin, num4, coverArt.yMax);
						PixelCorrections++;
					}
				}
			}
			finally
			{
				UnityEngine.Object.Destroy(texture2D);
			}
		}
		finally
		{
			pixelCheckRunning = false;
		}
	}

	private void CoverScreen(Camera camera)
	{
		if (recovered == null || recovered.Root == null)
		{
			if (coverRoot != null)
			{
				ResetCover();
			}
			return;
		}
		if (coverRoot != recovered.Root.transform)
		{
			BeginCover();
		}
		if (coverSamples < 45)
		{
			coverSamples++;
			SampleArt();
		}
		else if (coverHasArt && pixelChecks < 3 && !pixelCheckRunning && (++coverSamples - 45) % 20 == 0)
		{
			pixelChecks++;
			StartCoroutine(CheckEdgePixels(camera, coverRoot));
		}
		if (coverHasArt && !(coverArt.width < 1f) && !(coverArt.height < 1f))
		{
			Transform parent = coverRoot.parent;
			Vector3 vector = parent.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)));
			Vector3 vector2 = parent.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f)));
			Rect rect = (CoverView = Rect.MinMaxRect(Mathf.Min(vector.x, vector2.x), Mathf.Min(vector.y, vector2.y), Mathf.Max(vector.x, vector2.x), Mathf.Max(vector.y, vector2.y)));
			float num = coverBasePosition.x + coverBaseScale.x * coverArt.xMin;
			float num2 = coverBasePosition.x + coverBaseScale.x * coverArt.xMax;
			float num3 = coverBasePosition.y + coverBaseScale.y * coverArt.yMin;
			float num4 = coverBasePosition.y + coverBaseScale.y * coverArt.yMax;
			Vector2 center = rect.center;
			float num5 = 1f;
			if (num < center.x)
			{
				num5 = Mathf.Max(num5, (center.x - rect.xMin) / (center.x - num));
			}
			if (num2 > center.x)
			{
				num5 = Mathf.Max(num5, (rect.xMax - center.x) / (num2 - center.x));
			}
			if (num3 < center.y)
			{
				num5 = Mathf.Max(num5, (center.y - rect.yMin) / (center.y - num3));
			}
			if (num4 > center.y)
			{
				num5 = Mathf.Max(num5, (rect.yMax - center.y) / (num4 - center.y));
			}
			if (num5 > 1f)
			{
				num5 = Mathf.Min(num5 * 1.004f, 2f);
			}
			CoverScale = num5;
			Vector3 vector3 = coverBaseScale * num5;
			Vector3 vector4 = new Vector3(center.x + (coverBasePosition.x - center.x) * num5, center.y + (coverBasePosition.y - center.y) * num5, coverBasePosition.z);
			if (coverRoot.localScale != vector3)
			{
				coverRoot.localScale = vector3;
			}
			if (coverRoot.localPosition != vector4)
			{
				coverRoot.localPosition = vector4;
			}
		}
	}

	private void AlignLogo(Camera camera)
	{
		UISprite uISprite = FindNativeLogo();
		if (uISprite == null)
		{
			return;
		}
		Transform transform = uISprite.transform;
		Transform parent = transform.parent;
		if (parent == null)
		{
			return;
		}
		if (logoRightInset < 0f)
		{
			if (uISprite.leftAnchor.target != null && uISprite.rightAnchor.target != null && uISprite.leftAnchor.relative == 1f && uISprite.rightAnchor.relative == 1f)
			{
				logoRightInset = (float)(-(uISprite.leftAnchor.absolute + uISprite.rightAnchor.absolute)) * 0.5f;
			}
			else
			{
				logoRightInset = 960f - transform.localPosition.x;
			}
			if (logoRightInset < 0f)
			{
				logoRightInset = 0f;
			}
			uISprite.SetAnchor((Transform)null);
		}
		float num = Mathf.Max(parent.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f))).x, parent.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f))).x) - logoRightInset;
		Vector3 localPosition = transform.localPosition;
		if (Mathf.Abs(localPosition.x - num) > 0.01f)
		{
			transform.localPosition = new Vector3(num, localPosition.y, localPosition.z);
		}
	}

	public static float TextCenterY(UILabel label)
	{
		float num = (1f - label.pivotOffset.y) * (float)label.height;
		float num2 = (0f - label.pivotOffset.y) * (float)label.height;
		float num3 = Mathf.Min(label.printedSize.y, label.height);
		switch (label.pivot)
		{
		case UIWidget.Pivot.TopLeft:
		case UIWidget.Pivot.Top:
		case UIWidget.Pivot.TopRight:
			return num - num3 * 0.5f;
		case UIWidget.Pivot.BottomLeft:
		case UIWidget.Pivot.Bottom:
		case UIWidget.Pivot.BottomRight:
			return num2 + num3 * 0.5f;
		default:
			return (num + num2) * 0.5f;
		}
	}

	private void AlignMessage()
	{
		UILabel label_Message = view.Label_Message;
		if (label_Message == null || !label_Message.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!messageUnanchored)
		{
			label_Message.SetAnchor((Transform)null);
			messageUnanchored = true;
		}
		if (!SprkTapToPlay.TryGetLineWorldY(out var y))
		{
			Transform transform = view.transform.Find("UIPanel/Root");
			if (transform == null)
			{
				return;
			}
			y = transform.TransformPoint(new Vector3(0f, -464f, 0f)).y;
		}
		Transform transform2 = label_Message.transform;
		float y2 = transform2.TransformPoint(new Vector3(0f, TextCenterY(label_Message), 0f)).y;
		Transform parent = transform2.parent;
		float num = ((parent == null) ? (y - y2) : (parent.InverseTransformPoint(new Vector3(0f, y, 0f)).y - parent.InverseTransformPoint(new Vector3(0f, y2, 0f)).y));
		MessageLineOffset = Mathf.Abs(num);
		if (Mathf.Abs(num) > 0.05f)
		{
			transform2.localPosition += new Vector3(0f, num, 0f);
		}
	}

	private void ApplyEraLogo(Background option)
	{
		RemoveEraLogo();
		UISprite uISprite = FindNativeLogo();
		if (!(uISprite == null) && !string.IsNullOrEmpty(option.logo))
		{
			Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true)
			{
				name = option.id + " logo"
			};
			if (!texture2D.LoadImage(File.ReadAllBytes(Path.Combine(ContentRoot, option.logo)), markNonReadable: true))
			{
				UnityEngine.Object.Destroy(texture2D);
				return;
			}
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.anisoLevel = 2;
			eraLogoTexture = texture2D;
			GameObject gameObject = new GameObject("SPRK Era Logo");
			gameObject.layer = uISprite.gameObject.layer;
			gameObject.transform.SetParent(uISprite.transform.parent, worldPositionStays: false);
			gameObject.transform.localRotation = uISprite.transform.localRotation;
			gameObject.transform.localScale = uISprite.transform.localScale;
			eraLogo = gameObject.AddComponent<UITexture>();
			eraLogo.shader = Shader.Find("Unlit/Transparent Colored");
			eraLogo.mainTexture = texture2D;
			eraLogo.depth = uISprite.depth;
			float num = Mathf.Min((float)uISprite.width * 0.82f / (float)texture2D.width, (float)uISprite.height * 0.8f / (float)texture2D.height);
			eraLogo.width = Mathf.Max(2, Mathf.RoundToInt((float)texture2D.width * num));
			eraLogo.height = Mathf.Max(2, Mathf.RoundToInt((float)texture2D.height * num));
			Vector3[] worldCorners = uISprite.worldCorners;
			gameObject.transform.position = (worldCorners[0] + worldCorners[2]) * 0.5f;
			nativeLogoAlpha = ((uISprite.alpha > 0f) ? uISprite.alpha : 1f);
			uISprite.alpha = 0f;
		}
	}

	private void RestoreNativeArt()
	{
		foreach (KeyValuePair<GameObject, bool> item in originalVisibility)
		{
			if (item.Key != null)
			{
				item.Key.SetActive(item.Value);
			}
		}
		originalVisibility.Clear();
	}

	public void ApplyBackground()
	{
		if (view == null || !AtLogin)
		{
			return;
		}
		string selected = SelectedBackground;
		if (selected == appliedBackground && recovered != null)
		{
			HideNativeArt();
			return;
		}
		ResetCover();
		if (recovered != null)
		{
			recovered.Dispose();
			recovered = null;
		}
		RestoreNativeArt();
		Background background = Options.backgrounds.First((Background x) => x.id == selected);
		recovered = new SprkRecoveredLoginScene(Path.Combine(ContentRoot, background.scene), view.transform);
		HideNativeArt();
		BeginCover();
		appliedBackground = selected;
		ApplyEraLogo(background);
	}

	public static AudioClip FilterMusic(AudioClip incoming)
	{
		if (Application.isBatchMode || !AtLogin)
		{
			return incoming;
		}
		if (incoming != null && (activeView == null || incoming != activeView.customMusic))
		{
			nativeMusic = incoming;
		}
		string selectedMusic = SelectedMusic;
		if (activeView != null && activeView.customMusic != null && activeView.customMusicId == selectedMusic)
		{
			return activeView.customMusic;
		}
		return incoming;
	}

	public static void RefreshMusic()
	{
		if (activeView != null && AtLogin && activeView.gameObject.activeInHierarchy)
		{
			activeView.ApplyMusic();
		}
	}

	private void ApplyMusic()
	{
		int generation = ++musicGeneration;
		string selected = SelectedMusic;
		if (customMusic != null && customMusicId == selected)
		{
			SingletonAuto<BgmManager2>.instance.PlayFromResources(customMusic, 1f);
			return;
		}
		StartCoroutine(LoadMusic(Options.music.First((Music x) => x.id == selected), generation));
	}

	private IEnumerator LoadMusic(Music option, int generation)
	{
		string text = Path.Combine(ContentRoot, option.file);
		using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(text).AbsoluteUri, text.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ? AudioType.OGGVORBIS : AudioType.WAV);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			throw new IOException("Login music could not be loaded: " + request.error);
		}
		AudioClip content = DownloadHandlerAudioClip.GetContent(request);
		content.name = option.label;
		if (generation != musicGeneration || !AtLogin || SelectedMusic != option.id)
		{
			UnityEngine.Object.Destroy(content);
			yield break;
		}
		AudioClip audioClip = customMusic;
		customMusic = content;
		customMusicId = option.id;
		SingletonAuto<BgmManager2>.instance.PlayFromResources(customMusic, 1f);
		if (audioClip != null)
		{
			UnityEngine.Object.Destroy(audioClip, 2f);
		}
	}

	private void OnDisable()
	{
		musicGeneration++;
		RemoveEraLogo();
		ResetCover();
		if (recovered != null)
		{
			recovered.Dispose();
			recovered = null;
			appliedBackground = null;
			RestoreNativeArt();
		}
	}

	private void OnDestroy()
	{
		musicGeneration++;
		RemoveEraLogo();
		ResetCover();
		if (recovered != null)
		{
			recovered.Dispose();
		}
		if (customMusic != null)
		{
			UnityEngine.Object.Destroy(customMusic);
		}
		if (activeView == this)
		{
			activeView = null;
		}
	}

	public static void Setup(GameOptionWindow window)
	{
		Transform transform = window.transform.Find("Panel/Root/EtcOption");
		if (!(transform == null) && !(window.EtcInfo.Button_Coupon == null))
		{
			SprkLoginPresentationMenu sprkLoginPresentationMenu = transform.GetComponent<SprkLoginPresentationMenu>();
			if (sprkLoginPresentationMenu == null)
			{
				sprkLoginPresentationMenu = transform.gameObject.AddComponent<SprkLoginPresentationMenu>();
			}
			sprkLoginPresentationMenu.Setup(window);
		}
	}

	public static void EnsureProbe()
	{
		if (!probeStarted && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_TEST")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")))
		{
			probeStarted = true;
			GameObject obj = new GameObject("SPRK login presentation probe");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.AddComponent<SprkLoginPresentationProbe>();
		}
	}
}
