using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NWindow;
using NGameAsset;
using NNguiExtension;
using NShared.NJit;
using NVespa;
using UnityEngine;

/// <summary>
/// Prefab-fixture probe (docs/client-automation.md): renders My Info on the
/// Login screen, builds the Profile tab and checks layout, tab switching, the
/// picture override and cropping. `show` leaves the Profile tab open with a
/// fixture picture for a screenshot; `show-empty` without a picture.
/// </summary>
public static class SprkProfileImageProbe
{
	private static GameObject shown;

	private static readonly Dictionary<string, List<UIAtlas>> atlases = new Dictionary<string, List<UIAtlas>>();

	private static string othersResult;

	public static string Run(string mode)
	{
		if (mode == "others")
		{
			othersResult = null;
			SprkProfileImage.Runner.StartCoroutine(Guard(CheckOthers(delegate(string r)
			{
				othersResult = "PASS " + r;
			})));
			return "started";
		}
		if (mode == "others-result")
		{
			return othersResult ?? "pending";
		}
		if (mode == "friend-slot" || mode == "friend-slot-show")
		{
			return FriendSlot(mode == "friend-slot-show");
		}
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		string text = CheckCrop();
		MyInfoWindow myInfoWindow = Show();
		SprkProfileImageView sprkProfileImageView = SprkProfileImageView.Setup(myInfoWindow);
		Check(sprkProfileImageView != null, "view not built");
		Check(SprkProfileImageView.Setup(myInfoWindow) == sprkProfileImageView, "setup is not idempotent");
		Transform transform = myInfoWindow.transform.Find("Panel/Root/CategoryRoot");
		Check(transform.Cast<Transform>().Count((Transform t) => t.name == SprkProfileImageView.TabName) == 1, "tab duplicated");
		Transform transform2 = transform.Find(SprkProfileImageView.TabName);
		Transform transform3 = transform.Find("Button_Buff");
		Check(Mathf.Approximately(transform2.localPosition.y, -162f) && Mathf.Approximately(transform2.localPosition.x, transform3.localPosition.x), "tab position " + transform2.localPosition);
		Check(transform2.Find("Label").GetComponent<UILabel>().text == SprkText.Get("Profile"), "tab label " + transform2.Find("Label").GetComponent<UILabel>().text);
		Check(!sprkProfileImageView.ProfileShown && sprkProfileImageView.CommonContent.All((GameObject g) => g.activeSelf), "common tab not shown first");
		Check(sprkProfileImageView.CommonTab.GetComponent<UISprite>().spriteName == "Mission_Button_02" && transform2.GetComponent<UISprite>().spriteName == "Mission_Button_03", "initial tab sprites");
		sprkProfileImageView.ShowProfile();
		Check(sprkProfileImageView.ProfileShown && sprkProfileImageView.CommonContent.All((GameObject g) => !g.activeSelf), "profile tab content");
		Check(sprkProfileImageView.CommonTab.GetComponent<UISprite>().spriteName == "Mission_Button_03" && transform2.GetComponent<UISprite>().spriteName == "Mission_Button_02", "selected tab sprites");
		UITexture uiTex = sprkProfileImageView.Preview.UiTex;
		Check(uiTex != null && uiTex.width == 440 && uiTex.height == 440 && uiTex.shader != null, "preview texture");
		Check(sprkProfileImageView.UploadButton.onClick.Count == 1 && sprkProfileImageView.RemoveButton.onClick.Count == 1, "button handlers");
		Check(!sprkProfileImageView.RemoveButton.isEnabled, "remove enabled without a picture");
		string text2 = sprkProfileImageView.Status.text;
		Texture2D texture2D = Fixture();
		SprkProfileImage.SetPicture(texture2D);
		Check(uiTex.mainTexture == texture2D, "picture not applied to the watched preview");
		Check(sprkProfileImageView.RemoveButton.isEnabled && sprkProfileImageView.Status.text != text2, "status/remove not refreshed");
		GameObject gameObject = new GameObject("SprkProfileUnwatched");
		gameObject.transform.SetParent(shown.transform, worldPositionStays: false);
		UIBundleLoader uIBundleLoader = gameObject.AddComponent<UIBundleLoader>();
		uIBundleLoader.SetUITexture(gameObject.AddComponent<UITexture>());
		Check(!SprkProfileImage.TryApply(uIBundleLoader), "unwatched loader overridden");
		UnityEngine.Object.DestroyImmediate(gameObject);
		sprkProfileImageView.ShowCommon();
		Check(sprkProfileImageView.CommonContent.All((GameObject g) => g.activeSelf) && !sprkProfileImageView.ProfileShown, "back to common");
		string result = "PASS profile tab at " + transform2.localPosition + ", label '" + transform2.Find("Label").GetComponent<UILabel>().text + "', preview 440x440, help label height " + sprkProfileImageView.Panel.transform.Find("Label_ProfileHelp").GetComponent<UILabel>().height + "; " + text;
		if (mode == "show" || mode == "show-empty")
		{
			if (mode == "show-empty")
			{
				SprkProfileImage.SetPicture(null);
			}
			sprkProfileImageView.ShowProfile();
			UIPanel[] componentsInChildren = shown.GetComponentsInChildren<UIPanel>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].alpha = 1f;
			}
			// The Login UIRoot uses another scale than the lobby: fit the
			// window's root (1921x1089 units) to the screen height, centred.
			WindowAnimation[] componentsInChildren2 = shown.GetComponentsInChildren<WindowAnimation>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
			UIWidget component = shown.transform.Find("Panel/Root").GetComponent<UIWidget>();
			Camera camera = NGUITools.FindCameraForLayer(component.gameObject.layer);
			Vector3[] worldCorners = component.worldCorners;
			float num = camera.WorldToScreenPoint(worldCorners[1]).y - camera.WorldToScreenPoint(worldCorners[0]).y;
			Check(num > 1f, "window not on screen");
			shown.transform.localScale *= (float)Screen.height * 0.98f / num;
			shown.transform.position += shown.transform.parent.position - component.transform.position;
			return result + "; shown at " + num.ToString("0") + "px -> " + ((float)Screen.height * 0.98f).ToString("0") + "px";
		}
		SprkProfileImage.SetPicture(null);
		Check(uiTex.mainTexture != texture2D, "picture not cleared");
		UnityEngine.Object.DestroyImmediate(shown);
		return result;
	}

	private static IEnumerator Guard(IEnumerator inner)
	{
		while (true)
		{
			try
			{
				if (!inner.MoveNext())
				{
					break;
				}
			}
			catch (Exception ex)
			{
				othersResult = "FAIL " + ex.Message;
				break;
			}
			yield return inner.Current;
		}
	}

	/// <summary>Other players: a bound native portrait shows the player's picture and gets its icon back.</summary>
	private static IEnumerator CheckOthers(Action<string> done)
	{
		GameObject gameObject = new GameObject("SprkProfileOthersFixture");
		UITexture uITexture = gameObject.AddComponent<UITexture>();
		Texture2D texture2D = new Texture2D(4, 4);
		Texture2D texture2D2 = new Texture2D(4, 4);
		Texture2D texture2D3 = Fixture();
		uITexture.mainTexture = texture2D;
		uITexture.uvRect = new Rect(0.1f, 0.1f, 0.5f, 0.5f);
		try
		{
			SprkProfileImage.SetRemotePicture(900001L, texture2D3);
			SprkProfileImage.Bind(uITexture, 900001L);
			yield return null;
			Check(uITexture.mainTexture == texture2D3 && uITexture.uvRect == new Rect(0f, 0f, 1f, 1f), "player picture not shown");
			// The slot is reused for a player without a picture: their icon returns.
			SprkProfileImage.Bind(uITexture, 900002L);
			Check(uITexture.mainTexture == texture2D && uITexture.uvRect == new Rect(0.1f, 0.1f, 0.5f, 0.5f), "native icon not restored on rebind");
			yield return null;
			Check(uITexture.mainTexture == texture2D, "picture shown for the wrong player");
			// Native code loads a new icon while the picture is shown: the picture stays, the new icon is kept for later.
			SprkProfileImage.Bind(uITexture, 900001L);
			yield return null;
			uITexture.mainTexture = texture2D2;
			yield return null;
			Check(uITexture.mainTexture == texture2D3, "native reload replaced the picture");
			SprkProfileImage.SetRemotePicture(900001L, null);
			yield return null;
			Check(uITexture.mainTexture == texture2D2, "removed picture not replaced by the latest native icon");
			SprkProfileImage.Bind(uITexture, 0L);
			yield return null;
			Check(uITexture.mainTexture == texture2D2, "unbound texture changed");
			done("others: bind, rebind, native reload, removal");
		}
		finally
		{
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(texture2D);
			UnityEngine.Object.Destroy(texture2D2);
		}
	}

	private static string CheckCrop()
	{
		// 300x200: 50px red bars left and right of a green centre; the centred
		// square crop must keep only green, scaled to Side x Side.
		Texture2D texture2D = new Texture2D(300, 200, TextureFormat.RGBA32, mipChain: false);
		Color32[] array = new Color32[60000];
		for (int i = 0; i < 200; i++)
		{
			for (int j = 0; j < 300; j++)
			{
				array[i * 300 + j] = ((j < 50 || j >= 250) ? new Color32(byte.MaxValue, 0, 0, byte.MaxValue) : new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
		}
		texture2D.SetPixels32(array);
		texture2D.Apply();
		byte[] data = texture2D.EncodeToPNG();
		byte[] data2 = new Texture2D(8, 8, TextureFormat.RGBA32, mipChain: false).EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		byte[] array2 = SprkProfileImage.Crop(data);
		Check(array2 != null, "crop failed");
		Texture2D texture2D2 = new Texture2D(2, 2);
		Check(texture2D2.LoadImage(array2) && texture2D2.width == 256 && texture2D2.height == 256, "crop size " + texture2D2.width + "x" + texture2D2.height);
		int num = 0;
		Color32[] pixels = texture2D2.GetPixels32();
		foreach (Color32 color in pixels)
		{
			if (color.r > 64)
			{
				num++;
			}
		}
		UnityEngine.Object.Destroy(texture2D2);
		Check(num < 256 * 4, "red border kept after crop: " + num + " pixels");
		Check(SprkProfileImage.Crop(data2) == null, "8x8 image accepted");
		Check(SprkProfileImage.Crop(new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }) == null, "garbage accepted");
		Check(SprkProfileImage.Crop(null) == null, "null accepted");
		return "crop 300x200 -> " + array2.Length + " byte 256x256 PNG, red pixels left=" + num;
	}

	private static Texture2D Fixture()
	{
		Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, mipChain: false);
		Color32[] array = new Color32[65536];
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				bool flag = (i / 32 + j / 32) % 2 == 0;
				array[i * 256 + j] = new Color32((byte)j, (byte)(flag ? 200 : 60), (byte)i, byte.MaxValue);
			}
		}
		texture2D.SetPixels32(array);
		texture2D.Apply();
		texture2D.name = "SPRK profile probe fixture";
		return texture2D;
	}

	private static MyInfoWindow Show()
	{
		if (shown != null)
		{
			UnityEngine.Object.DestroyImmediate(shown);
		}
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		shown = UnityEngine.Object.Instantiate(VespaResource.Load<GameObject>(MyInfoWindow.PrefabPath), uIRoot.transform, worldPositionStays: false);
		shown.name = "ProfileImageShow_MyInfoWindow";
		UIPanel[] componentsInChildren = shown.GetComponentsInChildren<UIPanel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth += 500;
		}
		shown.SetActive(value: true);
		bool patchDownloaded = Define.PatchDownloaded;
		Define.PatchDownloaded = true;
		try
		{
			AtlasAssignerBase[] componentsInChildren2 = shown.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Assign();
			}
		}
		finally
		{
			Define.PatchDownloaded = patchDownloaded;
		}
		LoadAtlases(shown);
		// What SprkPetPresentation.Setup does in the real window.
		shown.transform.Find("Panel/Root/CategoryRoot/Button_Buff").gameObject.SetActive(value: true);
		return shown.GetComponent<MyInfoWindow>();
	}

	private static UIAtlas heroIcons;

	/// <summary>
	/// Two friend slots with a real hero icon: the native layout on top, the
	/// fixed one (portrait above the frame's star strip) below.
	/// `friend-slot-show` leaves them on screen for a screenshot.
	/// </summary>
	private static string FriendSlot(bool keep)
	{
		if (shown != null)
		{
			UnityEngine.Object.DestroyImmediate(shown);
		}
		UIRoot uIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		shown = new GameObject("ProfileImageShow_FriendSlots");
		shown.layer = uIRoot.gameObject.layer;
		shown.transform.SetParent(uIRoot.transform, worldPositionStays: false);
		UIPanel uIPanel = shown.AddComponent<UIPanel>();
		uIPanel.depth = 600;
		GameObject prefab = VespaResource.Load<GameObject>(FriendSlotComponent.PrefabPath);
		Check(prefab != null, "friend slot prefab missing");
		FriendSlotComponent[] slots = new FriendSlotComponent[2];
		for (int i = 0; i < 2; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, shown.transform, worldPositionStays: false);
			gameObject.name = ((i == 0) ? "Native" : "Fixed");
			gameObject.SetActive(value: true);
			slots[i] = gameObject.GetComponent<FriendSlotComponent>();
		}
		bool patchDownloaded = Define.PatchDownloaded;
		Define.PatchDownloaded = true;
		try
		{
			AtlasAssignerBase[] componentsInChildren = shown.GetComponentsInChildren<AtlasAssignerBase>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].Assign();
			}
		}
		finally
		{
			Define.PatchDownloaded = patchDownloaded;
		}
		LoadAtlases(shown);
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(slots[0].transform);
		slots[1].transform.localPosition = slots[0].transform.localPosition - new Vector3(0f, bounds.size.y + 20f, 0f);
		UIAtlas uIAtlas = HeroIcons();
		UISpriteData uISpriteData = ((uIAtlas != null) ? uIAtlas.GetSprite("Clause_1") : null);
		Check(uISpriteData != null, "hero icon atlas not loaded");
		Texture texture = uIAtlas.texture;
		Rect uvRect = new Rect((float)uISpriteData.X / (float)texture.width, 1f - (float)(uISpriteData.Y + uISpriteData.Height) / (float)texture.height, (float)uISpriteData.Width / (float)texture.width, (float)uISpriteData.Height / (float)texture.height);
		UITexture[] textures = new UITexture[2];
		UISprite[] frames = new UISprite[2];
		for (int k = 0; k < 2; k++)
		{
			textures[k] = slots[k].transform.Find("Portrait/Texture_Hero").GetComponent<UITexture>();
			frames[k] = slots[k].transform.Find("Portrait/Sprite_PortraiFrame").GetComponent<UISprite>();
			textures[k].mainTexture = texture;
			textures[k].uvRect = uvRect;
		}
		UITexture uITexture = textures[1];
		UISprite uISprite = frames[1];
		string text = "texture " + uITexture.width + "x" + uITexture.height + " at " + uITexture.transform.localPosition + " depth " + uITexture.depth + ", frame '" + uISprite.spriteName + "' " + uISprite.width + "x" + uISprite.height + " at " + uISprite.transform.localPosition + " depth " + uISprite.depth;
		Check(uISprite.spriteName == "Common_Hero_Frame_Small", "frame sprite " + uISprite.spriteName);
		slots[1].SprkPortraitAboveFrame();
		Check(uITexture.depth > uISprite.depth, "portrait still below frame: " + uITexture.depth + " <= " + uISprite.depth);
		int depth = uITexture.depth;
		slots[1].SprkPortraitAboveFrame();
		Check(uITexture.depth == depth, "not idempotent");
		Check(textures[0].depth <= frames[0].depth, "native slot changed");
		string result = "PASS friend slot before: " + text + "; after: texture depth " + uITexture.depth;
		if (!keep)
		{
			UnityEngine.Object.DestroyImmediate(shown);
			return result;
		}
		// Fit both slots on screen, centred, above the Login screen.
		uIPanel.depth = 5000;
		uIPanel.alpha = 1f;
		Camera camera = NGUITools.FindCameraForLayer(shown.layer);
		Bounds bounds2 = NGUIMath.CalculateAbsoluteWidgetBounds(shown.transform);
		Vector3 vector = camera.WorldToScreenPoint(bounds2.max) - camera.WorldToScreenPoint(bounds2.min);
		float num = vector.y;
		Check(num > 1f && vector.x > 1f, "slots not on screen");
		shown.transform.localScale *= Mathf.Min((float)Screen.height * 0.9f / num, (float)Screen.width * 0.9f / vector.x);
		Bounds bounds3 = NGUIMath.CalculateAbsoluteWidgetBounds(shown.transform);
		shown.transform.position += uIRoot.transform.position - bounds3.center;
		return result + "; shown at " + num.ToString("0") + "px";
	}

	private static UIAtlas HeroIcons()
	{
		if (heroIcons != null)
		{
			return heroIcons;
		}
		string path = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle/NguiAtlas_Icon_Creature_Hero1.unity3d");
		AssetBundle assetBundle = (File.Exists(path) ? AssetBundle.LoadFromFile(path) : null);
		if (assetBundle == null)
		{
			return null;
		}
		GameObject[] array = assetBundle.LoadAllAssets<GameObject>();
		foreach (GameObject gameObject in array)
		{
			UIAtlas component = gameObject.GetComponent<UIAtlas>();
			if (component != null)
			{
				heroIcons = component;
				break;
			}
		}
		return heroIcons;
	}

	private static void LoadAtlases(GameObject root)
	{
		foreach (string item in (from a in root.GetComponentsInChildren<UISpriteAtlasAssigner>(includeInactive: true)
			where !string.IsNullOrEmpty(a.AtlasName) && (a.Target == null || a.Target.atlas == null)
			select a.AtlasName).Distinct())
		{
			if (atlases.ContainsKey(item))
			{
				continue;
			}
			List<UIAtlas> list = (atlases[item] = new List<UIAtlas>());
			string path = Path.Combine(Application.dataPath, "Documents/Patch/StandaloneWindows/Assetbundle/HighAndMidSpec/NguiAtlas_UI_Prefab_" + item + ".unity3d");
			AssetBundle assetBundle = (File.Exists(path) ? AssetBundle.LoadFromFile(path) : null);
			if (assetBundle == null)
			{
				continue;
			}
			GameObject[] array = assetBundle.LoadAllAssets<GameObject>();
			foreach (GameObject gameObject in array)
			{
				UIAtlas component = gameObject.GetComponent<UIAtlas>();
				if (component != null)
				{
					list.Add(component);
				}
			}
			list.AddRange(from a in assetBundle.LoadAllAssets<UIAtlas>()
				where !list.Contains(a)
				select a);
		}
		List<UIAtlas> source = atlases.Values.SelectMany((List<UIAtlas> l) => l).ToList();
		UISprite[] componentsInChildren = root.GetComponentsInChildren<UISprite>(includeInactive: true);
		foreach (UISprite sprite in componentsInChildren)
		{
			if (sprite.atlas == null && !string.IsNullOrEmpty(sprite.spriteName))
			{
				UIAtlas uIAtlas = source.FirstOrDefault((UIAtlas a) => a.GetSprite(sprite.spriteName) != null);
				if (uIAtlas != null)
				{
					string spriteName = sprite.spriteName;
					sprite.atlas = uIAtlas;
					sprite.spriteName = spriteName;
				}
			}
		}
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Profile image: " + message);
		}
	}
}
