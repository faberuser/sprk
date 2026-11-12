using System;
using NVespa;
using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class EnableAsyncWrapper : IRequestWrapper, IAsyncLoadBase
{
	private bool _isDone;

	private UnityEngine.Object _asset;

	private AssetBundleRequest _request;

	private string _keyDesc = string.Empty;

	private string _loadMessage = string.Empty;

	private const int MAX_CONCURRENT_ASYNC_LOAD = 20;

	private static int _concurrentAsyncLoad;

	private static int _rawAsyncLoadCount;

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

	UnityEngine.Object IRequestWrapper.asset => _asset;

	public EnableAsyncWrapper(AssetBundle bundle, string assetName, Type typeInfo)
	{
		_keyDesc = assetName;
		if (SingletonUpdater<AsyncManager>.instance.ForceSyncLoad)
		{
			_asset = bundle.LoadAsset(assetName, typeInfo);
			_isDone = true;
			return;
		}
		_request = bundle.LoadAssetAsync(assetName, typeInfo);
		SingletonUpdater<AsyncManager>.instance.Add(_request, delegate
		{
			_asset = _request.asset;
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
			bool num = _asset is GameObject;
			bool flag = _asset is Component;
			if (!num)
			{
			}
			_asset = null;
		}
	}
}
