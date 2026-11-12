using NVespa;
using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class MainAssetAsyncWrapper : IRequestWrapper, IAsyncLoadBase
{
	private bool _isDone;

	private Object _asset;

	private AssetBundle _bundle;

	private AssetBundleRequest _request;

	private string _keyDesc = string.Empty;

	private string _loadMessage = string.Empty;

	string IAsyncLoadBase.KeyDesc => _keyDesc;

	bool IAsyncLoadBase.IsDone => _isDone;

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (!_isDone)
			{
				return false;
			}
			return _asset != null;
		}
	}

	string IAsyncLoadBase.LoadMessage => _loadMessage;

	Object IRequestWrapper.asset => _asset;

	public MainAssetAsyncWrapper(AssetBundle bundle)
	{
		_keyDesc = "mainAsset";
		_bundle = bundle;
		if (SingletonUpdater<AsyncManager>.instance.ForceSyncLoad)
		{
			_bundle.LoadAllAssets();
			_asset = _bundle.mainAsset;
			_isDone = true;
			return;
		}
		_request = bundle.LoadAllAssetsAsync();
		SingletonUpdater<AsyncManager>.instance.Add(_request, delegate
		{
			_asset = _bundle.mainAsset;
			_bundle = null;
			if (_asset == null)
			{
				_loadMessage = "Asset In Bundle Load Fail: " + _keyDesc;
			}
			_request = null;
			_isDone = true;
		});
	}

	void IRequestWrapper.Close()
	{
		_request = null;
		if (_asset != null)
		{
			_asset = null;
		}
	}
}
