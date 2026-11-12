using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class AssetLoad<TAsset, TData> : IAsyncLoadBase where TAsset : Object where TData : class
{
	public string LoadMessage;

	private string _assetName;

	private IRequestWrapper _request;

	public TData Data { get; private set; }

	public TAsset Asset
	{
		get
		{
			if (_request == null)
			{
				return null;
			}
			return _request.asset as TAsset;
		}
	}

	string IAsyncLoadBase.KeyDesc => _assetName;

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (_request == null)
			{
				return true;
			}
			return _request.IsDone;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (_request == null)
			{
				return false;
			}
			if (!_request.IsDone)
			{
				return false;
			}
			return _request.IsValid;
		}
	}

	string IAsyncLoadBase.LoadMessage
	{
		get
		{
			if (String_Extension_Method.IsNullOrEmpty(LoadMessage))
			{
				return $"{typeof(TAsset).Name} Asset(AssetName={_assetName}";
			}
			return LoadMessage;
		}
	}

	public bool Open(TData data, IBundleLoad bundleLoad, string assetName)
	{
		Data = data;
		_assetName = assetName;
		_request = bundleLoad.LoadAsync(assetName, typeof(TAsset));
		if (_request == null)
		{
			return false;
		}
		return true;
	}
}
