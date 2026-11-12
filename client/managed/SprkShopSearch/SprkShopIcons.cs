using System;
using System.Reflection;
using NGame2.NUI.NComponent2.NPayShop;
using NNguiExtension;
using UnityEngine;

public static class SprkShopIcons
{
	private static UIAtlas starAtlas;

	private static string starSprite;

	private static Texture2D magnifier;

	private static readonly string[] StarNames = new string[2] { "Common_Star", "Common_Star_2" };

	public static bool Find(string[] names, out UIAtlas atlas, out string sprite)
	{
		atlas = null;
		sprite = null;
		foreach (string text in names)
		{
			UIAtlas[] array = Resources.FindObjectsOfTypeAll<UIAtlas>();
			foreach (UIAtlas uIAtlas in array)
			{
				if (uIAtlas != null && uIAtlas.GetSprite(text) != null)
				{
					atlas = uIAtlas;
					sprite = text;
					return true;
				}
			}
		}
		return false;
	}

	public static bool Star(out UIAtlas atlas, out string sprite)
	{
		if (starAtlas == null)
		{
			Find(StarNames, out starAtlas, out starSprite);
		}
		atlas = starAtlas;
		sprite = starSprite;
		return atlas != null;
	}

	public static Texture2D Magnifier()
	{
		if (magnifier != null)
		{
			return magnifier;
		}
		Color32[] array = new Color32[9216];
		float num = 40f;
		float num2 = 56f;
		float num3 = 27f;
		float num4 = 19f;
		for (int i = 0; i < 96; i++)
		{
			for (int j = 0; j < 96; j++)
			{
				float num5 = (float)j + 0.5f;
				float num6 = (float)i + 0.5f;
				float num7 = Mathf.Sqrt((num5 - num) * (num5 - num) + (num6 - num2) * (num6 - num2));
				float a = Mathf.Clamp01(Mathf.Min(num7 - num4 + 0.75f, num3 - num7 + 0.75f));
				Vector2 vector = new Vector2(num + num3 * 0.68f, num2 - num3 * 0.68f);
				Vector2 vector2 = new Vector2(84f, 12f);
				Vector2 vector3 = new Vector2(num5, num6);
				float num8 = Mathf.Clamp01(Vector2.Dot(vector3 - vector, vector2 - vector) / (vector2 - vector).sqrMagnitude);
				float b = Mathf.Clamp01(7.5f - Vector2.Distance(vector3, vector + (vector2 - vector) * num8) + 0.75f);
				byte a2 = (byte)(Mathf.Max(a, b) * 255f);
				array[i * 96 + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a2);
			}
		}
		magnifier = new Texture2D(96, 96, TextureFormat.RGBA32, mipChain: false);
		magnifier.wrapMode = TextureWrapMode.Clamp;
		magnifier.SetPixels32(array);
		magnifier.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		magnifier.name = "SprkShopMagnifier";
		return magnifier;
	}

	public static void ApplyCategoryIcon(PayShopCategoryComponent component, bool search)
	{
		if (component.CategoryImageSelector == null)
		{
			return;
		}
		UISprite uISprite = typeof(UISpriteSelector).GetField("_targetSprite", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component.CategoryImageSelector) as UISprite;
		if (uISprite == null)
		{
			uISprite = component.CategoryImageSelector.GetComponent<UISprite>();
		}
		if (uISprite == null)
		{
			uISprite = component.CategoryImageSelector.GetComponentInChildren<UISprite>(includeInactive: true);
		}
		if (uISprite == null)
		{
			Debug.LogWarning("[SprkShopSearch] category icon sprite not found");
			return;
		}
		Transform transform = uISprite.transform.parent.Find("SprkSearchIcon");
		if (!search)
		{
			if (transform != null)
			{
				transform.gameObject.SetActive(value: false);
			}
			uISprite.enabled = true;
			return;
		}
		UITexture uITexture = ((transform != null) ? transform.GetComponent<UITexture>() : null);
		if (uITexture == null)
		{
			uITexture = NGUITools.AddWidget<UITexture>(uISprite.transform.parent.gameObject);
			uITexture.name = "SprkSearchIcon";
			uITexture.shader = Shader.Find("Unlit/Transparent Colored");
		}
		uITexture.gameObject.SetActive(value: true);
		uITexture.mainTexture = Magnifier();
		int num = Math.Min(uISprite.width, uISprite.height);
		uITexture.width = (int)((float)num * 0.82f);
		uITexture.height = uITexture.width;
		uITexture.depth = uISprite.depth;
		uITexture.color = uISprite.color;
		uITexture.transform.localPosition = uISprite.transform.localPosition;
		uITexture.transform.localScale = uISprite.transform.localScale;
		uISprite.enabled = false;
	}
}
