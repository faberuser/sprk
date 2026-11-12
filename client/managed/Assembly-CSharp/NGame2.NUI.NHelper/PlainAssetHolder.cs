using System;
using NGameAsset;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NHelper;

public class PlainAssetHolder : MonoBehaviour, LinkFactoryComponent.IReleaseEvent
{
	private UITexture _uiTex;

	private AssetTexture _assetTex;

	private UIHelper.EAssetTextureAssign _assetTexAssign;

	private Action<AssetTexture.TextureInfo> _onAssign;

	private AsyncManager.SingleEntry _asyncEntry;

	private LinkFactoryComponent _factoryLink;

	public bool ReleaseAssetOnClose { get; set; }

	public void Open(Component componentRoot, UITexture uiTex, string assetPath, UIHelper.EAssetTextureAssign assetTexAssign, Action<AssetTexture.TextureInfo> onAssign)
	{
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(assetPath);
		Open(componentRoot, uiTex, assetTexture, assetTexAssign, onAssign);
	}

	public void Open(Component componentRoot, UITexture uiTex, AssetTexture assetTex, UIHelper.EAssetTextureAssign assetTexAssign, Action<AssetTexture.TextureInfo> onAssign)
	{
		Close();
		AddToFactoryLink(componentRoot);
		_uiTex = uiTex;
		_assetTex = assetTex;
		_assetTexAssign = assetTexAssign;
		_onAssign = onAssign;
		if (_assetTex.ISDONE())
		{
			OnComplete();
			return;
		}
		_asyncEntry = new AsyncManager.SingleEntry();
		_asyncEntry.AsyncLoad = _assetTex;
		_asyncEntry.OnComplete = OnComplete;
		SingletonUpdater<AsyncManager>.instance.Add(_asyncEntry);
	}

	private void AddToFactoryLink(Component componentRoot)
	{
		if (!(_factoryLink != null))
		{
			_factoryLink = GetFactoryLink(componentRoot);
			if (_factoryLink != null)
			{
				_factoryLink.AddReleaseEvent(this);
			}
		}
	}

	private LinkFactoryComponent GetFactoryLink(Component componentRoot)
	{
		if (componentRoot != null)
		{
			LinkFactoryComponent linkFactoryComponent = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(componentRoot.transform);
			if (linkFactoryComponent != null)
			{
				return linkFactoryComponent;
			}
		}
		Transform parent = base.transform;
		while (parent != null)
		{
			LinkFactoryComponent linkFactoryComponent2 = SingletonAuto<LinkedTransform<LinkFactoryComponent>>.instance.Get(parent);
			if (linkFactoryComponent2 != null)
			{
				return linkFactoryComponent2;
			}
			parent = parent.parent;
		}
		return null;
	}

	public void Close()
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
			_uiTex = null;
		}
		if (_assetTex != null)
		{
			if (ReleaseAssetOnClose)
			{
				_assetTex.CloseAndRelease();
			}
			else
			{
				_assetTex.Close();
			}
			_assetTex = null;
		}
		ReleaseAssetOnClose = false;
		_onAssign = null;
		_factoryLink = null;
	}

	private void OnComplete()
	{
		_asyncEntry = null;
		AssetTexture.TextureInfo textureInfo = null;
		if (_assetTex != null)
		{
			textureInfo = _assetTex.GetTextureInfo();
		}
		UIHelper.SetTextureInfo(_uiTex, textureInfo, _assetTexAssign);
		if (textureInfo != null && textureInfo.Tex != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onAssign, textureInfo);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(_onAssign, null);
		}
		_onAssign = null;
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			Close();
		}
	}

	void LinkFactoryComponent.IReleaseEvent.OnRelease()
	{
		Close();
	}
}
