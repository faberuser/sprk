using NGame2.NUI.NRender3D.NPreviewCamera;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NCamera;
using NVespa.NPrefab2;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NUI.NRender3D;

public class ItemPreview : LinkFactoryComponent
{
	public struct InfoData
	{
		public int RenderTargetWidth;

		public int RenderTargetHeight;

		public string Code;

		public ItemType ItemType;

		public TransformData extraTransform;

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
			return true;
		}
	}

	private struct CameraInfo
	{
		public float OrthoSize;

		public Vector3 Pos;

		public Vector3 Dir;
	}

	public const string PrefabPath = "Prefab2/Item3DPreview/Item3DPreview";

	public static PrefabAuto<ItemPreview> PrefabAuto = new PrefabAuto<ItemPreview>("Prefab2/Item3DPreview/Item3DPreview");

	public static Transform _previewRoot;

	public GameObject ItemRoot;

	public Camera MainCamera;

	private IAsyncLoad<IEffect> _effectAsync;

	private int _layerLock = -1;

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

	public OrthographicDefault DefaultOrtho { get; private set; }

	public PerspectiveDefault DefaultPerspective { get; private set; }

	public static void OnSceneChanged()
	{
		if (Vespa_Common_Class_Extension_Method.IsUnityNull(_previewRoot))
		{
			_previewRoot = null;
		}
	}

	public static ItemPreview CreateInstance(InfoData info)
	{
		ItemPreview itemPreview = PrefabAuto.Factory.Create(PreviewRoot, -1);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemPreview.transform);
		itemPreview.Open(info);
		return itemPreview;
	}

	public static void ReleaseInstance(ItemPreview itemPreview)
	{
		if (!(itemPreview == null))
		{
			itemPreview.Close();
			FactoryBase.ReleaseComponent(itemPreview);
		}
	}

	internal void Open(InfoData info)
	{
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
			PostProcessVolume componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<PostProcessVolume>(MainCamera.gameObject);
			componentAuto.profile = VespaResource.Load<PostProcessProfile>("Effect/PostProcessPreview");
			componentAuto.isGlobal = true;
			componentAuto.enabled = SystemOptionData.Render_EnableRimLight;
		}
		Show(show: true);
		UIHelper.EnableGameObject(ItemRoot, enable: true);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(ItemRoot.transform);
		MainCamera.orthographic = true;
		MainCamera.nearClipPlane = -50f;
		MainCamera.farClipPlane = 50f;
		OpenPreviewItem(info);
	}

	private void OpenPreviewItem(InfoData info)
	{
		_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(info.Code, base.gameObject.layer, info.extraTransform);
		_effectAsync.Value.SetParent(ItemRoot.transform);
	}

	internal void Close()
	{
		EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, immediatelyRelease: true);
		SingletonAuto<LayerManager>.instance.ReleaseLayer(_layerLock);
		_layerLock = -1;
		if (MainCamera != null)
		{
			MainCamera.targetTexture = null;
		}
		if (RenderTarget != null)
		{
			RenderTarget.Release();
			VespaObject.DestroyObject(RenderTarget);
			RenderTarget = null;
		}
	}

	public void Show(bool show)
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(show);
		}
	}

	private void Update()
	{
		if (_effectAsync != null && _effectAsync.Value != null)
		{
			_effectAsync.Value.Update();
		}
	}
}
