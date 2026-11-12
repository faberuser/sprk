using System;
using System.IO;
using NGameAsset.NPatch;
using NVespa;
using UnityEngine;

namespace NGameAsset.NAssetTexture;

public class TextureFromPlainAsset : AssetTexture.ILoad
{
	private string _assetPath = string.Empty;

	private IPlainAssetLoad2 _assetLoad;

	private AssetTexture.TextureInfo _texInfo = new AssetTexture.TextureInfo();

	private Action _onComplete;

	AssetTexture.TextureInfo AssetTexture.ILoad.Value => _texInfo;

	private static string GetAssetPath(string path)
	{
		string rhs = String_Extension_Method.CombinePath("Texture", path);
		string text = PatchDownloader.GetDownloadedAssetPath(UnityCache.persistentDataPath, String_Extension_Method.CombinePath("PlainAsset", rhs));
		if (!IsValidExtension(Path.GetExtension(text)))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
			text = String_Extension_Method.CombinePath(Path.GetDirectoryName(text), fileNameWithoutExtension + ".png");
		}
		return text;
	}

	public void Open(string path, Action onComplete)
	{
		string assetPath = GetAssetPath(path);
		OpenInternal(AssetTexture.ESrcType.PlainAsset, assetPath, onComplete);
	}

	public void OpenUrl(string assetPath, Action onComplete)
	{
		if (!string.IsNullOrEmpty(assetPath))
		{
			assetPath = assetPath.Trim();
		}
		string cachedPath = SingletonAuto<WebTextureCache>.instance.GetCachedPath(assetPath);
		if (String_Extension_Method.IsNullOrEmpty(cachedPath))
		{
			OpenInternal(AssetTexture.ESrcType.Url, assetPath, delegate
			{
				WWW wWW = _assetLoad.WWW;
				if (wWW != null && __Extension_Method_ICollection__.COUNT(wWW.bytes) > 0)
				{
					byte[] bytes = wWW.bytes;
					if (String_Extension_Method.IsNullOrEmpty(SingletonAuto<WebTextureCache>.instance.GetCachedPath(assetPath)))
					{
						SingletonAuto<WebTextureCache>.instance.AddCache(assetPath, bytes);
					}
				}
				Vespa_System_Action_Extension_Method.fire(onComplete);
			});
		}
		else
		{
			OpenInternal(AssetTexture.ESrcType.PlainAsset, cachedPath, onComplete);
		}
	}

	private void OpenInternal(AssetTexture.ESrcType srcType, string assetPath, Action onComplete)
	{
		_assetPath = assetPath;
		_assetLoad = SingletonUpdater<PlainAssetManager>.instance.LockAsset(assetPath, reload: false);
		_onComplete = onComplete;
		_texInfo.SrcType = srcType;
		if (_assetLoad.ISDONE())
		{
			OnCompleteAsset();
			return;
		}
		AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
		singleEntry.AsyncLoad = _assetLoad;
		singleEntry.OnComplete = OnCompleteAsset;
		SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
	}

	private void OnCompleteAsset()
	{
		if (_assetLoad != null)
		{
			_ = _assetLoad.IsValid;
		}
		if (_assetLoad == null)
		{
			_texInfo.Tex = null;
			_texInfo.Width = 4;
			_texInfo.Height = 4;
		}
		else
		{
			_texInfo.Tex = _assetLoad.Texture;
			if (_texInfo.Tex == null)
			{
				_texInfo.Width = 4;
				_texInfo.Height = 4;
			}
			else
			{
				_texInfo.Width = _texInfo.Tex.width;
				_texInfo.Height = _texInfo.Tex.height;
			}
		}
		_texInfo.UvRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
	}

	public static bool IsValidExtension(string extension)
	{
		string[] array = new string[4] { ".png", ".jpg", ".bmp", ".tga" };
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i], extension, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	void AssetTexture.ILoad.Close()
	{
		_texInfo.Tex = null;
		_onComplete = null;
		if (_assetLoad != null && SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			PlainAssetManager.ReleaseAsset(_assetLoad, destroyImmediately: false);
			_assetLoad = null;
		}
	}

	void AssetTexture.ILoad.CloseAndRelease()
	{
		_texInfo.Tex = null;
		_onComplete = null;
		if (_assetLoad != null && SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			PlainAssetManager.ReleaseAsset(_assetLoad, destroyImmediately: true);
			_assetLoad = null;
		}
	}
}
