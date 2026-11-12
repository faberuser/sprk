using System;
using NGameAsset.NNguiAtlas;
using NVespa;
using UnityEngine;

namespace NGameAsset.NAssetTexture;

public class TextureFromNguiAtlas : AssetTexture.ILoad
{
	private AtlasDataContainer.AtlasLink _atlasLink;

	private INguiAtlas _nguiAtlas;

	private Action _onComplete;

	private AssetTexture.TextureInfo _texInfo = new AssetTexture.TextureInfo();

	AssetTexture.TextureInfo AssetTexture.ILoad.Value => _texInfo;

	public void Open(AtlasDataContainer.AtlasLink atlasLink, Action onComplete)
	{
		_atlasLink = atlasLink;
		_nguiAtlas = SingletonAuto<NguiAtlasContainer>.instance.Lock(atlasLink.AtlasCode);
		_onComplete = onComplete;
		_texInfo.SrcType = AssetTexture.ESrcType.NguiAtlas;
		if (_nguiAtlas.ISDONE())
		{
			OnCompleteAsset();
			return;
		}
		AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
		singleEntry.AsyncLoad = _nguiAtlas;
		singleEntry.OnComplete = OnCompleteAsset;
		SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
	}

	private void OnCompleteAsset()
	{
		if (_nguiAtlas == null)
		{
			return;
		}
		if (!_nguiAtlas.IsValid)
		{
			CompleteInvalid();
			return;
		}
		UISpriteData sprite = _nguiAtlas.Value.GetSprite(_atlasLink.SpriteName);
		if (sprite == null)
		{
			CompleteInvalid();
			return;
		}
		Rect rect = new Rect(sprite.X, sprite.Y, sprite.Width, sprite.Height);
		rect = NGUIMath.ConvertToTexCoords(rect, _nguiAtlas.GetAtlasWidth(), _nguiAtlas.GetAtlasHeight());
		_texInfo.Tex = _nguiAtlas.Value.texture as Texture2D;
		if (_nguiAtlas.HasAlphaTexture)
		{
			_texInfo.Mtrl = _nguiAtlas.Value.spriteMaterial;
		}
		_texInfo.Width = sprite.Width;
		_texInfo.Height = sprite.Height;
		_texInfo.UvRect = rect;
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
	}

	private void CompleteInvalid()
	{
		_texInfo.Tex = null;
		_texInfo.Mtrl = null;
		_texInfo.Width = 4;
		_texInfo.Height = 4;
		_texInfo.UvRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
	}

	void AssetTexture.ILoad.Close()
	{
		_texInfo.Tex = null;
		_texInfo.Mtrl = null;
		_onComplete = null;
		if (_nguiAtlas != null)
		{
			NguiAtlasContainer.ReleaseAtlas(_nguiAtlas, destroyImmediately: false);
			_nguiAtlas = null;
		}
	}

	void AssetTexture.ILoad.CloseAndRelease()
	{
		_texInfo.Tex = null;
		_texInfo.Mtrl = null;
		_onComplete = null;
		if (_nguiAtlas != null)
		{
			NguiAtlasContainer.ReleaseAtlas(_nguiAtlas, destroyImmediately: true);
			_nguiAtlas = null;
		}
	}
}
