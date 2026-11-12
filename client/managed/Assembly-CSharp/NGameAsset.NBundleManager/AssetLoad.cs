using NGameAsset.NAsyncLoad;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class AssetLoad<TAsset> : IBundleAssetLoad<TAsset>, IBundleAssetLoadBase, IAsyncLoadBase where TAsset : Object
{
	private static bool s_releaseBundleOnClose;

	public string LoadMessage;

	private BundleAssetKey _key;

	private IBundleLoad _bundleLoad;

	private IRequestWrapper _request;

	private Object _asset;

	private IAssetFactory<TAsset> _assetFactory;

	private bool _isDone;

	private bool _isValid;

	private LockCounter _lockCounter = new LockCounter();

	BundleAssetKey IBundleAssetLoadBase.Key => _key;

	ILockCounter IBundleAssetLoadBase.Lock => _lockCounter;

	TAsset IBundleAssetLoad<TAsset>.Asset => _asset as TAsset;

	IAssetFactory<TAsset> IBundleAssetLoad<TAsset>.Factory
	{
		get
		{
			if (_assetFactory == null)
			{
				_assetFactory = new AssetFactory<TAsset>(this);
			}
			return _assetFactory;
		}
	}

	string IAsyncLoadBase.KeyDesc => $"{_key.BundleFilename}:{_key.AssetName}";

	bool IAsyncLoadBase.IsDone => _isDone;

	bool IAsyncLoadBase.IsValid => _isValid;

	string IAsyncLoadBase.LoadMessage
	{
		get
		{
			if (String_Extension_Method.IsNullOrEmpty(LoadMessage))
			{
				return $"{typeof(TAsset).Name} Asset(AssetName={_key.AssetName}";
			}
			return LoadMessage;
		}
	}

	static AssetLoad()
	{
		s_releaseBundleOnClose = false;
		if (typeof(TAsset) == typeof(AudioClip))
		{
			s_releaseBundleOnClose = true;
		}
	}

	public bool Open(BundleAssetKey key)
	{
		if (!Define.PatchDownloaded)
		{
			VespaDebug.LogError($"Should never load asset before patch download (remove or embed it): {key.AssetName} in {key.BundleFilename}");
			return false;
		}
		_key = key;
		_isDone = false;
		_isValid = false;
		_bundleLoad = SingletonAuto<BundleManager>.instance.LockBundle(_key.BundleFilename);
		if (_bundleLoad == null)
		{
			return false;
		}
		if (_bundleLoad.IsDone)
		{
			OnCompleteBundle();
		}
		else
		{
			AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
			singleEntry.AsyncLoad = _bundleLoad;
			singleEntry.OnComplete = OnCompleteBundle;
			SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
		}
		return true;
	}

	void IBundleAssetLoadBase.DestroyUnusedInFactory()
	{
		if (_assetFactory != null)
		{
			_assetFactory.DestroyUnused();
		}
	}

	void IBundleAssetLoadBase.Close(bool destroyImmediately)
	{
		if (_assetFactory != null)
		{
			_assetFactory.Close();
			_assetFactory = null;
		}
		if (_request != null)
		{
			_request.Close();
			_request = null;
		}
		if (_bundleLoad != null)
		{
			BundleManager.ReleaseBundle(_bundleLoad, destroyImmediately);
			_bundleLoad = null;
		}
	}

	private void OnCompleteBundle()
	{
		if (_bundleLoad == null)
		{
			return;
		}
		if (!_bundleLoad.IsValid)
		{
			SetInvalid($"Invalid BundleLoad: {_key.BundleFilename}:{_key.AssetName}", _bundleLoad);
			return;
		}
		SentrySdk.AddBreadcrumb($"[Load Asset] FileName({_key.BundleFilename}) , AssetName({_key.AssetName})");
		if (SingletonUpdater<AssetCacheService>.instance.GetCachedAsset(_key.BundleFilename, _key.AssetName, out _asset))
		{
			_isDone = true;
			_isValid = true;
			OnCompleteAsset();
			return;
		}
		if (String_Extension_Method.IsNullOrEmpty(_key.AssetName))
		{
			_request = _bundleLoad.LoadMainAsset();
		}
		else
		{
			_request = _bundleLoad.LoadAsync(_key.AssetName, typeof(TAsset));
		}
		if (_request == null)
		{
			SetInvalid($"Invalid BundleLoad.LoadAsync: {_key.BundleFilename}:{_key.AssetName}", _bundleLoad);
			return;
		}
		if (_request.IsDone)
		{
			OnCompleteAsset();
			return;
		}
		AsyncManager.SingleEntry singleEntry = new AsyncManager.SingleEntry();
		singleEntry.AsyncLoad = _request;
		singleEntry.OnComplete = OnCompleteAsset;
		SingletonUpdater<AsyncManager>.instance.Add(singleEntry);
	}

	private void OnCompleteAsset()
	{
		if (_request == null)
		{
			return;
		}
		if (!_request.IsValid)
		{
			SetInvalid($"Invalid Asset Request: {_key.BundleFilename}:{_key.AssetName}", _request);
			return;
		}
		if (!s_releaseBundleOnClose && _bundleLoad != null)
		{
			BundleManager.ReleaseBundle(_bundleLoad, destroyImmediately: false);
			_bundleLoad = null;
		}
		_asset = _request.asset;
		SingletonUpdater<AssetCacheService>.instance.CacheAsset(_key.BundleFilename, _key.AssetName, _asset);
		_isDone = true;
		_isValid = true;
	}

	private void SetInvalid(string message, IAsyncLoadBase childAsyncLoad)
	{
		_isDone = true;
		_isValid = false;
		LoadMessage = message;
		if (childAsyncLoad != null && !String_Extension_Method.IsNullOrEmpty(childAsyncLoad.LoadMessage))
		{
			LoadMessage += $"\n - {childAsyncLoad.LoadMessage}";
		}
	}
}
