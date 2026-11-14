using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MiniJSON;
using NClientData;
using NGame2.NUI;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NGameAsset;
using NGameAsset.NBundleManager;
using NGameAsset.NNguiAtlas;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class SprkIllustrationUpscale : MonoBehaviour
{
	private HeroIllustBook window;

	private Texture source;

	private Material originalMaterial;

	private Rect originalUv;

	private Texture2D enhanced;

	private Material enhancedMaterial;

	private int generation;

	private int requested;

	private bool ready;

	private bool closed;

	private CancellationTokenSource cancellation;

	private Task<byte[]> pending;

	private string status = "";

	private UIButton button;

	private UILabel label;

	private GameObject loading;

	private UILabel loadingLabel;

	private UITexture loadingFill;

	private SprkIllustrationProgress progress;

	private SprkIllustrationReuse reuse = new SprkIllustrationReuse();

	private UIWidget helpAnchor;

	private string storage;

	private string runtime;

	private byte[] lastSource;

	private int inputWidth;

	private int inputHeight;

	private static SprkIllustrationUpscale active;

	private static bool probeStarted;

	private static SprkIllustrationUpscale Get(HeroIllustBook view)
	{
		SprkIllustrationUpscale sprkIllustrationUpscale = view.GetComponent<SprkIllustrationUpscale>();
		if (sprkIllustrationUpscale == null)
		{
			sprkIllustrationUpscale = view.gameObject.AddComponent<SprkIllustrationUpscale>();
			sprkIllustrationUpscale.window = view;
		}
		return sprkIllustrationUpscale;
	}

	public static void BeforeChange(HeroIllustBook view)
	{
		if (!(view == null))
		{
			SprkIllustrationUpscale sprkIllustrationUpscale = Get(view);
			sprkIllustrationUpscale.ResetSource();
			sprkIllustrationUpscale.closed = false;
			if (probeStarted)
			{
				Debug.Log("Illustration probe: change gen=" + sprkIllustrationUpscale.generation);
			}
		}
	}

	public static void Closing(HeroIllustBook view)
	{
		if (!(view == null))
		{
			SprkIllustrationUpscale component = view.GetComponent<SprkIllustrationUpscale>();
			if (component != null)
			{
				component.ResetSource();
				component.closed = true;
			}
		}
	}

	public static PlainAssetHolder Assign(Component owner, UITexture texture, AssetTexture asset, UIHelper.EAssetTextureAssign assign, Action<AssetTexture.TextureInfo> callback)
	{
		HeroIllustBook heroIllustBook = owner as HeroIllustBook;
		if (heroIllustBook == null)
		{
			return UIHelper.ResetPlaneAssetTextureAsync(owner, texture, asset, assign, callback);
		}
		SprkIllustrationUpscale state = Get(heroIllustBook);
		int version = state.generation;
		return UIHelper.ResetPlaneAssetTextureAsync(owner, texture, asset, assign, delegate(AssetTexture.TextureInfo info)
		{
			if (probeStarted)
			{
				Debug.Log("Illustration probe: assigned " + asset.Path + " info=" + (info != null) + " gen=" + version + " current=" + state.generation + " closed=" + state.closed + " texture=" + texture.mainTexture);
			}
			if (callback != null)
			{
				callback(info);
			}
			if (state != null && !state.closed && state.generation == version && info != null && texture.mainTexture != null)
			{
				state.source = texture.mainTexture;
				state.originalUv = texture.uvRect;
				state.originalMaterial = texture.material;
				state.ready = true;
			}
		});
	}

	private void Awake()
	{
		storage = Path.Combine(Application.persistentDataPath, "SprkIllustrationAI");
		runtime = Path.Combine(Application.dataPath, "Managed", "SprkIllustrationRuntime");
	}

	private string ModelDownloadUrl()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_UPDATE_MANIFEST_URL");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			return SprkIllustrationUpscaleCore.ModelUrlFromManifest(environmentVariable);
		}
		string text = Path.Combine(Path.GetDirectoryName(Application.dataPath), "sprk-launcher.json");
		SprkIllustrationLauncherConfig sprkIllustrationLauncherConfig = null;
		if (File.Exists(text))
		{
			if (new FileInfo(text).Length > 65536)
			{
				throw new InvalidDataException("Launcher server profile is too large");
			}
			if (!(Json.Deserialize(File.ReadAllText(text)) is IDictionary<string, object> dictionary))
			{
				throw new InvalidDataException("Launcher server profile is invalid");
			}
			sprkIllustrationLauncherConfig = new SprkIllustrationLauncherConfig
			{
				SelectedProfile = (dictionary.TryGetValue("SelectedProfile", out var value) ? (value as string) : null),
				ManifestUrl = (dictionary.TryGetValue("ManifestUrl", out value) ? (value as string) : null)
			};
			if (dictionary.TryGetValue("Profiles", out value) && value != null)
			{
				IList obj = (value as IList) ?? throw new InvalidDataException("Launcher server profiles are invalid");
				List<SprkIllustrationLauncherProfile> list = new List<SprkIllustrationLauncherProfile>();
				foreach (object item in obj)
				{
					if (!(item is IDictionary<string, object> dictionary2))
					{
						throw new InvalidDataException("Launcher server profile is invalid");
					}
					list.Add(new SprkIllustrationLauncherProfile
					{
						Id = (dictionary2.TryGetValue("Id", out value) ? (value as string) : null),
						HostUrl = (dictionary2.TryGetValue("HostUrl", out value) ? (value as string) : null),
						ManifestUrl = (dictionary2.TryGetValue("ManifestUrl", out value) ? (value as string) : null)
					});
				}
				sprkIllustrationLauncherConfig.Profiles = list.ToArray();
			}
		}
		return SprkIllustrationUpscaleCore.ModelUrlFromProfile(sprkIllustrationLauncherConfig, Environment.GetEnvironmentVariable("SPRK_HOST_URL"));
	}

	private void Cancel()
	{
		if (cancellation != null)
		{
			cancellation.Cancel();
			cancellation.Dispose();
			cancellation = null;
		}
		if (pending != null)
		{
			pending.ContinueWith(delegate(Task<byte[]> task)
			{
				_ = task.Exception;
			}, TaskContinuationOptions.OnlyOnFaulted);
		}
		pending = null;
		progress = null;
	}

	private void Restore()
	{
		if (window != null && window.HeroIllustTexture != null && enhanced != null && window.HeroIllustTexture.mainTexture == enhanced)
		{
			window.HeroIllustTexture.mainTexture = source;
			window.HeroIllustTexture.material = originalMaterial;
			window.HeroIllustTexture.uvRect = originalUv;
		}
		if (enhanced != null)
		{
			UnityEngine.Object.Destroy(enhanced);
			enhanced = null;
		}
		if (enhancedMaterial != null)
		{
			UnityEngine.Object.Destroy(enhancedMaterial);
			enhancedMaterial = null;
		}
	}

	private void ResetSource()
	{
		HideHelp();
		generation++;
		requested = 0;
		Cancel();
		Restore();
		source = null;
		originalMaterial = null;
		ready = false;
		status = "";
		lastSource = null;
		reuse = new SprkIllustrationReuse();
		if (active == this)
		{
			active = null;
		}
		Refresh();
	}

	private void OnDisable()
	{
		if (probeStarted)
		{
			Debug.Log("Illustration probe: disabled gen=" + generation);
		}
		ResetSource();
		closed = true;
	}

	private void OnDestroy()
	{
		ResetSource();
	}

	private void OnApplicationQuit()
	{
		ResetSource();
	}

	private bool Visible()
	{
		if (window != null && window.IsOpen() && window.gameObject.activeInHierarchy)
		{
			return !closed;
		}
		return false;
	}

	private void Refresh()
	{
		if (label != null)
		{
			label.text = SprkText.Get((requested == 0) ? "Off" : "On");
		}
		UpdateLoading();
		if (helpAnchor != null && GameOptionTooltip.IsVisible(helpAnchor))
		{
			ShowHelp();
		}
	}

	private void ShowHelp()
	{
		if (!(helpAnchor == null))
		{
			GameOptionTooltip.Open(helpAnchor, new TooltipTextInfo
			{
				Name = new StringProxy
				{
					Value = SprkText.Get("AI Upscale")
				},
				Desc = new StringProxy
				{
					Value = SprkText.Get("Turn On to enhance this illustration at the model's full x4 resolution. Processing may take time and use additional GPU memory. AI may alter small details. Off displays the original.\nRequires a Vulkan GPU. The first selection downloads the default model.\n") + LocalizedStatus()
				}
			});
		}
	}

	private string LocalizedStatus()
	{
		if (status.StartsWith("Preparing x", StringComparison.Ordinal))
			return SprkText.Format("Preparing x{0}", status.Substring(11));
		if (status.StartsWith("Processing x", StringComparison.Ordinal))
			return SprkText.Format("Processing x{0}", status.Substring(12));
		if (status.StartsWith("Unavailable:", StringComparison.Ordinal))
			return SprkText.Get("Enhancement unavailable. Original displayed.");
		return SprkText.Get(status);
	}

	private void HideHelp()
	{
		if (helpAnchor != null)
		{
			GameOptionTooltip.Hide(helpAnchor);
		}
	}

	private void Cycle()
	{
		Select((requested == 0) ? 4 : 0);
	}

	public void Select(int factor)
	{
		if (!Visible() || (factor != 0 && factor != 4) || factor == requested)
		{
			return;
		}
		generation++;
		Cancel();
		requested = factor;
		status = "";
		if (factor == 0)
		{
			Restore();
			lastSource = null;
			reuse = new SprkIllustrationReuse();
			Refresh();
			return;
		}
		if (!ready || source == null)
		{
			status = "Waiting for artwork";
			Refresh();
			return;
		}
		if (active != null && active != this)
		{
			active.Select(0);
		}
		active = this;
		status = "Preparing x" + factor;
		Refresh();
		StartCoroutine(Prepare(generation, factor));
	}

	private IEnumerator Prepare(int version, int factor)
	{
		yield return null;
		if (version != generation || !Visible())
		{
			yield break;
		}
		try
		{
			if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || SystemInfo.maxTextureSize < 4096)
			{
				throw new InvalidOperationException("This graphics device does not support illustration enhancement");
			}
			inputWidth = Mathf.RoundToInt(Mathf.Abs(originalUv.width) * (float)source.width);
			inputHeight = Mathf.RoundToInt(Mathf.Abs(originalUv.height) * (float)source.height);
			if (inputWidth < 1 || inputHeight < 1 || inputWidth > 1024 || inputHeight > 1024)
			{
				throw new InvalidOperationException("Artwork exceeds the 1024 pixel input limit");
			}
			if (lastSource == null)
			{
				Texture2D texture2D = ReadCrop(source, inputWidth, inputHeight, originalUv);
				try
				{
					lastSource = texture2D.EncodeToPNG();
				}
				finally
				{
					UnityEngine.Object.Destroy(texture2D);
				}
			}
			byte[] bytes = lastSource;
			int w = inputWidth;
			int h = inputHeight;
			string model = SprkIllustrationUpscaleCore.ModelPath(storage);
			string cache = Path.Combine(storage, "cache");
			string modelUrl = (SprkIllustrationUpscaleCore.Nonempty(model) ? null : ModelDownloadUrl());
			cancellation = new CancellationTokenSource();
			CancellationToken token = cancellation.Token;
			progress = new SprkIllustrationProgress();
			SprkIllustrationProgress jobProgress = progress;
			SprkIllustrationReuse jobReuse = reuse;
			pending = Task.Run(() => SprkIllustrationUpscaleCore.Run(bytes, w, h, factor, model, runtime, cache, token, modelUrl, jobProgress, jobReuse), token);
			status = "Processing x" + factor;
			Refresh();
		}
		catch (Exception error)
		{
			Fail(error);
		}
	}

	private static Texture2D ReadCrop(Texture texture, int width, int height, Rect uv)
	{
		RenderTexture renderTexture = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		Texture2D texture2D = null;
		try
		{
			bool sRGBWrite = GL.sRGBWrite;
			try
			{
				GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
				Graphics.Blit(texture, temporary, new Vector2(uv.width, uv.height), new Vector2(uv.x, uv.y));
			}
			finally
			{
				GL.sRGBWrite = sRGBWrite;
			}
			RenderTexture.active = temporary;
			texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false, linear: false);
			texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, recalculateMipMaps: false);
			texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: false);
			return texture2D;
		}
		catch
		{
			if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
			throw;
		}
		finally
		{
			RenderTexture.active = renderTexture;
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	private void Fail(Exception error)
	{
		Restore();
		progress = null;
		reuse.Clear();
		status = "Unavailable: " + error.GetBaseException().Message + ". Original displayed.";
		Debug.LogWarning("SPRK illustration AI: " + error.GetBaseException().Message);
		Refresh();
	}

	private void Update()
	{
		if (!Visible())
		{
			return;
		}
		if (button == null)
		{
			CreateControls();
		}
		if (button != null && window.WidgetHideUI != null)
		{
			bool flag = window.WidgetHideUI.gameObject.activeInHierarchy && window.WidgetHideUI.alpha > 0.01f;
			if (button.gameObject.activeSelf != flag)
			{
				if (!flag)
				{
					HideHelp();
				}
				button.gameObject.SetActive(flag);
			}
		}
		if (label != null)
		{
			label.alpha = ((pending != null || status == "Waiting for artwork" || status.StartsWith("Preparing")) ? (0.65f + 0.35f * Mathf.Sin(Time.realtimeSinceStartup * 5f) * Mathf.Sin(Time.realtimeSinceStartup * 5f)) : 1f);
		}
		UpdateLoading();
		if (requested != 0 && ready && pending == null && status == "Waiting for artwork")
		{
			status = "Preparing x" + requested;
			Refresh();
			StartCoroutine(Prepare(generation, requested));
		}
		if (pending == null || !pending.IsCompleted)
		{
			return;
		}
		Task<byte[]> task = pending;
		pending = null;
		if (task.IsCanceled)
		{
			return;
		}
		if (task.IsFaulted)
		{
			Fail(task.Exception);
		}
		else
		{
			if (requested == 0 || !ready || (window.HeroIllustTexture.mainTexture != source && window.HeroIllustTexture.mainTexture != enhanced))
			{
				return;
			}
			Texture2D texture2D = null;
			try
			{
				texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: false);
				if (!texture2D.LoadImage(task.Result, markNonReadable: false) || texture2D.width != inputWidth * 4 || texture2D.height != inputHeight * 4)
				{
					throw new InvalidDataException("Enhanced PNG could not be decoded");
				}
				texture2D.wrapMode = TextureWrapMode.Clamp;
				texture2D.filterMode = FilterMode.Bilinear;
				texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				texture2D.name = "SPRK Illustration x" + requested;
				Restore();
				enhanced = texture2D;
				texture2D = null;
				if (originalMaterial != null)
				{
					enhancedMaterial = new Material(originalMaterial);
					enhancedMaterial.mainTexture = enhanced;
				}
				window.HeroIllustTexture.material = enhancedMaterial;
				window.HeroIllustTexture.mainTexture = enhanced;
				window.HeroIllustTexture.uvRect = new Rect(0f, 0f, 1f, 1f);
				progress = null;
				reuse.Clear();
				lastSource = null;
				status = "On (x4 ready)";
				Refresh();
			}
			catch (Exception error)
			{
				if (texture2D != null)
				{
					UnityEngine.Object.Destroy(texture2D);
				}
				byte[] bytes = lastSource;
				int scale = requested;
				if (bytes != null)
				{
					Task.Run(delegate
					{
						SprkIllustrationUpscaleCore.RemoveCache(bytes, SprkIllustrationUpscaleCore.ModelPath(storage), scale, Path.Combine(storage, "cache"));
					});
				}
				Fail(error);
			}
		}
	}

	private void UpdateLoading()
	{
		if (loading == null)
		{
			return;
		}
		bool flag = requested != 0 && (pending != null || status.StartsWith("Preparing") || status == "Waiting for artwork") && button != null && button.gameObject.activeInHierarchy;
		if (loading.activeSelf != flag)
		{
			loading.SetActive(flag);
		}
		if (flag)
		{
			string text = ((status == "Waiting for artwork") ? "Waiting for artwork" : "Preparing");
			float value = -1f;
			if (progress != null)
			{
				progress.Read(out text, out value);
			}
			loadingLabel.text = SprkText.Get(text) + ((value < 0f) ? "..." : (" " + Mathf.FloorToInt(Mathf.Clamp01(value) * 100f) + "%"));
			loadingFill.width = Mathf.Max(2, Mathf.RoundToInt(240f * ((value < 0f) ? 0.35f : Mathf.Clamp01(value))));
			loadingFill.alpha = ((value < 0f) ? (0.55f + 0.45f * Mathf.Sin(Time.realtimeSinceStartup * 4f) * Mathf.Sin(Time.realtimeSinceStartup * 4f)) : 1f);
		}
	}

	private void CreateControls()
	{
		UIWidget widgetHideUI = window.WidgetHideUI;
		if (widgetHideUI == null)
		{
			return;
		}
		UIButton uIButton = widgetHideUI.GetComponent<UIButton>();
		if (uIButton == null)
		{
			uIButton = widgetHideUI.GetComponentInChildren<UIButton>(includeInactive: true);
		}
		UILabel componentInChildren = widgetHideUI.GetComponentInChildren<UILabel>(includeInactive: true);
		UISprite uISprite = ((uIButton != null && uIButton.tweenTarget != null) ? uIButton.tweenTarget.GetComponent<UISprite>() : null);
		if (uISprite == null)
		{
			UISprite[] componentsInChildren = widgetHideUI.GetComponentsInChildren<UISprite>(includeInactive: true);
			foreach (UISprite uISprite2 in componentsInChildren)
			{
				if (uISprite == null || uISprite2.width * uISprite2.height > uISprite.width * uISprite.height)
				{
					uISprite = uISprite2;
				}
			}
		}
		if (uIButton == null || componentInChildren == null || uISprite == null)
		{
			return;
		}
		GameObject gameObject = new GameObject("SPRK Illustration Controls");
		gameObject.layer = widgetHideUI.gameObject.layer;
		gameObject.transform.SetParent(window.transform, worldPositionStays: false);
		UIPanel uIPanel = gameObject.AddComponent<UIPanel>();
		UIPanel componentInParent = uISprite.GetComponentInParent<UIPanel>();
		uIPanel.depth = ((componentInParent == null) ? 1000 : (componentInParent.depth + 1));
		GameObject gameObject2 = new GameObject("SPRK AI Upscale");
		gameObject2.layer = widgetHideUI.gameObject.layer;
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		UISprite uISprite3 = gameObject2.AddComponent<UISprite>();
		uISprite3.atlas = uISprite.atlas;
		uISprite3.spriteName = uISprite.spriteName;
		uISprite3.type = uISprite.type;
		uISprite3.color = uISprite.color;
		uISprite3.depth = uISprite.depth;
		uISprite3.width = 120;
		uISprite3.height = widgetHideUI.height;
		uISprite3.SetAnchor(widgetHideUI.gameObject);
		uISprite3.leftAnchor.Set(0f, -132f);
		uISprite3.rightAnchor.Set(0f, -12f);
		uISprite3.bottomAnchor.Set(0f, 0f);
		uISprite3.topAnchor.Set(1f, 0f);
		uISprite3.UpdateAnchors();
		button = gameObject2.AddComponent<UIButton>();
		EventDelegate.Add(button.onClick, Cycle);
		button.tweenTarget = gameObject2;
		button.defaultColor = uIButton.defaultColor;
		button.hover = uIButton.hover;
		button.pressed = uIButton.pressed;
		button.disabledColor = uIButton.disabledColor;
		button.normalSprite = (string.IsNullOrEmpty(uIButton.normalSprite) ? uISprite.spriteName : uIButton.normalSprite);
		button.hoverSprite = uIButton.hoverSprite;
		button.pressedSprite = uIButton.pressedSprite;
		button.disabledSprite = uIButton.disabledSprite;
		button.enabled = true;
		button.isEnabled = true;
		GameObject gameObject3 = new GameObject("Label");
		gameObject3.layer = gameObject2.layer;
		gameObject3.transform.SetParent(gameObject2.transform, worldPositionStays: false);
		label = gameObject3.AddComponent<UILabel>();
		label.bitmapFont = componentInChildren.bitmapFont;
		label.trueTypeFont = componentInChildren.trueTypeFont;
		label.fontSize = componentInChildren.fontSize;
		label.depth = uISprite.depth + 1;
		label.color = componentInChildren.color;
		label.effectStyle = componentInChildren.effectStyle;
		label.effectColor = componentInChildren.effectColor;
		label.overflowMethod = UILabel.Overflow.ShrinkContent;
		label.maxLineCount = 1;
		label.SetAnchor(gameObject2);
		label.leftAnchor.Set(0f, 10f);
		label.rightAnchor.Set(1f, -10f);
		label.bottomAnchor.Set(0f, 0f);
		label.topAnchor.Set(1f, 0f);
		label.UpdateAnchors();
		NGUITools.AddWidgetCollider(gameObject2);
		uISprite3.autoResizeBoxCollider = true;
		helpAnchor = uISprite3;
		Collider[] components = gameObject2.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		TooltipPrintInfo tooltipPrintInfo = gameObject2.GetComponent<TooltipPrintInfo>();
		if (tooltipPrintInfo == null)
		{
			tooltipPrintInfo = gameObject2.AddComponent<TooltipPrintInfo>();
		}
		tooltipPrintInfo.Mode = UITooltipTrigger.ETooltipTriggerMode.ClickAndTooltip;
		tooltipPrintInfo.CustomAnchor = uISprite3;
		tooltipPrintInfo.OwnerPivot = UIWidget.Pivot.BottomRight;
		tooltipPrintInfo.TooltipPivot = UIWidget.Pivot.TopRight;
		UIEventListener.Get(gameObject2).onHover = delegate(GameObject go, bool over)
		{
			if (over)
			{
				ShowHelp();
			}
			else
			{
				HideHelp();
			}
		};
		loading = new GameObject("SPRK AI Loading");
		loading.layer = gameObject2.layer;
		loading.transform.SetParent(gameObject.transform, worldPositionStays: false);
		UITexture uITexture = loading.AddComponent<UITexture>();
		uITexture.mainTexture = Texture2D.whiteTexture;
		uITexture.width = 260;
		uITexture.height = 62;
		uITexture.SetAnchor(gameObject2);
		uITexture.leftAnchor.Set(0f, 0f);
		uITexture.rightAnchor.Set(0f, 260f);
		uITexture.bottomAnchor.Set(0f, -70f);
		uITexture.topAnchor.Set(0f, -8f);
		uITexture.UpdateAnchors();
		uITexture.width = 260;
		uITexture.height = 62;
		uITexture.color = new Color(0.025f, 0.035f, 0.05f, 0.9f);
		uITexture.depth = uISprite.depth + 2;
		GameObject gameObject4 = new GameObject("Progress text");
		gameObject4.layer = gameObject2.layer;
		gameObject4.transform.SetParent(loading.transform, worldPositionStays: false);
		loadingLabel = gameObject4.AddComponent<UILabel>();
		loadingLabel.bitmapFont = componentInChildren.bitmapFont;
		loadingLabel.trueTypeFont = componentInChildren.trueTypeFont;
		loadingLabel.fontSize = 22;
		loadingLabel.color = new Color(1f, 0.89f, 0.62f, 1f);
		loadingLabel.depth = uISprite.depth + 3;
		loadingLabel.maxLineCount = 1;
		loadingLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
		loadingLabel.SetAnchor(loading);
		loadingLabel.leftAnchor.Set(0f, 10f);
		loadingLabel.rightAnchor.Set(1f, -10f);
		loadingLabel.bottomAnchor.Set(0f, 20f);
		loadingLabel.topAnchor.Set(1f, -4f);
		loadingLabel.UpdateAnchors();
		GameObject gameObject5 = new GameObject("Progress track");
		gameObject5.layer = gameObject2.layer;
		gameObject5.transform.SetParent(loading.transform, worldPositionStays: false);
		UITexture uITexture2 = gameObject5.AddComponent<UITexture>();
		uITexture2.mainTexture = Texture2D.whiteTexture;
		uITexture2.color = new Color(0.2f, 0.18f, 0.13f, 1f);
		uITexture2.width = 240;
		uITexture2.height = 6;
		uITexture2.depth = uISprite.depth + 3;
		uITexture2.transform.localPosition = new Vector3(0f, -20f, 0f);
		GameObject gameObject6 = new GameObject("Progress fill");
		gameObject6.layer = gameObject2.layer;
		gameObject6.transform.SetParent(gameObject5.transform, worldPositionStays: false);
		loadingFill = gameObject6.AddComponent<UITexture>();
		loadingFill.mainTexture = Texture2D.whiteTexture;
		loadingFill.color = new Color(0.93f, 0.72f, 0.32f, 1f);
		loadingFill.pivot = UIWidget.Pivot.Left;
		loadingFill.width = 2;
		loadingFill.height = 6;
		loadingFill.depth = uISprite.depth + 4;
		loadingFill.transform.localPosition = new Vector3(-120f, 0f, 0f);
		Refresh();
	}

	public static void EnsureProbe()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("SPRK_ILLUSTRATION_AI_TEST");
		if (!probeStarted && !string.IsNullOrEmpty(environmentVariable) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")))
		{
			probeStarted = true;
			Application.runInBackground = true;
			Directory.CreateDirectory(environmentVariable);
			GameObject obj = new GameObject("SPRK Illustration AI Probe");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.AddComponent<SprkIllustrationProbeRunner>().Begin(environmentVariable);
		}
	}

	public static IEnumerator Probe(string output)
	{
		yield return new WaitForSecondsRealtime(5f);
		TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
		typeof(Define).GetProperty("PatchDownloaded").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { true });
		SingletonAuto<LocalizationLoader>.instance.SetEnableLoadFromPatch(enableLoad: true);
		SingletonAuto<TableDataManager>.instance.Load();
		string text = Path.Combine(Application.dataPath, "Documents", "Patch", "StandaloneWindows");
		Dictionary<string, AssetInfo> dictionary = new Dictionary<string, AssetInfo>();
		MethodInfo setMethod = typeof(AssetInfo).GetProperty("EntryName").GetSetMethod(nonPublic: true);
		string[] files = Directory.GetFiles(Path.Combine(text, "Assetbundle"), "*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			string text2 = files[i].Substring(text.Length + 1).Replace('\\', '/');
			AssetInfo assetInfo = new AssetInfo();
			setMethod.Invoke(assetInfo, new object[1] { text2 });
			dictionary.Add(text2, assetInfo);
		}
		SingletonAuto<AssetFileManager>.instance.Setup(dictionary);
		typeof(UnityCache).GetProperty("persistentDataPath").GetSetMethod(nonPublic: true).Invoke(null, new object[1] { Application.dataPath });
		LoaderPlaying loader = SingletonUpdater<LoaderPlaying>.instance;
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		loader.GetType().GetMethod("LoadPackFromBinary", flags).Invoke(loader, null);
		loader.GetType().GetField("_isReady", flags).SetValue(loader, true);
		GameRoot instance = GameRoot.Instance;
		instance.CostumeDataContainer.Load();
		List<CostumeData> costumes = (from c in instance.CostumeDataContainer.GetAllData()
			where !string.IsNullOrEmpty(c.IllustPath)
			select c).Take(2).ToList();
		if (costumes.Count < 2)
		{
			throw new Exception("No illustration test costumes");
		}
		foreach (CostumeData item in costumes)
		{
			AtlasDataContainer instance2 = SingletonAuto<AtlasDataContainer>.instance;
			AtlasDataContainer.AtlasLink link = instance2.GetAtlasLink(item.IllustPath);
			AtlasData atlas = instance2.Get(link.AtlasCode);
			IBundleLoad load = SingletonAuto<BundleManager>.instance.LockBundle(atlas.BundleFileName);
			FieldInfo field = typeof(BundleLoadBase).GetField("_DownloadState", flags);
			field.SetValue(load, Enum.ToObject(field.FieldType, 2));
			load.StartLoad();
			float until = Time.realtimeSinceStartup + 30f;
			while (!load.IsDone && Time.realtimeSinceStartup < until)
			{
				SingletonUpdater<AsyncManager>.instance.ChainUpdate();
				yield return null;
			}
			if (!load.IsValid)
			{
				throw new Exception("Native atlas preload failed: " + load.LoadMessage);
			}
			AssetBundle assetBundle = (AssetBundle)typeof(BundleLoadBase).GetField("AssetBundle", flags).GetValue(load);
			File.AppendAllText(Path.Combine(output, "paths.txt"), "\nloaded=" + atlas.BundleFileName + " assets=" + string.Join(",", assetBundle.GetAllAssetNames()) + " main=" + assetBundle.mainAsset);
			INguiAtlas nativeAtlas = SingletonAuto<NguiAtlasContainer>.instance.Lock(link.AtlasCode);
			until = Time.realtimeSinceStartup + 30f;
			while (!nativeAtlas.IsDone && Time.realtimeSinceStartup < until)
			{
				SingletonUpdater<AsyncManager>.instance.ChainUpdate();
				yield return null;
			}
			if (!nativeAtlas.IsValid)
			{
				throw new Exception("Native atlas object failed: " + nativeAtlas.LoadMessage + " done=" + nativeAtlas.IsDone);
			}
			File.AppendAllText(Path.Combine(output, "paths.txt"), "\natlas=" + link.AtlasCode + " texture=" + nativeAtlas.Value.texture?.ToString() + " sprite=" + link.SpriteName + " found=" + (nativeAtlas.Value.GetSprite(link.SpriteName) != null));
		}
		LoginManagement instance3 = SceneAutoBehaviour<LoginManagement>.instance;
		instance3.AddInstance<HeroIllustBook>("Resources/Prefab2/Window/IllustratedBook/HeroIllustBook");
		HeroIllustBook view = instance3.GetInstance<HeroIllustBook>();
		int nativeUiLayer = view.LabelHeroName.gameObject.layer;
		view.Init(null, null, costumes[0].CostumeIndex, new List<int>
		{
			costumes[0].CostumeIndex,
			costumes[1].CostumeIndex
		});
		view.Open();
		UIPanel fixturePanel = NGUITools.CreateUI(advanced3D: false, 30);
		UIRoot componentInParent = fixturePanel.GetComponentInParent<UIRoot>();
		componentInParent.scalingStyle = UIRoot.Scaling.Constrained;
		componentInParent.manualWidth = 1920;
		componentInParent.manualHeight = 1080;
		componentInParent.fitWidth = true;
		componentInParent.fitHeight = true;
		view.transform.SetParent(fixturePanel.transform, worldPositionStays: false);
		view.transform.localPosition = Vector3.zero;
		view.transform.localScale = Vector3.one;
		NGUITools.SetLayer(view.gameObject, 30);
		yield return new WaitForSecondsRealtime(1f);
		SprkIllustrationUpscale state = Get(view);
		state.storage = Path.Combine(output, "working");
		Directory.CreateDirectory(state.storage);
		string fixture = Environment.GetEnvironmentVariable("SPRK_ILLUSTRATION_MODEL_FIXTURE");
		if (!string.IsNullOrEmpty(fixture))
		{
			string provisioned = Path.Combine(state.storage, "provision.sprkmodel");
			bool[] array = new bool[2] { false, true };
			for (int num = 0; num < array.Length; num++)
			{
				if (array[num])
				{
					File.WriteAllBytes(provisioned, new byte[0]);
				}
				Task job = Task.Run(delegate
				{
					SprkIllustrationUpscaleCore.Provision(provisioned, CancellationToken.None, fixture + "/default");
				});
				while (!job.IsCompleted)
				{
					yield return null;
				}
				job.GetAwaiter().GetResult();
				if (SprkIllustrationUpscaleCore.Hash(File.ReadAllBytes(provisioned)) != "f18ef1a86726f26288739b107f2bb6d4b815d04bc14f6bb7b3d0dacbc1a1fdba")
				{
					throw new Exception("Provisioned default hash mismatch");
				}
			}
			string race = Path.Combine(state.storage, "provision-race.sprkmodel");
			Task raced = Task.Run(delegate
			{
				SprkIllustrationUpscaleCore.Provision(race, CancellationToken.None, fixture + "/race");
			});
			while (!raced.IsCompleted)
			{
				yield return null;
			}
			raced.GetAwaiter().GetResult();
			if (File.ReadAllText(race) != "user replacement")
			{
				throw new Exception("Concurrent user replacement was overwritten");
			}
			string interrupted = Path.Combine(state.storage, "interrupted.sprkmodel");
			Task truncated = Task.Run(delegate
			{
				SprkIllustrationUpscaleCore.Provision(interrupted, CancellationToken.None, fixture + "/truncated");
			});
			while (!truncated.IsCompleted)
			{
				yield return null;
			}
			if (!truncated.IsFaulted || File.Exists(interrupted))
			{
				throw new Exception("Partial model was activated");
			}
			_ = truncated.Exception;
			File.WriteAllText(Path.Combine(output, "provisioning.txt"), "PASS: missing/zero-length downloads, pinned hash, atomic activation, concurrent user replacement, truncated download rejection");
		}
		if (string.IsNullOrEmpty(fixture))
		{
			File.Copy(Path.Combine(output, "default.sprkmodel"), Path.Combine(state.storage, "active.sprkmodel"), overwrite: true);
		}
		float deadline = Time.realtimeSinceStartup + 30f;
		while (!state.ready && Time.realtimeSinceStartup < deadline)
		{
			loader.ChainUpdate();
			SingletonUpdater<AsyncManager>.instance.ChainUpdate();
			yield return null;
		}
		if (!state.ready)
		{
			throw new Exception("Native artwork assignment did not finish; closed=" + state.closed + " open=" + view.IsOpen() + " gen=" + state.generation + " texture=" + view.HeroIllustTexture.mainTexture?.ToString() + " path=" + costumes[0].IllustPath);
		}
		List<string> report = new List<string>
		{
			"player=" + Application.unityVersion + " gpu=" + SystemInfo.graphicsDeviceName + " api=" + SystemInfo.graphicsDeviceType.ToString() + " colors=" + QualitySettings.activeColorSpace,
			"source=" + state.source.name + " " + state.source.width + "x" + state.source.height + " format=" + ((Texture2D)state.source).format.ToString() + " uv=" + state.originalUv.ToString("F6") + " material=" + ((state.originalMaterial == null) ? "null" : state.originalMaterial.shader.name)
		};
		if (state.requested != 0 || state.pending != null || state.enhanced != null)
		{
			throw new Exception("Viewer did not open Off");
		}
		if (!string.IsNullOrEmpty(fixture) && File.Exists(Path.Combine(state.storage, "active.sprkmodel")))
		{
			throw new Exception("Off provisioned a model");
		}
		if (state.button == null || !state.button.transform.IsChildOf(view.transform) || state.button.GetComponent<Collider>() == null)
		{
			throw new Exception("Viewer control/collider missing");
		}
		UISprite uISprite = state.helpAnchor as UISprite;
		if (uISprite == null || uISprite.atlas == null || uISprite.leftAnchor.target != view.WidgetHideUI.transform || uISprite.rightAnchor.absolute != -12 || state.label.text != "Off")
		{
			throw new Exception("Native-style button anchor/label missing");
		}
		Vector3[] worldCorners = uISprite.worldCorners;
		Vector3[] worldCorners2 = view.WidgetHideUI.worldCorners;
		Vector3 vector = view.transform.InverseTransformPoint(worldCorners[2]);
		Vector3 vector2 = view.transform.InverseTransformPoint(worldCorners2[1]);
		if (System.Math.Abs(vector2.x - vector.x - 12f) > 1f || System.Math.Abs(vector2.y - vector.y) > 1f)
		{
			throw new Exception("AI button is not beside Hide UI");
		}
		if (view.transform.Find("SPRK Illustration Controls/SPRK AI Status") != null)
		{
			throw new Exception("Old status text remains visible");
		}
		report.Add("compactNativeButton=true label=" + state.label.text + " sprite=" + uISprite.spriteName);
		state.helpAnchor.gameObject.layer = nativeUiLayer;
		state.ShowHelp();
		GameOptionTooltip gameOptionTooltip = Resources.FindObjectsOfTypeAll<GameOptionTooltip>().FirstOrDefault((GameOptionTooltip t) => t.gameObject.scene.IsValid() && t.gameObject.activeSelf);
		if (gameOptionTooltip != null)
		{
			gameOptionTooltip.transform.SetParent(fixturePanel.transform, worldPositionStays: false);
			NGUITools.SetLayer(gameOptionTooltip.gameObject, 30);
		}
		if (!GameOptionTooltip.IsVisible(state.helpAnchor))
		{
			throw new Exception("Native viewer help missing; anchor=" + state.helpAnchor.gameObject.activeInHierarchy + " print=" + state.helpAnchor.GetComponentInParent<TooltipPrintInfo>()?.ToString() + " tooltips=" + string.Join(",", from t in Resources.FindObjectsOfTypeAll<GameOptionTooltip>()
				select t.name + " active=" + t.gameObject.activeSelf + " scene=" + t.gameObject.scene.IsValid()));
		}
		state.HideHelp();
		state.helpAnchor.gameObject.layer = 30;
		int width = view.HeroIllustTexture.width;
		int height = view.HeroIllustTexture.height;
		if ((int)typeof(HeroIllustBook).GetField("_scaleLimitMax", flags).GetValue(view) != 8)
		{
			throw new Exception("Native touch zoom limit did not increase");
		}
		typeof(HeroIllustBook).GetMethod("SetZoomRate", flags).Invoke(view, new object[1] { 50f });
		if (view.IllustRoot.localScale.x != 8f)
		{
			throw new Exception("Native zoom did not clamp to 800%");
		}
		view.OnDoubleClick();
		yield return new WaitForSecondsRealtime(0.6f);
		if (System.Math.Abs(view.IllustRoot.localScale.x - 1f) > 0.001f)
		{
			throw new Exception("Reset did not restore 100% after extended zoom");
		}
		report.Add("nativeZoom800AndReset=true");
		view.IllustRoot.localScale = new Vector3(2f, 2f, 1f);
		view.IllustRoot.localPosition = new Vector3(30f, 20f, 0f);
		Vector3 zoom = view.IllustRoot.localScale;
		Vector3 pan = view.IllustRoot.localPosition;
		bool sawProgress = false;
		EventDelegate.Execute(state.button.onClick);
		if (state.requested != 4 || state.label.text != "On")
		{
			throw new Exception("Off did not toggle directly to On/x4");
		}
		state.Select(2);
		if (state.requested != 4 || state.label.text != "On")
		{
			throw new Exception("Removed x2 option was accepted");
		}
		float start = Time.realtimeSinceStartup;
		deadline = start + 130f;
		while (state.enhanced == null && Time.realtimeSinceStartup < deadline && !state.status.StartsWith("Unavailable"))
		{
			yield return null;
			if (state.progress == null)
			{
				continue;
			}
			state.progress.Read(out var text3, out var value);
			if (text3 == "Upscaling" && value > 0.1f && value < 0.9f && !sawProgress)
			{
				if (state.loading == null || !state.loading.activeSelf || !state.loadingLabel.text.Contains("%"))
				{
					throw new Exception("Visible tile progress missing");
				}
				sawProgress = true;
				Task<byte[]> task = state.pending;
				state.Select(4);
				if (state.pending != task)
				{
					throw new Exception("Repeated On restarted the job");
				}
				yield return new WaitForEndOfFrame();
				CaptureProbe(Path.Combine(output, "viewer-loading.png"));
			}
		}
		if (state.enhanced == null || state.enhanced.width != state.inputWidth * 4 || state.enhanced.height != state.inputHeight * 4 || state.label.text != "On")
		{
			throw new Exception("Full x4 enhancement failed: " + state.status);
		}
		if (view.HeroIllustTexture.width != width || view.HeroIllustTexture.height != height || view.IllustRoot.localScale != zoom || view.IllustRoot.localPosition != pan)
		{
			throw new Exception("Enhancement changed logical size or viewer transform");
		}
		if (!sawProgress)
		{
			throw new Exception("GPU progress not exercised");
		}
		report.Add("onFullX4=" + state.enhanced.width + "x" + state.enhanced.height + " seconds=" + (Time.realtimeSinceStartup - start));
		yield return new WaitForEndOfFrame();
		CaptureProbe(Path.Combine(output, "viewer-on.png"));
		EventDelegate.Execute(state.button.onClick);
		if (state.requested != 0 || state.label.text != "Off" || view.HeroIllustTexture.mainTexture != state.source || view.HeroIllustTexture.uvRect != state.originalUv || state.enhanced != null || state.loading.activeSelf)
		{
			throw new Exception("On did not toggle directly to Off/restore original");
		}
		string executable = Path.Combine(state.runtime, "realesrgan-ncnn-vulkan.exe");
		File.Move(executable, executable + ".probe-disabled");
		EventDelegate.Execute(state.button.onClick);
		start = Time.realtimeSinceStartup;
		deadline = start + 20f;
		while (state.enhanced == null && Time.realtimeSinceStartup < deadline && !state.status.StartsWith("Unavailable"))
		{
			yield return null;
		}
		if (state.enhanced == null || state.label.text != "On")
		{
			throw new Exception("Repeated On did not reuse disk cache");
		}
		report.Add("offOnOnly=true x2Rejected=true visibleTileProgress=true cachedOnWithoutRuntimeSeconds=" + (Time.realtimeSinceStartup - start));
		File.Move(executable + ".probe-disabled", executable);
		state.Select(0);
		state.Select(4);
		yield return null;
		state.Select(0);
		yield return new WaitForSecondsRealtime(0.5f);
		if (state.enhanced != null || state.pending != null)
		{
			throw new Exception("Cancelled result was assigned");
		}
		report.Add("offDuringProcessing=true");
		view.OnClickHideUI();
		yield return new WaitForSecondsRealtime(0.6f);
		if (state.button.gameObject.activeSelf)
		{
			throw new Exception("AI control remained visible during Hide UI");
		}
		view.OnClickClose();
		yield return new WaitForSecondsRealtime(0.6f);
		if (!state.button.gameObject.activeSelf || !view.IsOpen())
		{
			throw new Exception("Native Show UI did not restore controls");
		}
		report.Add("helpAndHideUI=true");
		state.Select(4);
		view.OnClickNext();
		yield return new WaitForSecondsRealtime(1f);
		if (state.requested != 0 || state.enhanced != null)
		{
			throw new Exception("Navigation retained enhancement");
		}
		report.Add("navigationOff=true");
		view.Close();
		yield return null;
		view.InitSeasonReward(null, 0, costumes);
		view.Open();
		yield return new WaitForSecondsRealtime(1f);
		if (state.requested != 0 || !state.ready)
		{
			throw new Exception("Season viewer did not open Off with its native source");
		}
		state.Select(4);
		deadline = Time.realtimeSinceStartup + 130f;
		while (state.enhanced == null && Time.realtimeSinceStartup < deadline && !state.status.StartsWith("Unavailable"))
		{
			yield return null;
		}
		if (state.enhanced == null)
		{
			throw new Exception("Season reward enhancement failed: " + state.status);
		}
		report.Add("seasonReward=true");
		view.Close();
		yield return null;
		if (state.enhanced != null || state.pending != null || state.source != null)
		{
			throw new Exception("Close retained enhancement resources");
		}
		report.Add("closeReleases=true");
		File.WriteAllLines(Path.Combine(output, "PASS.txt"), report.ToArray());
		Application.Quit();
	}

	private static void CaptureProbe(string path)
	{
		Camera cachedCamera = UICamera.FindCameraForLayer(30).cachedCamera;
		RenderTexture renderTexture = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(1280, 720, 24);
		RenderTexture targetTexture = cachedCamera.targetTexture;
		cachedCamera.targetTexture = temporary;
		cachedCamera.Render();
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(1280, 720, TextureFormat.RGBA32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
		texture2D.Apply();
		cachedCamera.targetTexture = targetTexture;
		RenderTexture.active = renderTexture;
		RenderTexture.ReleaseTemporary(temporary);
		File.WriteAllBytes(path, texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
	}
}
