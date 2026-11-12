using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using UnityEngine;

public sealed class SprkLightPortraitRenderProbe : MonoBehaviour
{
	private string report;

	private GameObject panelRoot;

	private GameObject cameraRoot;

	private RenderTexture target;

	private Texture2D picture;

	private Texture2D white;

	private static void CompleteCachedPortrait(UIBundleLoader loader, AssetTexture.TextureInfo info)
	{
		loader.SendMessage("ReleaseAssetTexture", SendMessageOptions.RequireReceiver);
		object obj = Activator.CreateInstance(typeof(AssetTexture).Assembly.GetType("NGameAsset.NAssetTexture.TextureFromNguiAtlas"));
		obj.GetType().GetField("_texInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, new AssetTexture.TextureInfo
		{
			Width = info.Width,
			Height = info.Height,
			Tex = info.Tex,
			Mtrl = info.Mtrl,
			UvRect = info.UvRect,
			SrcType = info.SrcType
		});
		AssetTexture assetTexture = new AssetTexture();
		typeof(AssetTexture).GetField("_load", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(assetTexture, obj);
		typeof(AssetTexture).GetProperty("IsDone").SetValue(assetTexture, true, null);
		typeof(UIBundleLoader).GetField("_assetTex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(loader, assetTexture);
		loader.SendMessage("OnComplete", SendMessageOptions.RequireReceiver);
		if (loader.UiTex.mainTexture != info.Tex)
		{
			throw new Exception("Native cached completion did not bind fixture texture");
		}
	}

	public void Run(string path)
	{
		report = path;
		StartCoroutine(Observe());
	}

	private IEnumerator Observe()
	{
		IEnumerator test = Check();
		while (true)
		{
			object obj = null;
			bool flag;
			try
			{
				flag = test.MoveNext();
				if (flag)
				{
					obj = test.Current;
				}
			}
			catch (Exception ex)
			{
				File.WriteAllText(report, "FAIL: " + ex);
				break;
			}
			if (!flag)
			{
				break;
			}
			yield return obj;
		}
		if (panelRoot != null)
		{
			UnityEngine.Object.Destroy(panelRoot);
		}
		if (cameraRoot != null)
		{
			UnityEngine.Object.Destroy(cameraRoot);
		}
		if (target != null)
		{
			target.Release();
			UnityEngine.Object.Destroy(target);
		}
		if (picture != null)
		{
			UnityEngine.Object.Destroy(picture);
		}
		if (white != null)
		{
			UnityEngine.Object.Destroy(white);
		}
		Application.Quit();
	}

	private IEnumerator Check()
	{
		yield return new WaitForSecondsRealtime(2f);
		panelRoot = new GameObject("SPRK portrait render panel");
		panelRoot.layer = 30;
		UIPanel panel = panelRoot.AddComponent<UIPanel>();
		panel.depth = 50;
		cameraRoot = new GameObject("SPRK portrait render camera");
		cameraRoot.layer = 30;
		Camera camera = cameraRoot.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 330f;
		camera.nearClipPlane = 0.1f;
		camera.farClipPlane = 100f;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = new Color(0.08f, 0.2f, 0.4f, 1f);
		camera.cullingMask = 1073741824;
		cameraRoot.AddComponent<UICamera>();
		camera.transform.position = new Vector3(0f, 0f, -10f);
		target = new RenderTexture(288, 660, 24, RenderTextureFormat.ARGB32);
		target.Create();
		camera.targetTexture = target;
		GameObject heroGo = new GameObject("HeroPortrait");
		heroGo.layer = 30;
		heroGo.transform.SetParent(panelRoot.transform, worldPositionStays: false);
		HeroPortraitComponent hero = heroGo.AddComponent<HeroPortraitComponent>();
		hero.HeroInfo = new CachedHeroInfo
		{
			CostumeIndex = 7280
		};
		UIWidget uIWidget = heroGo.AddComponent<UIWidget>();
		uIWidget.width = 328;
		uIWidget.height = 720;
		GameObject imageGo = new GameObject("Texture_HeroPortrait");
		imageGo.layer = 30;
		imageGo.transform.SetParent(heroGo.transform, worldPositionStays: false);
		UITexture ui = imageGo.AddComponent<UITexture>();
		ui.pivot = UIWidget.Pivot.Center;
		ui.width = 288;
		ui.height = 660;
		ui.depth = 102;
		ui.leftAnchor.target = heroGo.transform;
		ui.leftAnchor.relative = 0f;
		ui.leftAnchor.absolute = 20;
		ui.rightAnchor.target = heroGo.transform;
		ui.rightAnchor.relative = 1f;
		ui.rightAnchor.absolute = -20;
		ui.bottomAnchor.target = heroGo.transform;
		ui.bottomAnchor.relative = 0f;
		ui.bottomAnchor.absolute = 30;
		ui.topAnchor.target = heroGo.transform;
		ui.topAnchor.relative = 1f;
		ui.topAnchor.absolute = -30;
		ui.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
		ui.ResetAnchors();
		ui.UpdateAnchors();
		white = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
		white.SetPixels(new Color[4]
		{
			Color.white,
			Color.white,
			Color.white,
			Color.white
		});
		white.Apply();
		string sample = Environment.GetEnvironmentVariable("SPRK_LIGHT_PORTRAIT_RENDER_IMAGE");
		if (!string.IsNullOrEmpty(sample) && !white.LoadImage(File.ReadAllBytes(sample)))
		{
			throw new Exception("Could not load sample portrait");
		}
		ui.mainTexture = white;
		UIBundleLoader loader = (hero.HeroBundleTex = imageGo.AddComponent<UIBundleLoader>());
		BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		typeof(UIBundleLoader).GetField("_uiTex", flags).SetValue(loader, ui);
		typeof(UIBundleLoader).GetField("_assetPath", flags).SetValue(loader, "Portrait/Monster/Ordeal/LightPancironne");
		AssetTexture.TextureInfo info = new AssetTexture.TextureInfo
		{
			Width = 260,
			Height = 490,
			Tex = white,
			UvRect = new Rect(0f, 0f, 1f, 1f)
		};
		bool lifecycle = Environment.GetEnvironmentVariable("SPRK_LIGHT_PORTRAIT_LIFECYCLE") == "1";
		if (lifecycle)
		{
			hero.HeroInfo.CostumeIndex = 180;
			loader.SetAssetPath("Portrait/Monster/Ordeal/LightPancironne", UIHelper.EAssetTextureAssign.ClipBottom);
			CompleteCachedPortrait(loader, info);
			for (int cycle = 0; cycle < 6; cycle++)
			{
				if (cycle > 0)
				{
					heroGo.SetActive(value: false);
					if (ui.width != 288 || ui.height != 660)
					{
						throw new Exception("Pool disable failed to restore original card");
					}
					hero.HeroInfo.CostumeIndex = 180;
					heroGo.SetActive(value: true);
					loader.SetAssetPath("Portrait/Monster/Ordeal/LightPancironne", UIHelper.EAssetTextureAssign.ClipBottom);
					CompleteCachedPortrait(loader, info);
					yield return null;
				}
				hero.HeroInfo.CostumeIndex = 7280;
				loader.SetAssetPath("Portrait/Monster/Ordeal/LightPancironne", UIHelper.EAssetTextureAssign.ClipBottom);
				yield return new WaitForEndOfFrame();
				int expectedHeight = Mathf.RoundToInt((float)info.Height * Mathf.Min(288f / (float)info.Width, 660f / (float)info.Height));
				Transform transform = imageGo.transform.Find("SPRK Light portrait edge fade");
				if (ui.width != 288 || ui.height != 660 || Mathf.Abs(ui.drawingDimensions.w - ui.drawingDimensions.y - (float)expectedHeight) > 0.01f || transform == null || !transform.gameObject.activeSelf || transform.gameObject.layer != 30)
				{
					throw new Exception("Cached portrait did not recover after party reuse, cycle " + cycle + " dimensions=" + ui.width + "x" + ui.height + " expected=" + expectedHeight + " fade=" + transform?.ToString() + " texture=" + ui.mainTexture?.ToString() + " active=" + loader.isActiveAndEnabled + " asset=" + typeof(UIBundleLoader).GetField("_assetTex", flags).GetValue(loader)?.ToString() + " path=" + loader.AssetPath + " mode=" + hero.PortraitMode.ToString() + " costume=" + ((hero.HeroInfo == null) ? (-1) : hero.HeroInfo.CostumeIndex));
				}
				ui.drawRegion = new Vector4(0f, 0f, 1f, 1f);
				ui.uvRect = new Rect(0f, 0f, 1f, 0.5f);
				ui.UpdateAnchors();
				yield return new WaitForEndOfFrame();
				if (Mathf.Abs(ui.drawingDimensions.w - ui.drawingDimensions.y - (float)expectedHeight) > 0.01f || ui.uvRect != info.UvRect)
				{
					throw new Exception("Late party layout reset was not repaired");
				}
				if (ui.leftAnchor.absolute != 20 || ui.rightAnchor.absolute != -20 || ui.bottomAnchor.absolute != 30 || ui.topAnchor.absolute != -30)
				{
					throw new Exception("Fallback fit mutated the native card anchor offsets");
				}
				hero.HeroInfo.CostumeIndex = 0;
				loader.SetAssetPath("Portrait/Hero/Frey_5", UIHelper.EAssetTextureAssign.ClipBottom);
				CompleteCachedPortrait(loader, new AssetTexture.TextureInfo
				{
					Width = 500,
					Height = 1200,
					Tex = white,
					UvRect = new Rect(0f, 0f, 1f, 1f)
				});
				ui.UpdateAnchors();
				yield return new WaitForEndOfFrame();
				if (ui.width != 288 || ui.height != 660 || ui.transform.localPosition.sqrMagnitude > 0.01f || ui.drawRegion != new Vector4(0f, 0f, 1f, 1f))
				{
					throw new Exception("Normal portrait overflowed after reusing a fallback card");
				}
				hero.HeroInfo.CostumeIndex = 7280;
				loader.SetAssetPath("Portrait/Monster/Ordeal/LightPancironne", UIHelper.EAssetTextureAssign.ClipBottom);
				CompleteCachedPortrait(loader, info);
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			SprkImageRecovery.FitHeroPortrait(loader, info);
		}
		Transform fade = imageGo.transform.Find("SPRK Light portrait edge fade");
		if (fade == null)
		{
			throw new Exception("Fade widget not created");
		}
		if (fade.gameObject.layer != 30)
		{
			throw new Exception("Fade does not follow portrait UI layer");
		}
		yield return null;
		yield return null;
		yield return new WaitForEndOfFrame();
		camera.Render();
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = target;
		picture = new Texture2D(288, 660, TextureFormat.RGBA32, mipChain: false);
		picture.ReadPixels(new Rect(0f, 0f, 288f, 660f), 0, 0);
		picture.Apply();
		RenderTexture.active = active;
		File.WriteAllBytes(report + ".png", picture.EncodeToPNG());
		File.WriteAllText(report + ".diagnostics", "panel=" + panel?.ToString() + " widgets=" + panel.widgets.Count + " drawcalls=" + panel.DrawCallCount + " uiVisible=" + ui.isVisible + " uiShader=" + ui.shader?.ToString() + " uiDrawCall=" + ui.drawCall?.ToString() + " fadeVisible=" + fade.GetComponent<UITexture>().isVisible + " fadeShader=" + fade.GetComponent<UITexture>().shader?.ToString() + " fadeDrawCall=" + fade.GetComponent<UITexture>().drawCall?.ToString() + " camera=" + NGUITools.FindCameraForLayer(30)?.ToString() + " uiLayer=" + ui.drawCall.gameObject.layer + " fadeLayer=" + fade.GetComponent<UITexture>().drawCall.gameObject.layer + " uiPos=" + ui.drawCall.transform.position.ToString() + " fadePos=" + fade.GetComponent<UITexture>().drawCall.transform.position.ToString() + " uiRenderer=" + ui.drawCall.GetComponent<Renderer>()?.ToString() + " uiBounds=" + ui.drawCall.GetComponent<Renderer>().bounds.ToString() + " cameraPos=" + camera.transform.position.ToString() + " cameraSize=" + camera.orthographicSize);
		Color pixel = picture.GetPixel(144, 650);
		Color pixel2 = picture.GetPixel(144, 330);
		Color pixel3 = picture.GetPixel(144, 10);
		if (pixel.maxColorComponent > 0.15f || pixel3.maxColorComponent > 0.15f || pixel2.maxColorComponent < 0.6f || (string.IsNullOrEmpty(sample) && (pixel2.r < 0.8f || pixel2.g < 0.8f || pixel2.b < 0.8f)))
		{
			string[] obj = new string[6] { "Fade did not render black at both card edges with an unobscured portrait center: top=", null, null, null, null, null };
			Color color = pixel;
			obj[1] = color.ToString();
			obj[2] = ", middle=";
			color = pixel2;
			obj[3] = color.ToString();
			obj[4] = ", bottom=";
			color = pixel3;
			obj[5] = color.ToString();
			throw new Exception(string.Concat(obj));
		}
		File.WriteAllText(report, "PASS: actual NGUI camera render shows black at both edges and the image in the center" + (lifecycle ? "; six anchored native cached-callback cycles, normal/fallback costume switches, intact anchor offsets and late layout resets" : ""));
	}
}
