using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NGame2.NLogin;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset.NSound;
using NShared;
using NVespa;
using NVespa.NSingleton;
using Spine.Unity;
using UnityEngine;

public sealed class SprkLoginPresentationProbe : MonoBehaviour
{
	private void FreezeLogin()
	{
		LoginManager instance = SingletonUpdater<LoginManager>.instance;
		string[] array = new string[3] { "_state", "_nextState", "_subState" };
		foreach (string text in array)
		{
			typeof(LoginManager).GetField(text, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, null);
		}
	}

	private void Update()
	{
		FreezeLogin();
		UILabel[] array = Resources.FindObjectsOfTypeAll<UILabel>();
		foreach (UILabel uILabel in array)
		{
			if (uILabel != null && uILabel.gameObject.activeInHierarchy && (uILabel.text ?? "").IndexOf("Cannot read Host", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				BaseWindow componentInParent = uILabel.GetComponentInParent<BaseWindow>();
				if (componentInParent != null)
				{
					componentInParent.gameObject.SetActive(value: false);
				}
			}
		}
	}

	private static void Check(bool value, string message)
	{
		if (!value)
		{
			throw new Exception("Login presentation: " + message);
		}
	}

	private static void Screenshot(string path)
	{
		Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture2D.Apply();
		File.WriteAllBytes(path, texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
	}

	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(4f);
		FreezeLogin();
		string output = Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_TEST");
		Directory.CreateDirectory(output);
		List<string> report = new List<string>();
		LoginManagement manager = SceneAutoBehaviour<LoginManagement>.instance;
		LoginBackground background = manager.GetInstance<LoginBackground>();
		if (background == null)
		{
			manager.AddInstance<LoginBackground>("Resources/Prefab2/Window/Login/LoginBackground");
			background = manager.GetInstance<LoginBackground>();
		}
		background.Open(null);
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera.name == "BI UI Camera")
			{
				camera.gameObject.SetActive(value: false);
			}
		}
		Transform[] componentsInChildren;
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_MENU")))
		{
			SystemOptionData.ShowTitleGameIntroduce = true;
			background.HideMessage();
			LoginTouchWait touch = manager.GetInstance<LoginTouchWait>();
			touch.Open(null);
			LoginAccountInfo menu = manager.GetInstance<LoginAccountInfo>();
			menu.Open(null, null, null);
			yield return new WaitForSecondsRealtime(1f);
			menu.Refresh_Noraml();
			menu.Refresh_Noraml();
			yield return new WaitForSecondsRealtime(1f);
			List<string> list = new List<string>();
			Component[] array = new Component[2] { menu, touch };
			foreach (Component component in array)
			{
				componentsInChildren = component.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform in componentsInChildren)
				{
					string text = transform.name;
					Transform parent = transform.parent;
					while (parent != null && parent != component.transform.parent)
					{
						text = parent.name + "/" + text;
						parent = parent.parent;
					}
					UIWidget component2 = transform.GetComponent<UIWidget>();
					UISprite uISprite = component2 as UISprite;
					UILabel uILabel = component2 as UILabel;
					list.Add(text + " active=" + transform.gameObject.activeSelf + "/" + transform.gameObject.activeInHierarchy + " local=" + transform.localPosition.ToString() + " world=" + transform.position.ToString() + ((component2 == null) ? "" : (" " + component2.GetType().Name + " " + component2.width + "x" + component2.height + " pivot=" + component2.pivot.ToString() + " anchored=" + component2.isAnchored)) + ((uISprite == null) ? "" : (" sprite=" + uISprite.spriteName)) + ((uILabel == null) ? "" : (" text=\"" + uILabel.text + "\"")) + ((transform.GetComponent<UIButton>() != null) ? " BUTTON" : "") + ((transform.GetComponent<UIGrid>() != null) ? (" GRID cell=" + transform.GetComponent<UIGrid>().cellWidth + "x" + transform.GetComponent<UIGrid>().cellHeight + " arr=" + transform.GetComponent<UIGrid>().arrangement) : "") + ((transform.GetComponent<UITable>() != null) ? " TABLE" : "") + " scale=" + transform.localScale.ToString() + " comps=" + string.Join(",", (from c in transform.GetComponents<Component>()
						select c.GetType().Name).ToArray()));
				}
			}
			File.WriteAllLines(Path.Combine(output, "menu-hierarchy.txt"), list.ToArray());
			File.WriteAllLines(Path.Combine(output, "menu-labels.txt"), (from l in Resources.FindObjectsOfTypeAll<UILabel>()
				where l != null && l.gameObject.activeInHierarchy && l.gameObject.scene.IsValid()
				select l).Select(delegate(UILabel l)
			{
				string text2 = l.name;
				Transform parent3 = l.transform.parent;
				while (parent3 != null)
				{
					text2 = parent3.name + "/" + text2;
					parent3 = parent3.parent;
				}
				return text2 + " text=\"" + l.text + "\" world=" + l.transform.position.ToString() + " alpha=" + l.finalAlpha;
			}).ToArray());
			yield return new WaitForEndOfFrame();
			Screenshot(Path.Combine(output, "menu.png"));
			Transform settingsRoot = menu.LanguageButtonRoot.transform;
			Transform intro = menu.GameIntroduceButtonRoot.transform;
			Check(!menu.LoginButtonRoot.activeInHierarchy, "Log In button visible");
			Check(menu.Label_ServerName == null || !menu.Label_ServerName.gameObject.activeInHierarchy, "Server row visible");
			Check(settingsRoot.gameObject.activeInHierarchy && intro.gameObject.activeInHierarchy, "Settings or Introduction hidden");
			Check(Vector3.Distance(settingsRoot.position, menu.LoginButtonRoot.transform.position) < 0.001f, "Settings is not in the Log In slot");
			Transform menuRoot = menu.transform.Find("Panel/Root");
			Func<bool> mirrored = delegate
			{
				Vector3 vector4 = menuRoot.InverseTransformPoint(intro.position);
				Vector3 vector5 = menuRoot.InverseTransformPoint(settingsRoot.position);
				return vector5.x > 0f && Mathf.Abs(vector4.x + vector5.x) < 0.5f && Mathf.Abs(vector4.y - vector5.y) < 0.5f;
			};
			Check(mirrored(), "Introduction is not bottom-left on the Settings row");
			Check(intro.GetComponentsInChildren<UILabel>(includeInactive: true).All((UILabel l) => l.text == "Introduction"), "Introduction caption");
			Transform transform2 = touch.transform.Find("Panel/Root");
			UIWidget component3 = transform2.Find("Sprite_backGround").GetComponent<UIWidget>();
			UILabel componentInChildren = settingsRoot.GetComponentInChildren<UILabel>(includeInactive: true);
			Check(componentInChildren.pivot == UIWidget.Pivot.Bottom, "Settings caption pivot changed");
			float num = SprkTapToPlay.CaptionCenterY(componentInChildren, transform2);
			Check(touch.GetComponents<SprkTapToPlay>().Length == 1 && Mathf.Abs(component3.transform.localScale.x - 0.8f) < 0.001f && Mathf.Abs(component3.transform.localPosition.x) < 0.01f, "TAP TO PLAY size");
			Check(Mathf.Abs(component3.transform.localPosition.y - num) < 0.5f && Mathf.Abs(num - -464f) < 3f, "TAP TO PLAY is not on the Settings row: " + component3.transform.localPosition.y + " vs " + num + " fallback " + -464f);
			Check(intro.GetComponentInChildren<UIButton>(includeInactive: true).onClick.Count > 0, "Introduction lost its action");
			for (int num2 = 0; num2 < 8; num2++)
			{
				menu.Refresh_Noraml();
			}
			yield return null;
			Check(mirrored() && Vector3.Distance(settingsRoot.position, menu.LoginButtonRoot.transform.position) < 0.001f && !menu.LoginButtonRoot.activeInHierarchy, "Layout changed after refresh");
			Check(settingsRoot.GetComponentInChildren<UIButton>(includeInactive: true).onClick.Count == 1, "Duplicate Settings callbacks");
			Vector3 introPos = intro.position;
			menu.EnableGameIntroduce(enable: true);
			menu.Refresh_PatchDownloading();
			yield return null;
			Check(intro.gameObject.activeInHierarchy && Vector3.Distance(intro.position, introPos) < 0.001f, "Introduction should stay bottom-left while patching");
			yield return new WaitForEndOfFrame();
			Screenshot(Path.Combine(output, "menu-patching.png"));
			menu.Refresh_Noraml();
			yield return null;
			Check(mirrored(), "Layout not restored after patching");
			File.WriteAllText(Path.Combine(output, "report.txt"), "menuLayout=True loginHidden=True serverHidden=True introductionBottomLeft=True tapToPlayOnRow=True");
			Application.Quit();
			yield break;
		}
		SprkLoginPresentation bridge = background.GetComponent<SprkLoginPresentation>();
		SprkLoginPresentation.Catalog options = SprkLoginPresentation.Options;
		SprkLoginPresentation.Background[] backgrounds;
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_COVER")))
		{
			List<string> lines = new List<string> { "screen=" + Screen.width + "x" + Screen.height };
			background.SetMessage("[State]StateBase_SelectLoginMethod", animate: false);
			backgrounds = options.backgrounds;
			foreach (SprkLoginPresentation.Background option in backgrounds)
			{
				SprkLoginPresentation.SelectBackground(option.id);
				yield return new WaitForSecondsRealtime(3.5f);
				yield return new WaitForEndOfFrame();
				Screenshot(Path.Combine(output, "cover-" + option.id + ".png"));
				Rect coverView = bridge.CoverView;
				Rect coverArt = bridge.CoverArt;
				float coverScale = bridge.CoverScale;
				UISprite nativeLogo = bridge.NativeLogo;
				Vector3[] worldCorners = nativeLogo.worldCorners;
				float x = NGUITools.FindCameraForLayer(nativeLogo.gameObject.layer).WorldToViewportPoint((worldCorners[0] + worldCorners[2]) * 0.5f).x;
				string[] obj = new string[13]
				{
					option.id,
					" scale=",
					coverScale.ToString("0.0000"),
					" art=",
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				Rect rect = coverArt;
				obj[4] = rect.ToString();
				obj[5] = " view=";
				rect = coverView;
				obj[6] = rect.ToString();
				obj[7] = " logoViewportX=";
				obj[8] = x.ToString("0.0000");
				obj[9] = " messageOffset=";
				obj[10] = bridge.MessageLineOffset.ToString("0.00");
				obj[11] = " pixelCorrections=";
				obj[12] = bridge.PixelCorrections.ToString();
				lines.Add(string.Concat(obj));
				Check(coverScale >= 1f && coverScale <= 2f, "Cover scale out of range: " + option.id);
				Check(bridge.MessageLineOffset < 0.5f, "Status message is not on the TAP TO PLAY line: " + option.id);
			}
			File.WriteAllLines(Path.Combine(output, "cover.txt"), lines.ToArray());
			Application.Quit();
			yield break;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_SINGLE");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			SprkLoginPresentation.SelectBackground(environmentVariable);
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_TIMELINE")))
			{
				float start = Time.realtimeSinceStartup;
				float[] array2 = new float[10] { 0.2f, 1f, 2f, 3f, 4f, 4.5f, 5f, 5.5f, 6f, 7f };
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					float at = array2[num3];
					while (Time.realtimeSinceStartup - start < at)
					{
						yield return null;
					}
					yield return new WaitForEndOfFrame();
					Screenshot(Path.Combine(output, "timeline-" + at.ToString("0.0", CultureInfo.InvariantCulture) + ".png"));
				}
			}
			yield return new WaitForSecondsRealtime(8f);
			yield return new WaitForEndOfFrame();
			Screenshot(Path.Combine(output, "single.png"));
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_CORNER")))
			{
				List<string> list2 = new List<string>();
				allCameras = Camera.allCameras;
				foreach (Camera camera2 in allCameras)
				{
					list2.Add("CAMERA " + camera2.name + " depth=" + camera2.depth + " mask=" + camera2.cullingMask + " ortho=" + camera2.orthographic);
				}
				Func<Transform, string> func = delegate(Transform t)
				{
					string text2 = t.name;
					Transform parent3 = t.parent;
					while (parent3 != null)
					{
						text2 = parent3.name + "/" + text2;
						parent3 = parent3.parent;
					}
					return text2;
				};
				foreach (Renderer r in from renderer in Resources.FindObjectsOfTypeAll<Renderer>()
					where renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy && renderer.gameObject.scene.IsValid()
					select renderer)
				{
					Camera camera3 = (from c in Camera.allCameras
						where (c.cullingMask & (1 << r.gameObject.layer)) != 0
						orderby c.depth descending
						select c).FirstOrDefault();
					if (camera3 == null)
					{
						continue;
					}
					Vector3 vector = camera3.WorldToViewportPoint(r.bounds.center);
					if (!(vector.x < 0.6f) && !(vector.y < 0.75f))
					{
						string[] obj2 = new string[8]
						{
							"RENDERER ",
							func(r.transform),
							" vp=",
							null,
							null,
							null,
							null,
							null
						};
						Vector3 vector2 = vector;
						obj2[3] = vector2.ToString();
						obj2[4] = " size=";
						obj2[5] = r.bounds.size.ToString();
						obj2[6] = " mats=";
						obj2[7] = string.Join(",", r.sharedMaterials.Select((Material m) => (!(m == null)) ? (m.name + "/" + m.shader.name + "/" + ((m.mainTexture == null) ? "-" : m.mainTexture.name)) : "null"));
						list2.Add(string.Concat(obj2));
					}
				}
				foreach (UIWidget item in from w in Resources.FindObjectsOfTypeAll<UIWidget>()
					where w != null && w.enabled && w.gameObject.activeInHierarchy && w.gameObject.scene.IsValid() && w.isVisible
					select w)
				{
					Camera camera4 = NGUITools.FindCameraForLayer(item.gameObject.layer);
					if (!(camera4 == null))
					{
						Vector3 vector3 = camera4.WorldToViewportPoint(item.transform.TransformPoint(item.localCenter));
						if (!(vector3.x < 0.6f) && !(vector3.y < 0.75f))
						{
							UISprite uISprite2 = item as UISprite;
							UITexture uITexture = item as UITexture;
							UILabel uILabel2 = item as UILabel;
							string[] obj3 = new string[17]
							{
								"WIDGET ",
								item.GetType().Name,
								" ",
								func(item.transform),
								" vp=",
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null,
								null
							};
							Vector3 vector2 = vector3;
							obj3[5] = vector2.ToString();
							obj3[6] = " ";
							obj3[7] = item.width.ToString();
							obj3[8] = "x";
							obj3[9] = item.height.ToString();
							obj3[10] = " color=";
							obj3[11] = item.color.ToString();
							obj3[12] = " alpha=";
							obj3[13] = item.finalAlpha.ToString();
							obj3[14] = ((uISprite2 != null) ? (" sprite=" + uISprite2.spriteName + "@" + ((uISprite2.atlas == null) ? "null" : uISprite2.atlas.name)) : "");
							obj3[15] = ((uITexture != null) ? (" tex=" + ((uITexture.mainTexture == null) ? "null" : uITexture.mainTexture.name)) : "");
							obj3[16] = ((uILabel2 != null) ? (" text=\"" + uILabel2.text + "\"") : "");
							list2.Add(string.Concat(obj3));
						}
					}
				}
				foreach (UISprite item2 in from s in Resources.FindObjectsOfTypeAll<UISprite>()
					where s != null && s.gameObject.activeInHierarchy && s.spriteName == "Logo_Rebellion_White"
					select s)
				{
					Texture mainTexture = item2.mainTexture;
					if (mainTexture == null)
					{
						continue;
					}
					int num4 = 398;
					int num5 = 178;
					int num6 = 500;
					int num7 = 222;
					Material material = item2.material;
					list2.Add("LOGO tex=" + mainTexture.name + " " + mainTexture.width + "x" + mainTexture.height + " mat=" + ((material == null) ? "null" : (material.name + "/" + material.shader.name + " alphaTex=" + ((material.HasProperty("_AlphaTex") && material.GetTexture("_AlphaTex") != null) ? material.GetTexture("_AlphaTex").name : "-") + " kw=" + string.Join(",", material.shaderKeywords))));
					RenderTexture temporary = RenderTexture.GetTemporary(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32);
					Graphics.Blit(mainTexture, temporary);
					RenderTexture active = RenderTexture.active;
					RenderTexture.active = temporary;
					Texture2D texture2D = new Texture2D(num6, num7, TextureFormat.RGBA32, mipChain: false);
					texture2D.ReadPixels(new Rect(num4, mainTexture.height - num5 - num7, num6, num7), 0, 0);
					texture2D.Apply();
					RenderTexture.active = active;
					RenderTexture.ReleaseTemporary(temporary);
					File.WriteAllBytes(Path.Combine(output, "logo-runtime.png"), texture2D.EncodeToPNG());
					UnityEngine.Object.Destroy(texture2D);
					Transform parent2 = item2.transform;
					while (parent2 != null)
					{
						Component[] array = parent2.GetComponents<Component>();
						foreach (Component component4 in array)
						{
							list2.Add("  COMP " + parent2.name + " " + component4.GetType().FullName);
						}
						parent2 = parent2.parent;
					}
				}
				list2.Add("LOGOSTATUS " + SprkLoginPresentation.LogoStatus);
				if (bridge.NativeLogo != null)
				{
					Vector3[] worldCorners2 = bridge.NativeLogo.worldCorners;
					string[] obj4 = new string[12]
					{
						"NATIVE pivot=",
						bridge.NativeLogo.pivot.ToString(),
						" local=",
						bridge.NativeLogo.transform.localPosition.ToString(),
						" scale=",
						bridge.NativeLogo.transform.localScale.ToString(),
						" corners=",
						null,
						null,
						null,
						null,
						null
					};
					Vector3 vector2 = worldCorners2[0];
					obj4[7] = vector2.ToString();
					obj4[8] = "..";
					vector2 = worldCorners2[2];
					obj4[9] = vector2.ToString();
					obj4[10] = " enabled=";
					obj4[11] = bridge.NativeLogo.enabled.ToString();
					list2.Add(string.Concat(obj4));
				}
				if (bridge.EraLogo != null)
				{
					Vector3[] worldCorners3 = bridge.EraLogo.worldCorners;
					string[] obj5 = new string[8]
					{
						"ERA ",
						bridge.EraLogo.width.ToString(),
						"x",
						bridge.EraLogo.height.ToString(),
						" corners=",
						null,
						null,
						null
					};
					Vector3 vector2 = worldCorners3[0];
					obj5[5] = vector2.ToString();
					obj5[6] = "..";
					vector2 = worldCorners3[2];
					obj5[7] = vector2.ToString();
					list2.Add(string.Concat(obj5));
				}
				File.WriteAllLines(Path.Combine(output, "corner.txt"), list2.ToArray());
			}
			if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_ISOLATE")))
			{
				Renderer[] renderers = (from renderer in bridge.RecoveredRoot.GetComponentsInChildren<Renderer>(includeInactive: true)
					where renderer.enabled && renderer.gameObject.activeInHierarchy && (!(renderer.sharedMaterial != null) || !renderer.sharedMaterial.shader.name.StartsWith("Spine"))
					select renderer).ToArray();
				for (int num3 = 0; num3 < renderers.Length; num3++)
				{
					renderers[num3].enabled = false;
					yield return null;
					yield return new WaitForEndOfFrame();
					Screenshot(Path.Combine(output, string.Concat("isolate-", num3.ToString(), "-", string.Concat(renderers[num3].name.Where((char c) => char.IsLetterOrDigit(c) || c == '_')), ".png")));
					renderers[num3].enabled = true;
				}
			}
			Shader shader = Shader.Find("Vespa/Effect2/NormalNoZ");
			File.WriteAllLines(Path.Combine(output, "shader.txt"), (from propertyIndex in Enumerable.Range(0, shader.GetPropertyCount())
				select shader.GetPropertyName(propertyIndex) + " " + shader.GetPropertyType(propertyIndex)).ToArray());
			File.WriteAllLines(Path.Combine(output, "single.txt"), (from renderer in bridge.RecoveredRoot.GetComponentsInChildren<Renderer>(includeInactive: true)
				select renderer.name + " active=" + renderer.gameObject.activeInHierarchy + " enabled=" + renderer.enabled + " bounds=" + renderer.bounds.ToString() + " scale=" + renderer.transform.lossyScale.ToString() + " materials=" + string.Join(",", renderer.sharedMaterials.Select((Material m) => (!(m == null)) ? (m.name + "/" + m.shader.name) : "null"))).ToArray());
			Application.Quit();
			yield break;
		}
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_RESTART_TEST")))
		{
			Check(SprkLoginPresentation.SelectedBackground == options.backgrounds.Last().id, "Background did not survive restart");
			Check(SprkLoginPresentation.SelectedMusic == options.music.Last().id, "Music did not survive restart");
			for (int num3 = 0; num3 < 100; num3++)
			{
				if (!(bridge.LoadedMusicId != SprkLoginPresentation.SelectedMusic))
				{
					break;
				}
				yield return new WaitForSecondsRealtime(0.1f);
			}
			Check(bridge.RecoveredRoot != null && bridge.LoadedMusicId == SprkLoginPresentation.SelectedMusic, "Restart did not restore scene/audio");
			File.WriteAllText(Path.Combine(output, "restart.txt"), "PASS: independent background and music preferences survive restart and restore actual assets.");
			Application.Quit();
			yield break;
		}
		Check(SprkLoginPresentation.SelectedBackground == "x-the-final-azeta-and-valance" && bridge.RecoveredRoot != null, "Fresh profile did not open Azeta and Valance by default");
		Check(SprkLoginPresentation.SelectedMusic == "e47c58d47a86adfa0c40" && SprkLoginPresentation.MusicCaption == "Promise", "Fresh profile did not select Promise");
		for (int num3 = 0; num3 < 100; num3++)
		{
			if (!(bridge.LoadedMusicId != "e47c58d47a86adfa0c40"))
			{
				break;
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
		Check(bridge.LoadedMusicId == "e47c58d47a86adfa0c40", "Fresh profile did not play Promise");
		report.Add("defaultMusicPromise=True");
		report.Add("defaultBackgroundAzetaAndValance=True");
		Check(SprkLoginPresentation.LogoSubtitleRestored, "Corner logo subtitle was not restored");
		report.Add("cornerLogoSubtitleRestored=True");
		Check(background.Label_Version == null || !background.Label_Version.gameObject.activeInHierarchy, "Login version label is still shown");
		report.Add("versionLabelHidden=True");
		string[][] array3 = new string[20][]
		{
			new string[2] { "native", "x-the-final-azeta-and-valance" },
			new string[2] { "13-two-characters", "x-the-final-azeta-and-valance" },
			new string[2] { "13-two-characters-night", "x-the-final-azeta-and-valance" },
			new string[2] { "01-original-party", "original-heroes" },
			new string[2] { "02-cathedral-party", "heroes-of-orvel" },
			new string[2] { "03-war-table", "war-council" },
			new string[2] { "04-fire-dragon", "dragons-challenge" },
			new string[2] { "05-hero-ensemble", "gathered-heroes" },
			new string[2] { "06-red-sky", "pandemonium" },
			new string[2] { "07-red-sky-layers", "pandemonium" },
			new string[2] { "08-light-and-dark", "soul-weapon-light" },
			new string[2] { "08-light-and-dark-night", "soul-weapon-darkness" },
			new string[2] { "09-fallen-fate-spine", "ix-fallen-fate" },
			new string[2] { "09-fallen-fate-spine-night", "ix-fallen-fate" },
			new string[2] { "10-ruins-four-heroes", "x-rebellion" },
			new string[2] { "10-ruins-four-heroes-night", "x-rebellion" },
			new string[2] { "11-day-night", "x-rebellion" },
			new string[2] { "11-day-night-night", "x-rebellion" },
			new string[2] { "12-kasel-frey", "x-the-final-kasel-and-frey" },
			new string[2] { "12-kasel-frey-night", "x-the-final-kasel-and-frey" }
		};
		foreach (string[] array4 in array3)
		{
			string selectedMusic = SprkLoginPresentation.SelectedMusic;
			SprkTestPrefs.SetString("Sprk.Login.Background", array4[0]);
			Check(SprkLoginPresentation.SelectedBackground == array4[1] && SprkTestPrefs.GetString("Sprk.Login.Background", "") == array4[1], "Removed background preference did not migrate: " + array4[0]);
			Check(SprkLoginPresentation.SelectedMusic == selectedMusic, "Background migration changed music");
		}
		report.Add("removedBackgroundPreferencesMigrate=True");
		SprkTestPrefs.SetString("Sprk.Login.Music", "native");
		Check(SprkLoginPresentation.SelectedMusic == "e47c58d47a86adfa0c40", "Legacy Default Login Music did not migrate to Promise");
		report.Add("legacyDefaultMusicMigrates=True");
		SprkLoginPresentation.SelectBackground("native");
		Check(SprkLoginPresentation.SelectedBackground == "x-the-final-azeta-and-valance" && bridge.RecoveredRoot != null, "Legacy Current selection did not migrate to Azeta and Valance");
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "default-background.png"));
		bool uiOnly = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_LOGIN_PRESENTATION_UI_ONLY"));
		backgrounds = (uiOnly ? new SprkLoginPresentation.Background[0] : options.backgrounds);
		foreach (SprkLoginPresentation.Background option in backgrounds)
		{
			string selectedMusic2 = SprkLoginPresentation.SelectedMusic;
			SprkLoginPresentation.SelectBackground(option.id);
			Check(SprkLoginPresentation.SelectedMusic == selectedMusic2, "Background changed music preference");
			Check(bridge.RecoveredRoot != null, "Scene missing: " + option.id);
			if (string.IsNullOrEmpty(option.logo))
			{
				Check(bridge.EraLogo == null && bridge.NativeLogoVisible, "Native corner logo missing: " + option.id);
			}
			else
			{
				Check(bridge.EraLogo != null && bridge.EraLogo.mainTexture != null && bridge.NativeLogo != null && !bridge.NativeLogoVisible, "Era corner logo missing: " + option.id);
				Vector3[] worldCorners4 = bridge.NativeLogo.worldCorners;
				Vector3[] worldCorners5 = bridge.EraLogo.worldCorners;
				Check(Vector3.Distance((worldCorners4[0] + worldCorners4[2]) * 0.5f, (worldCorners5[0] + worldCorners5[2]) * 0.5f) < 0.01f, "Era corner logo is not centred on the native logo: " + option.id);
			}
			Check(background.BackEffectPanelRoot == null || !background.BackEffectPanelRoot.gameObject.activeInHierarchy, "Native backdrop covers recovered scene");
			yield return new WaitForSecondsRealtime((option.id.StartsWith("06-") || option.id.StartsWith("07-") || option.id.StartsWith("08-")) ? 8f : 1.2f);
			SkeletonAnimation[] spines = bridge.RecoveredRoot.GetComponentsInChildren<SkeletonAnimation>(includeInactive: true);
			SkeletonAnimation[] array5 = spines;
			foreach (SkeletonAnimation skeletonAnimation in array5)
			{
				if (skeletonAnimation.skeletonDataAsset == null)
				{
					Check(!skeletonAnimation.gameObject.activeInHierarchy || !skeletonAnimation.enabled, "Active skeleton without data: " + skeletonAnimation.name);
					continue;
				}
				Check(skeletonAnimation.skeletonDataAsset.GetSkeletonData(quiet: false) != null, "Invalid skeleton: " + skeletonAnimation.name);
				if (skeletonAnimation.gameObject.activeInHierarchy && skeletonAnimation.enabled && !string.IsNullOrEmpty(skeletonAnimation.AnimationName))
				{
					Check(skeletonAnimation.AnimationState.GetCurrent(0) != null && skeletonAnimation.AnimationState.GetCurrent(0).TrackTime > 0f, "Animation is not advancing: " + skeletonAnimation.name);
				}
			}
			yield return new WaitForEndOfFrame();
			Screenshot(Path.Combine(output, option.id + ".png"));
			report.Add("background=" + option.id + " logo=" + ((option.logo == null) ? "native" : "era") + " spines=" + spines.Length + " components=" + bridge.RecoveredRoot.GetComponentsInChildren<Component>(includeInactive: true).Length);
			File.WriteAllLines(Path.Combine(output, "progress.txt"), report.ToArray());
		}
		SprkLoginPresentation.SelectBackground("native");
		Check(bridge.RecoveredRoot != null && SprkLoginPresentation.SelectedBackground == "x-the-final-azeta-and-valance", "Legacy Current did not return to the default background");
		SprkLoginPresentation.SelectBackground(options.backgrounds.Last().id);
		SprkLoginPresentation.Music[] array6 = (uiOnly ? new SprkLoginPresentation.Music[0] : options.music);
		foreach (SprkLoginPresentation.Music option2 in array6)
		{
			string previousBackground = SprkLoginPresentation.SelectedBackground;
			SprkLoginPresentation.SelectMusic(option2.id);
			for (int n = 0; n < 100; n++)
			{
				if (!(bridge.LoadedMusicId != option2.id))
				{
					break;
				}
				yield return new WaitForSecondsRealtime(0.1f);
			}
			Check(bridge.LoadedMusicId == option2.id && bridge.LoadedMusic != null && bridge.LoadedMusic.length > 30f, "Music load failed: " + option2.label);
			Check(SprkLoginPresentation.SelectedBackground == previousBackground, "Music changed background preference");
			Check(SprkLoginPresentation.FilterMusic(bridge.LoadedMusic) == bridge.LoadedMusic, "Native music filter lost selection");
			report.Add("music=" + option2.id + " seconds=" + bridge.LoadedMusic.length);
		}
		SprkTestPrefs.SetString("Sprk.Login.Music", "silent");
		Check(SprkLoginPresentation.SelectedMusic == "e47c58d47a86adfa0c40", "Removed No Music choice did not migrate to Promise");
		SprkLoginPresentation.SelectMusic(options.music.First().id);
		SprkLoginPresentation.SelectMusic(options.music.Last().id);
		for (int num3 = 0; num3 < 100; num3++)
		{
			if (!(bridge.LoadedMusicId != options.music.Last().id))
			{
				break;
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
		yield return new WaitForSecondsRealtime(0.5f);
		Check(bridge.LoadedMusicId == options.music.Last().id, "Stale audio request replaced latest choice");
		bool originalBgm = SystemOptionData.Sound_EnableBgm;
		bool originalVolatile = VolatileOptionData.Sound_EnableBgm;
		SystemOptionData.Sound_EnableBgm = true;
		VolatileOptionData.Sound_EnableBgm = true;
		SprkLoginPresentation.RefreshMusic();
		BgmPlayer player = SingletonAuto<BgmPlayer>.instance;
		FieldInfo current3 = typeof(BgmPlayer).GetField("_currentBgm", BindingFlags.Instance | BindingFlags.NonPublic);
		yield return new WaitForSecondsRealtime(2f);
		Check(current3.GetValue(player) != null, "Native BGM player did not accept chosen music");
		Check(Resources.FindObjectsOfTypeAll<AudioSource>().Any((AudioSource s) => s.clip == bridge.LoadedMusic && s.isPlaying), "Chosen music is not playing through a native AudioSource");
		SystemOptionData.Sound_EnableBgm = false;
		yield return new WaitForSecondsRealtime(0.3f);
		Check(current3.GetValue(player) == null, "Chosen music ignored native BGM mute");
		SystemOptionData.Sound_EnableBgm = originalBgm;
		VolatileOptionData.Sound_EnableBgm = originalVolatile;
		report.Add("nativeBgmAndMute=True");
		report.Add("latestMusicRequestWins=True");
		SprkTestPrefs.SetString("Sprk.Login.Background", "invalid-background");
		SprkTestPrefs.SetString("Sprk.Login.Music", "invalid-music");
		Check(SprkLoginPresentation.SelectedBackground == "x-the-final-azeta-and-valance" && SprkLoginPresentation.SelectedMusic == "e47c58d47a86adfa0c40", "Invalid preference fallback");
		SprkLoginPresentation.SelectBackground(options.backgrounds.Last().id);
		SprkLoginPresentation.SelectMusic(options.music.Last().id);
		LoginAccountInfo login = manager.GetInstance<LoginAccountInfo>();
		if (login == null)
		{
			manager.AddInstance<LoginAccountInfo>("Resources/Prefab2/Window/Login/LoginAccountInfo");
			login = manager.GetInstance<LoginAccountInfo>();
		}
		SprkLoginSettings.Configure(login);
		login.GetComponent<SprkLoginSettings>().OpenSettings();
		GameOptionWindow settings = manager.GetInstance<GameOptionWindow>();
		yield return null;
		settings.MainTabs.Single((GameOptionWindow.TabUIInfo t) => t.GameOptionTab == GameOptionTab.Etc).Toggle.Set(state: true);
		settings.OnToggleMainTab();
		yield return new WaitForSecondsRealtime(0.5f);
		for (int num8 = 0; num8 < 8; num8++)
		{
			SprkLoginPresentation.Setup(settings);
		}
		Transform others = settings.transform.Find("Panel/Root/EtcOption");
		UIButton[] componentsInChildren2 = others.GetComponentsInChildren<UIButton>(includeInactive: true);
		Check(componentsInChildren2.Count((UIButton b) => b.name == "Button_LoginBackground") == 1 && componentsInChildren2.Count((UIButton b) => b.name == "Button_LoginMusic") == 1, "Duplicate Others controls");
		UIButton bgButton = componentsInChildren2.Single((UIButton b) => b.name == "Button_LoginBackground");
		UIButton musicButton = componentsInChildren2.Single((UIButton b) => b.name == "Button_LoginMusic");
		File.WriteAllLines(Path.Combine(output, "layout.txt"), new Transform[5]
		{
			settings.transform,
			settings.transform.Find("Panel"),
			settings.transform.Find("Panel/Root"),
			others,
			bgButton.transform
		}.Select((Transform t) => t.name + " local=" + t.localPosition.ToString() + " world=" + t.position.ToString() + " scale=" + t.lossyScale.ToString() + " active=" + t.gameObject.activeInHierarchy).ToArray());
		File.WriteAllLines(Path.Combine(output, "hierarchy.txt"), (from t in settings.GetComponentsInChildren<Transform>(includeInactive: true)
			where !new string[5] { "GameOption", "BattleOption", "SoundOption", "NoticeOption", "LinkedGroup" }.Any(delegate(string text2)
			{
				Transform transform3 = settings.transform.Find("Panel/Root/" + text2) ?? settings.transform.Find("Panel/Root/EtcOption/" + text2);
				return transform3 != null && t.IsChildOf(transform3);
			})
			select t).Select(delegate(Transform t)
		{
			string text2 = t.name;
			Transform parent3 = t.parent;
			while (parent3 != null && parent3 != settings.transform)
			{
				text2 = parent3.name + "/" + text2;
				parent3 = parent3.parent;
			}
			UIWidget component5 = t.GetComponent<UIWidget>();
			UISprite uISprite3 = component5 as UISprite;
			UILabel uILabel3 = component5 as UILabel;
			return text2 + " active=" + t.gameObject.activeSelf + " local=" + t.localPosition.ToString() + ((component5 == null) ? "" : (" " + component5.GetType().Name + " " + component5.width + "x" + component5.height + " depth=" + component5.depth + " color=" + component5.color.ToString() + " pivot=" + component5.pivot)) + ((uISprite3 == null) ? "" : (" sprite=" + uISprite3.spriteName + " atlas=" + ((uISprite3.atlas == null) ? "null" : uISprite3.atlas.name) + " type=" + uISprite3.type)) + ((uILabel3 == null) ? "" : (" text=\"" + uILabel3.text + "\" font=" + ((uILabel3.bitmapFont != null) ? uILabel3.bitmapFont.name : ((uILabel3.trueTypeFont != null) ? uILabel3.trueTypeFont.name : "null")) + " size=" + uILabel3.fontSize + " effect=" + uILabel3.effectStyle.ToString() + " effectColor=" + uILabel3.effectColor.ToString() + " grad=" + uILabel3.applyGradient)) + ((t.GetComponent<UIButton>() != null) ? " BUTTON" : "") + ((t.GetComponent<UIPanel>() != null) ? (" PANEL depth=" + t.GetComponent<UIPanel>().depth) : "");
		}).ToArray());
		Check(bgButton.gameObject.activeInHierarchy, "Others button is not visible");
		Check(bgButton.onClick.Count == 1 && musicButton.onClick.Count == 1, "Duplicate chooser callbacks");
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "settings-others.png"));
		Transform[] lobbyButtons = (from n2 in new string[5] { "Button_UseCoupon", "Button_ChangePassword", "Button_DeleteAccount", "Button_SwitchAccount", "Button_Logout" }
			select others.Find(n2) into t
			where t != null && !t.gameObject.activeSelf
			select t).ToArray();
		componentsInChildren = lobbyButtons;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: true);
		}
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "settings-others-lobby-layout.png"));
		componentsInChildren = lobbyButtons;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		EventDelegate.Execute(bgButton.onClick);
		yield return null;
		Transform picker = settings.transform.Find("Panel/Root/SPRK Login Background Picker");
		Check(picker != null, "Background picker missing");
		Check(Mathf.Abs(picker.position.x - settings.transform.Find("Panel/Root").position.x) < 0.01f, "Background picker is off-center");
		Check(picker.GetComponentsInChildren<UIButton>(includeInactive: true).Count((UIButton b) => b.name.StartsWith("Choice_")) == options.backgrounds.Length, "Background list incomplete or still includes Current");
		Check(!picker.GetComponentsInChildren<UILabel>(includeInactive: true).Any((UILabel label) => label.text.Contains("Current Background")), "Current Background remains in chooser");
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "background-picker.png"));
		UIScrollView scroll = picker.GetComponentInChildren<UIScrollView>();
		Vector3 before = scroll.transform.localPosition;
		scroll.SetDragAmount(0f, 1f, updateScrollbars: false);
		yield return null;
		Check(scroll.transform.localPosition != before, "Background choices do not scroll");
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "background-picker-bottom.png"));
		EventDelegate.Execute(picker.GetComponentsInChildren<UIButton>(includeInactive: true).Single((UIButton b) => b.name == "Close").onClick);
		EventDelegate.Execute(musicButton.onClick);
		yield return null;
		picker = settings.transform.Find("Panel/Root/SPRK Login Music Picker");
		Check(picker != null, "Music picker missing");
		Check(picker.GetComponentsInChildren<UIButton>(includeInactive: true).Count((UIButton b) => b.name.StartsWith("Choice_")) == options.music.Length, "Music list incomplete");
		Check(!picker.GetComponentsInChildren<UILabel>(includeInactive: true).Any((UILabel label) => label.text == "No Music"), "No Music remains in chooser");
		Check(!picker.GetComponentsInChildren<UILabel>(includeInactive: true).Any((UILabel label) => label.text.Contains("Default Login Music")), "Default Login Music remains in chooser");
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "music-picker.png"));
		EventDelegate.Execute(picker.GetComponentsInChildren<UIButton>(includeInactive: true).Single((UIButton b) => b.name == "Choice_0").onClick);
		yield return new WaitForEndOfFrame();
		Screenshot(Path.Combine(output, "music-picker-selected.png"));
		Check(SprkLoginPresentation.SelectedMusic == options.music.First().id && SprkLoginPresentation.SelectedBackground == options.backgrounds.Last().id, "Native picker did not apply independent music choice");
		SprkLoginPresentation.SelectMusic(options.music.Last().id);
		settings.Close();
		yield return null;
		login.GetComponent<SprkLoginSettings>().OpenSettings();
		settings.MainTabs.Single((GameOptionWindow.TabUIInfo t) => t.GameOptionTab == GameOptionTab.Etc).Toggle.Set(state: true);
		yield return null;
		Check(others.GetComponents<SprkLoginPresentationMenu>().Length == 1, "Reopening duplicated menu controller");
		report.Add("othersControlsAndPickers=True");
		report.Add("independentPreferences=True");
		report.Add("invalidPreferenceFallback=True");
		report.Add("legacyCurrentMigrates=True");
		report.Add("currentBackgroundChoiceRemoved=True");
		File.WriteAllLines(Path.Combine(output, "report.txt"), report.ToArray());
		Application.Quit();
	}
}
