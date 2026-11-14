using System;
using System.Collections.Generic;
using System.IO;
using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

public static class SprkPetPresentation
{
	private static UIAtlas cleanFrameAtlas;

	private static bool probeStarted;

	public static IList<PortalRenewalData> Contents(IList<PortalRenewalData> original, PortalRenewalCategory category)
	{
		if (category != PortalRenewalCategory.Pet)
		{
			return original;
		}
		List<PortalRenewalData> obj = ((original == null) ? new List<PortalRenewalData>() : new List<PortalRenewalData>(original));
		PetDestination(obj, PortalRenewalSubCategory.MENUBUTTON, SprkText.Get("Pets"), ClientActivityType.OpenPetHouse);
		PetDestination(obj, PortalRenewalSubCategory.Pet, SprkText.Get("Pet House"), ClientActivityType.OpenPetHouse);
		PetDestination(obj, PortalRenewalSubCategory.PetIncubator, SprkText.Get("See Hatching"), ClientActivityType.OpenPetIncubator);
		PetDestination(obj, PortalRenewalSubCategory.PetHouse, SprkText.Get("Visit Lil' Raider House"), ClientActivityType.OpenPetHouse);
		return obj;
	}

	private static void PetDestination(List<PortalRenewalData> rows, PortalRenewalSubCategory sub, string title, ClientActivityType activity)
	{
		foreach (PortalRenewalData row in rows)
		{
			if (row.Category == PortalRenewalCategory.Pet && row.SubCategory == sub)
			{
				return;
			}
		}
		rows.Add(new PortalRenewalData
		{
			Category = PortalRenewalCategory.Pet,
			SubCategory = sub,
			TitleLocalKey = title,
			MenuButtonLocalkey = title,
			MainImagePath = "Portal_PetHouse_Image",
			ContentsShowIndex = rows.Count,
			SubComponentShape = PortalRenewalCompType.Window,
			MoveTaskType = activity,
			MoveTaskValue = new string[0]
		});
	}

