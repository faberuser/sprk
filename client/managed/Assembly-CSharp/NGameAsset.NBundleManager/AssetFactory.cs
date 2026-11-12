using System.Collections.Generic;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class AssetFactory<TAsset> : IAssetFactory<TAsset> where TAsset : Object
{
	private IBundleAssetLoad<TAsset> _assetLoad;

	private Queue<TAsset> _unusedQueue;

	private bool _assetIsGameObject;

	private GameObject _assetGo;

	internal AssetFactory(IBundleAssetLoad<TAsset> assetLoad)
	{
		_assetLoad = assetLoad;
	}

	public void Close()
	{
		DestroyUnused();
		_assetGo = null;
		_assetLoad = null;
	}

	public void DestroyUnused()
	{
		if (_unusedQueue != null)
		{
			while (_unusedQueue.Count > 0)
			{
				VespaObject.DestroyObject(_unusedQueue.Dequeue());
			}
			_unusedQueue = null;
		}
	}

	TAsset IAssetFactory<TAsset>.Create()
	{
		TAsset unused = GetUnused();
		if (unused != null)
		{
			return unused;
		}
		if (!_assetLoad.ISVALID())
		{
			return null;
		}
		TAsset val = Object.Instantiate(_assetLoad.Asset);
		unused = val;
		if (unused == null)
		{
			VespaObject.DestroyObject(val);
			return null;
		}
		return unused;
	}

	private TAsset GetUnused()
	{
		if (!Define.ReuseAssetInstance.Value)
		{
			return null;
		}
		if (_unusedQueue == null)
		{
			return null;
		}
		TAsset val = null;
		do
		{
			if (_unusedQueue.Count == 0)
			{
				return null;
			}
			val = _unusedQueue.Dequeue();
		}
		while (!(val != null));
		if (!_assetIsGameObject)
		{
			return val;
		}
		GameObject gameObject = val as GameObject;
		gameObject.SetActive(value: true);
		if (gameObject.transform != null)
		{
			gameObject.transform.parent = null;
			if (_assetGo != null && _assetGo.transform != null)
			{
				gameObject.transform.localPosition = _assetGo.transform.localPosition;
				gameObject.transform.localRotation = _assetGo.transform.localRotation;
				gameObject.transform.localScale = _assetGo.transform.localScale;
			}
		}
		return val;
	}

	void IAssetFactory<TAsset>.Release(TAsset assetInstance)
	{
		if (!Define.ReuseAssetInstance.Value)
		{
			VespaObject.DestroyObject(assetInstance);
			return;
		}
		if (_unusedQueue == null)
		{
			_unusedQueue = new Queue<TAsset>();
			_assetGo = _assetLoad.Asset as GameObject;
			_assetIsGameObject = _assetGo != null;
		}
		if (_assetIsGameObject)
		{
			GameObject obj = assetInstance as GameObject;
			obj.transform.parent = AssetUpdateManager.DummyRootTm;
			obj.SetActive(value: false);
		}
		_unusedQueue.Enqueue(assetInstance);
	}
}
