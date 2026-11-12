using System;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NUI.NRender3D.NPreviewCamera;
using NVespa;
using NVespa.NCamera;
using NVespa.NGui;
using NVespa.NPrefab2;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NUI.NRender3D;

public class HeroPreview : LinkFactoryComponent, IInspectorGuiDrawer
{
	public struct InfoData
	{
		public int RenderTargetWidth;

		public int RenderTargetHeight;

		public string BackgroundName;

		public RenderFacadeBuilder RenderFacadeBuilder;

		public bool IsEquals(InfoData info)
		{
			if (RenderTargetWidth != info.RenderTargetWidth)
			{
				return false;
			}
			if (RenderTargetHeight != info.RenderTargetHeight)
			{
				return false;
			}
			if (!string.Equals(BackgroundName, info.BackgroundName))
			{
				return false;
			}
			if (RenderFacadeBuilder != info.RenderFacadeBuilder)
			{
				if (RenderFacadeBuilder == null || info.RenderFacadeBuilder == null)
				{
					return false;
				}
				if (!RenderFacadeBuilder.GetStock().IsEquals(info.RenderFacadeBuilder.GetStock()))
				{
					return false;
				}
			}
			return true;
		}
	}

	[Serializable]
	public class BackgroundInfo
	{
		public string Name;

		public Transform Root;
	}

	public const string PrefabPath = "Prefab2/Hero3DPreview/Hero3DPreview";

	public static PrefabAuto<HeroPreview> PrefabAuto = new PrefabAuto<HeroPreview>("Prefab2/Hero3DPreview/Hero3DPreview");

	public static Transform _previewRoot;

	public GameObject HeroRoot;

	public Camera MainCamera;

	public GameObject BackgroundRoot;

	public BackgroundInfo[] BackgroundInfos;

	private int _layerLock = -1;

	private IPreviewCamera _previewCamera;

	private PostProcessVolume _postProcessVolume;

	public static Transform PreviewRoot
	{
		get
		{
			if (_previewRoot == null)
			{
				_previewRoot = new GameObject("GamePreviewRoot").transform;
			}
			return _previewRoot;
		}
	}

	public InfoData Info { get; private set; }

	public RenderTexture RenderTarget { get; private set; }

	public RenderFacade RenderFacade { get; private set; }

	public OrthographicDefault DefaultOrtho { get; private set; }

	public PerspectiveDefault DefaultPerspective { get; private set; }

	public IPreviewCamera PreviewCamera
	{
		get
		{
			return _previewCamera;
		}
		set
		{
			if (_previewCamera != value)
			{
				_previewCamera = value;
				_previewCamera.AssignPreview(this);
			}
		}
	}

