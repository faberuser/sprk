using NVespa;
using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class AssetDataAsync<TAsset, TData> : IAsyncLoad<TAsset>, IAsyncLoadBase where TAsset : Object where TData : class
{
	private IBundleAssetLoad<TAsset> _assetLoad;

	public TData Data { get; private set; }

	public TAsset Asset => _assetLoad.Asset;

	public IAssetFactory<TAsset> Factory => _assetLoad.Factory;

	TAsset IAsyncLoad<TAsset>.Value => Asset;

	string IAsyncLoadBase.KeyDesc
	{
		get
		{
			if (_assetLoad != null)
			{
				return _assetLoad.KeyDesc;
			}
			return string.Empty;
		}
	}

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (_assetLoad != null)
			{
				return _assetLoad.IsDone;
			}
			return true;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (_assetLoad != null)
			{
				return _assetLoad.IsValid;
			}
			return false;
		}
	}

	string IAsyncLoadBase.LoadMessage
	{
		get
		{
			if (_assetLoad != null)
			{
				return _assetLoad.LoadMessage;
			}
			return string.Empty;
		}
	}

	public void Open(TData data, string bundleFileName, string assetName)
	{
		Data = data;
		_assetLoad = SingletonAuto<BundleAssetManager>.instance.LockAsset<TAsset>(bundleFileName, assetName);
	}

	public void OpenMainAsset(TData data, string bundleFileName)
	{
		Data = data;
		_assetLoad = SingletonAuto<BundleAssetManager>.instance.LockMainAsset<TAsset>(bundleFileName);
	}

	public void Close(bool destroyImmediately = false)
	{
		if (_assetLoad != null)
		{
			BundleAssetManager.ReleaseAsset(_assetLoad, destroyImmediately, null);
			_assetLoad = null;
		}
	}
}
