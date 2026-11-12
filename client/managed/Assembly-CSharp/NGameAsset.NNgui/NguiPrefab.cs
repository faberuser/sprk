using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NNgui;

public class NguiPrefab : INguiPrefab, IAsyncLoadBase
{
	private AssetDataAsync<GameObject, NguiData> _assetDataAsync;

	string IAsyncLoadBase.KeyDesc => _assetDataAsync.KEYDESC();

	bool IAsyncLoadBase.IsDone => _assetDataAsync.ISDONE();

	bool IAsyncLoadBase.IsValid => _assetDataAsync.ISVALID();

	string IAsyncLoadBase.LoadMessage => _assetDataAsync.LOADMESSAGE();

	public bool Open(NguiData data)
	{
		_assetDataAsync = new AssetDataAsync<GameObject, NguiData>();
		_assetDataAsync.OpenMainAsset(data, data.BundleFileName);
		return true;
	}

	void INguiPrefab.Close()
	{
		if (_assetDataAsync != null)
		{
			_assetDataAsync.Close();
			_assetDataAsync = null;
		}
	}

	NguiInstance INguiPrefab.CreateInstance(Transform parentTm, int layer)
	{
		if (!this.ISDONE() || !this.ISVALID())
		{
			return null;
		}
		NguiInstance nguiInstance = new NguiInstance();
		if (!nguiInstance.Open(_assetDataAsync.Asset, _assetDataAsync.Data, parentTm, layer))
		{
			nguiInstance.Close();
			return null;
		}
		return nguiInstance;
	}
}