	public static void OnSceneChanged()
	{
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(_previewRoot))
		{
			_previewRoot = null;
		}
	}

	public static HeroPreview CreateInstance(InfoData info)
	{
		HeroPreview heroPreview = PrefabAuto.Factory.Create(PreviewRoot, -1);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroPreview.transform);
		heroPreview.Open(info);
		return heroPreview;
	}

	public static void ReleaseInstance(HeroPreview heroPreview)
	{
		if (!(heroPreview == null))
		{
			heroPreview.Close();
			FactoryBase.ReleaseComponent(heroPreview);
		}
	}

	internal void Open(InfoData info)
	{
		StopAllCoroutines();
		_layerLock = SingletonAuto<LayerManager>.instance.LockLayer();
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(base.gameObject, _layerLock);
		MainCamera.cullingMask = 1 << _layerLock;
		Info = info;
		if (RenderTarget != null && (RenderTarget.width != Info.RenderTargetWidth || RenderTarget.height != Info.RenderTargetHeight))
		{
			VespaObject.DestroyObject(RenderTarget);
			RenderTarget = null;
		}
		if (RenderTarget == null)
		{
			RenderTarget = SprkGraphicsOptions.CreateTarget(Info.RenderTargetWidth, Info.RenderTargetHeight, 24);
			MainCamera.targetTexture = RenderTarget;
			Vespa_Unity_Component_Extension_Method.GetComponentAuto<TargetTextureCache>(MainCamera);
			_postProcessVolume = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<PostProcessVolume>(MainCamera.gameObject);
			_postProcessVolume.profile = VespaResource.Load<PostProcessProfile>("Effect/PostProcessPreview");
			_postProcessVolume.isGlobal = true;
			_postProcessVolume.enabled = SystemOptionData.Render_EnableRimLight;
		}
		if (String_Extension_Method.IsNullOrEmpty(Info.BackgroundName))
		{
			UIHelper.EnableGameObject(BackgroundRoot, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(BackgroundRoot, enable: true);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BackgroundInfos); i++)
			{
				UIHelper.EnableGameObject(BackgroundInfos[i].Root, string.Equals(BackgroundInfos[i].Name, info.BackgroundName));
			}
		}
		if (RenderFacade != null)
		{
			Reset();
		}
		if (Info.RenderFacadeBuilder != null)
		{
			Info.RenderFacadeBuilder.ParentTm = HeroRoot.transform;
			RenderFacade = info.RenderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		}
		if (!(RenderFacade == null))
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(RenderFacade.gameObject, base.gameObject.layer);
			RenderFacade.Action.SetDirection(new VectorXZ(0f, 1f), blendMotion: false);
			RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			Show(show: true);
			UIHelper.EnableGameObject(HeroRoot, enable: true);
			DefaultOrtho = new OrthographicDefault();
			DefaultPerspective = new PerspectiveDefault();
			PreviewCamera = DefaultOrtho;
			SceneAutoBehaviour<GuiManager>.instance.Add("PreviewCameraSetting", this, DrawToggleGui);
		}
	}

	private void Reset()
	{
		if (!(RenderFacade == null))
		{
			RenderFacadeBuilder.ReleaseRenderFacade(RenderFacade);
			RenderFacade = null;
		}
	}

	internal void Close()
	{
		SingletonAuto<LayerManager>.instance.ReleaseLayer(_layerLock);
		_layerLock = -1;
		if (RenderTarget != null)
		{
			MainCamera.targetTexture = null;
			VespaObject.DestroyObject(RenderTarget);
			RenderTarget = null;
		}
		Reset();
	}

	private void Update()
	{
		if (RenderFacade != null)
		{
			RenderFacade.ChainUpdate();
		}
	}

	private void LateUpdate()
	{
		if (SprkGraphicsOptions.ShouldUpdatePreview(this) && PreviewCamera != null)
		{
			PreviewCamera.UpdateCamera();
		}
	}

	public void Show(bool show)
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(show);
		}
	}

	public void DrawToggleGui()
	{
		if (_postProcessVolume == null)
		{
			GUILayout.Label("Post Porcessing does not exist.");
			return;
		}
		_postProcessVolume.enabled = GUILayout.Toggle(_postProcessVolume.enabled, "Enable post processing");
		if (!_postProcessVolume.enabled)
		{
			return;
		}
		foreach (PostProcessEffectSettings setting in _postProcessVolume.profile.settings)
		{
			setting.enabled.value = GUILayout.Toggle(setting.enabled, setting.name);
			if (!setting.enabled)
			{
				continue;
			}
			ColorGrading colorGrading = setting as ColorGrading;
			if (!colorGrading)
			{
				continue;
			}
			DrawFloatParameterGui(colorGrading.temperature, "Temperature", -100f, 100f);
			DrawFloatParameterGui(colorGrading.tint, "Tint", -100f, 100f);
			using (new BlockManager().Horizontal())
			{
				GUILayout.Space(20f);
				colorGrading.colorFilter.overrideState = GUILayout.Toggle(colorGrading.colorFilter.overrideState, "ColorFilter");
			}
			if (colorGrading.colorFilter.overrideState)
			{
				using (new BlockManager().Horizontal())
				{
					GUILayout.Space(40f);
					GUILayout.Label("Color R", GUILayout.Width(150f));
					colorGrading.colorFilter.value.r = GUILayout.HorizontalSlider(colorGrading.colorFilter.value.r, 0f, 2f);
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Space(40f);
					GUILayout.Label("Color G", GUILayout.Width(150f));
					colorGrading.colorFilter.value.g = GUILayout.HorizontalSlider(colorGrading.colorFilter.value.g, 0f, 2f);
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Space(40f);
					GUILayout.Label("Color B", GUILayout.Width(150f));
					colorGrading.colorFilter.value.b = GUILayout.HorizontalSlider(colorGrading.colorFilter.value.b, 0f, 2f);
				}
			}
			DrawFloatParameterGui(colorGrading.hueShift, "HueShift", -180f, 180f);
			DrawFloatParameterGui(colorGrading.saturation, "Saturation", -100f, 100f);
			DrawFloatParameterGui(colorGrading.brightness, "Brightness", -100f, 100f);
			DrawFloatParameterGui(colorGrading.contrast, "Contrast", -100f, 100f);
		}
	}

	public void DrawFloatParameterGui(FloatParameter parameter, string name, float min, float max)
	{
		using (new BlockManager().Horizontal())
		{
			GUILayout.Space(20f);
			parameter.overrideState = GUILayout.Toggle(parameter.overrideState, name, GUILayout.Width(150f));
			if (parameter.overrideState)
			{
				parameter.value = GUILayout.HorizontalSlider(parameter.value, min, max);
			}
		}
	}
}
