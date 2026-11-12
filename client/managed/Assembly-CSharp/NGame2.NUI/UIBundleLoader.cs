using NGameAsset;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

[AddComponentMenu("NGUI/UI/NGUI Bundle Loader (Extension)")]
[ExecuteInEditMode]
public class UIBundleLoader : MonoBehaviour, IInspectorGuiDrawer
{
	[SerializeField]
	private UITexture _uiTex;

	[SerializeField]
	private bool _releaseAssetOnClose;

	[SerializeField]
	private bool _showInEditMode;

	[SerializeField]
	private UIHelper.EAssetTextureAssign _assetTexAssign;

	[HideInInspector]
	[SerializeField]
	private string _assetPath = string.Empty;

	private UIHelper.EAssetTextureAssign _runtimeAssign = UIHelper.EAssetTextureAssign.__Ignore__;

	private AssetTexture _assetTexPreload;

	private AssetTexture _assetTex;

	private AsyncManager.SingleEntry _asyncEntry;

	private UIHelper.EAssetTextureAssign _currentAssign
	{
		get
		{
			if (_runtimeAssign == UIHelper.EAssetTextureAssign.__Ignore__)
			{
				return _assetTexAssign;
			}
			return _runtimeAssign;
		}
	}

	public string AssetPath
	{
		get
		{
			return _assetPath;
		}
		set
		{
			SprkImageRecovery.BeforePortraitPathChange(this, value);
			if (_assetPath != value)
			{
				_assetPath = value;
				RefreshAssetTexture();
			}
		}
	}

	public UITexture UiTex => _uiTex;

	public void SetAssetPath(string path, UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.__Ignore__)
	{
		SprkImageRecovery.BeforePortraitChange(this, path, assetTexAssign);
		UIHelper.EAssetTextureAssign currentAssign = _currentAssign;
		string assetPath = _assetPath;
		_assetPath = path;
		_runtimeAssign = assetTexAssign;
		if (assetPath != _assetPath || currentAssign != _currentAssign)
		{
			RefreshAssetTexture();
		}
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		LoadAssetTexture();
	}

	private void OnDisable()
	{
		SprkImageRecovery.Reset(this);
		ReleaseAssetTexture();
	}

	private void OnDestroy()
	{
		SprkImageRecovery.Reset(this);
		ReleaseAssetTexture();
	}

	private void RefreshAssetTexture()
	{
		ReleaseAssetTexture();
		LoadAssetTexture();
	}

	private void ReleaseAssetTexture()
	{
		if (_asyncEntry != null)
		{
			_asyncEntry.OnComplete = null;
			_asyncEntry = null;
		}
		if (_uiTex != null)
		{
			_uiTex.material = null;
			_uiTex.mainTexture = null;
		}
		if (_assetTex != null)
		{
			if (_releaseAssetOnClose)
			{
				_assetTex.CloseAndRelease();
			}
			else
			{
				_assetTex.Close();
			}
			_assetTex = null;
		}
		ReleaseAssetTexPreload();
	}

	private void LoadAssetTexture()
	{
		if (SprkImageRecovery.TryTpIcon(this) || String_Extension_Method.IsNullOrEmpty(_assetPath) || _assetTex != null)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			LoadAssetTexPreload();
			return;
		}
		_assetTex = new AssetTexture();
		_assetTex.Open(_assetPath);
		if (_assetTex.ISDONE())
		{
			OnComplete();
		}
		else
		{
			_asyncEntry = new AsyncManager.SingleEntry();
			_asyncEntry.AsyncLoad = _assetTex;
			_asyncEntry.OnComplete = OnComplete;
			SingletonUpdater<AsyncManager>.instance.Add(_asyncEntry);
		}
		ReleaseAssetTexPreload();
	}

	private void OnComplete()
	{
		_asyncEntry = null;
		AssetTexture.TextureInfo textureInfo = null;
		if (_assetTex != null)
		{
			textureInfo = _assetTex.GetTextureInfo();
		}
		UIHelper.SetTextureInfo(_uiTex, textureInfo, _currentAssign);
		SprkImageRecovery.FitHeroPortrait(this, textureInfo);
	}

	private void LoadAssetTexPreload()
	{
		if (_assetTexPreload == null)
		{
			_assetTexPreload = new AssetTexture();
			_assetTexPreload.Open(_assetPath);
			SingletonAuto<AsyncSceneContainer>.instance.Add(_assetTexPreload);
		}
	}

	private void ReleaseAssetTexPreload()
	{
		if (_assetTexPreload != null)
		{
			_assetTexPreload.Close();
			_assetTexPreload = null;
		}
	}

	public void SetUITexture(UITexture uiTexture)
	{
		_uiTex = uiTexture;
	}

	public void SetShowInEditorMode(bool showEditorMode)
	{
		_showInEditMode = showEditorMode;
	}

	private void Update()
	{
		SprkImageRecovery.Tick(this);
	}

	private void LateUpdate()
	{
		SprkImageRecovery.TickPortrait(this);
	}
}
