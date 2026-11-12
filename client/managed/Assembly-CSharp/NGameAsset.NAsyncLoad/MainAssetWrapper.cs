using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class MainAssetWrapper : IRequestWrapper, IAsyncLoadBase
{
	private Object _asset;

	string IAsyncLoadBase.KeyDesc => Vespa_Common_Class_Extension_Method.TOSTRING(_asset);

	bool IAsyncLoadBase.IsDone => true;

	bool IAsyncLoadBase.IsValid => _asset != null;

	string IAsyncLoadBase.LoadMessage => Vespa_Common_Class_Extension_Method.TOSTRING(_asset);

	Object IRequestWrapper.asset => _asset;

	public static IRequestWrapper LoadMainAsset(AssetBundle bundle)
	{
		return new MainAssetAsyncWrapper(bundle);
	}

	public MainAssetWrapper(AssetBundle bundle)
	{
		_asset = bundle.mainAsset;
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