	private static Texture2D CopyFrame(UIAtlas atlas, UISpriteData sprite)
	{
		Texture texture = atlas.texture;
		RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		RenderTexture active = RenderTexture.active;
		try
		{
			Graphics.Blit(texture, temporary);
			RenderTexture.active = temporary;
			Texture2D texture2D = new Texture2D(sprite.Width, sprite.Height, TextureFormat.RGBA32, mipChain: false);
			texture2D.ReadPixels(new Rect(sprite.X, texture.height - sprite.Y - sprite.Height, sprite.Width, sprite.Height), 0, 0);
			texture2D.Apply();
			return texture2D;
		}
		finally
		{
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	private static bool Gold(Color32 c)
	{
		if (c.a > 100 && c.r >= 150 && c.g >= 130 && c.b >= 80 && c.r >= c.g)
		{
			return (float)(int)c.g > (float)(int)c.b * 1.03f;
		}
		return false;
	}

	private static void RemoveOuterBorder(Texture2D texture)
	{
		int width = texture.width;
		int height = texture.height;
		Color32[] pixels = texture.GetPixels32();
		int[] array = new int[height];
		int[] array2 = new int[height];
		for (int i = 0; i < height; i++)
		{
			array[i] = width;
			array2[i] = -1;
			for (int j = 0; j < width; j++)
			{
				if (Gold(pixels[i * width + j]))
				{
					array[i] = Mathf.Min(array[i], j);
					array2[i] = j;
				}
			}
		}
		for (int k = 0; k < height; k++)
		{
			int num = width;
			int num2 = -1;
			for (int l = Mathf.Max(0, k - 2); l <= Mathf.Min(height - 1, k + 2); l++)
			{
				num = Mathf.Min(num, array[l] - 2);
				num2 = Mathf.Max(num2, array2[l] + 2);
			}
			for (int m = 0; m < width; m++)
			{
				if (m < num || m > num2)
				{
					pixels[k * width + m] = new Color32(0, 0, 0, 0);
				}
			}
		}
		texture.SetPixels32(pixels);
		texture.Apply();
	}

	private static void CleanWindowFrame(UISprite frame)
	{
		if (frame == null || frame.spriteName != "Common_Fram_Small2" || frame.atlas == null || frame.atlas == cleanFrameAtlas)
		{
			return;
		}
		if (cleanFrameAtlas == null)
		{
			UISpriteData sprite = frame.atlas.GetSprite(frame.spriteName);
			if (sprite == null || sprite.Width != 166 || sprite.Height != 161 || sprite.atlasScale != 1f)
			{
				return;
			}
			Texture2D texture2D = CopyFrame(frame.atlas, sprite);
			RemoveOuterBorder(texture2D);
			texture2D.name = "SPRK My Info gold frame";
			texture2D.wrapMode = TextureWrapMode.Clamp;
			GameObject gameObject = new GameObject("SPRK My Info frame atlas");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			cleanFrameAtlas = gameObject.AddComponent<UIAtlas>();
			Material material = new Material(frame.atlas.spriteMaterial);
			material.mainTexture = texture2D;
			UnityEngine.Object.DontDestroyOnLoad(material);
			UnityEngine.Object.DontDestroyOnLoad(texture2D);
			cleanFrameAtlas.spriteMaterial = material;
			cleanFrameAtlas.pixelSize = frame.atlas.pixelSize;
			cleanFrameAtlas.atlasWidth = texture2D.width;
			cleanFrameAtlas.atlasHeight = texture2D.height;
			UISpriteData uISpriteData = new UISpriteData();
			uISpriteData.CopyFrom(sprite);
			uISpriteData.SetRect(0, 0, sprite.Width, sprite.Height);
			cleanFrameAtlas.AddSprite(uISpriteData);
		}
		frame.atlas = cleanFrameAtlas;
	}

	public static PortalRenewalCompType Shape(PortalRenewalCompType original, PortalRenewalData data)
	{
		if (data.Category != PortalRenewalCategory.Pet)
		{
			return original;
		}
		return PortalRenewalCompType.Window;
	}

	public static void Setup(MyInfoWindow window)
	{
		if (window == null)
		{
			return;
		}
		Transform transform = window.transform.Find("Panel/Root/Sprite_Frame");
		if (transform != null)
		{
			CleanWindowFrame(transform.GetComponent<UISprite>());
		}
		Transform transform2 = window.transform.Find("Panel/Root/WidgetAvatar/Sprite_TextureDeco");
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(value: false);
		}
		Transform transform3 = window.transform.Find("Panel/Root/CategoryRoot/Button_Buff");
		if (transform3 == null)
		{
			return;
		}
		transform3.gameObject.SetActive(value: true);
		UIButton component = transform3.GetComponent<UIButton>();
		if (component != null)
		{
			EventDelegate.Set(component.onClick, delegate
			{
				ShowBuffViews(window);
			});
		}
	}

	public static void ShowBuffViews(MyInfoWindow window)
	{
		if (!(window == null))
		{
			MessagePopup3Button.Open(SprkText.Get("Buffs"), SprkText.Get("Choose which buffs to view."), PetTotalBuffPopup.OpenPopup, delegate
			{
				OpenTeamLevelBuffs(window);
			}, SprkText.Get("Pet Buffs"), SprkText.Get("Team Level Buffs"));
		}
	}

	public static void OpenTeamLevelBuffs(MyInfoWindow window)
	{
		if (window != null)
		{
			window.OnClickTeamLevelBuff();
		}
	}

	public static void EnsureProbe()
	{
		if (!probeStarted && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_PET_PORTAL_TEST")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
		{
			probeStarted = true;
			GameObject gameObject = new GameObject("SprkPetPortalTests");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<SprkPetPortalTests>();
		}
		else if (!probeStarted && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_MYINFO_FRAME_TEST")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
		{
			probeStarted = true;
			GameObject gameObject2 = new GameObject("SprkMyInfoFrameTests");
			UnityEngine.Object.DontDestroyOnLoad(gameObject2);
			gameObject2.AddComponent<SprkMyInfoFrameTests>();
		}
	}

	public static void RunProbe()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_MYINFO_FRAME_TEST");
		Directory.CreateDirectory(environmentVariable);
		UISprite component = Resources.Load<GameObject>("Prefab2/Window/GameOption/MyInfoWindow").transform.Find("Panel/Root/Sprite_Frame").GetComponent<UISprite>();
		UIAtlas atlas = component.atlas;
		UISpriteData sprite = atlas.GetSprite(component.spriteName);
		Texture2D texture2D = CopyFrame(atlas, sprite);
		Color32[] pixels = texture2D.GetPixels32();
		File.WriteAllBytes(Path.Combine(environmentVariable, "before.png"), texture2D.EncodeToPNG());
		GameObject gameObject = new GameObject("FrameProbe");
		gameObject.SetActive(value: false);
		UISprite uISprite = gameObject.AddComponent<UISprite>();
		uISprite.atlas = atlas;
		uISprite.spriteName = component.spriteName;
		uISprite.type = UIBasicSprite.Type.Sliced;
		uISprite.width = component.width;
		uISprite.height = component.height;
		CleanWindowFrame(uISprite);
		UIAtlas atlas2 = uISprite.atlas;
		CleanWindowFrame(uISprite);
		if (atlas2 == atlas || uISprite.atlas != atlas2 || component.atlas != atlas || atlas.GetSprite(sprite.name) != sprite)
		{
			throw new Exception("Frame scope/reuse regression");
		}
		Texture2D texture2D2 = (Texture2D)uISprite.atlas.texture;
		Color32[] pixels2 = texture2D2.GetPixels32();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < pixels.Length; i++)
		{
			if (Gold(pixels[i]))
			{
				num2++;
				if (!pixels[i].Equals(pixels2[i]))
				{
					throw new Exception("Gold frame pixel changed");
				}
			}
			if (pixels[i].a > 0 && pixels2[i].a == 0)
			{
				num++;
			}
		}
		if (num < 3000 || num2 < 500 || pixels2[13288].a != 0 || uISprite.width != component.width || uISprite.height != component.height || uISprite.border != component.border)
		{
			throw new Exception("Dark border removal or native geometry regression");
		}
		File.WriteAllBytes(Path.Combine(environmentVariable, "after.png"), texture2D2.EncodeToPNG());
		Color32[] pixels3 = CopyFrame(atlas, sprite).GetPixels32();
		for (int j = 0; j < pixels.Length; j++)
		{
			if (!pixels[j].Equals(pixels3[j]))
			{
				throw new Exception("Shared atlas changed");
			}
		}
		File.WriteAllText(Path.Combine(environmentVariable, "result.txt"), "PASS native My Info sprite: outer border removed=" + num + ", gold pixels preserved=" + num2 + ", native nine-slice geometry, shared atlas isolation and reopen reuse");
		UnityEngine.Object.Destroy(gameObject);
	}
}
