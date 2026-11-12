using System;
using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class DisableAsyncWrapper : IRequestWrapper, IAsyncLoadBase
{
	private UnityEngine.Object _asset;

	string IAsyncLoadBase.KeyDesc => Vespa_Common_Class_Extension_Method.TOSTRING(_asset);

	bool IAsyncLoadBase.IsDone => true;

	bool IAsyncLoadBase.IsValid => _asset != null;

	string IAsyncLoadBase.LoadMessage => Vespa_Common_Class_Extension_Method.TOSTRING(_asset);

	UnityEngine.Object IRequestWrapper.asset => _asset;

	public DisableAsyncWrapper(AssetBundle bundle, string assetName, Type typeInfo)
	{
		_asset = bundle.LoadAsset(assetName, typeInfo);
	}

	void IRequestWrapper.Close()
	{
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
