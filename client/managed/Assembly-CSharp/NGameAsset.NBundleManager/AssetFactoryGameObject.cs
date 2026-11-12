using System.Collections.Generic;
using UnityEngine;

namespace NGameAsset.NBundleManager;

public class AssetFactoryGameObject : IAssetFactory<GameObject>
{
	private IBundleAssetLoad<GameObject> _assetLoad;

	private Queue<GameObject> _unusedQueue;

	private GameObject _assetGo;

	internal AssetFactoryGameObject(IBundleAssetLoad<GameObject> assetLoad)
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
				VespaObject.PendingDestroyGo(_unusedQueue.Dequeue());
			}
			_unusedQueue = null;
		}
	}

	GameObject IAssetFactory<GameObject>.Create()
	{
		GameObject unused = GetUnused();
		if (unused != null)
		{
			return unused;
		}
		if (!_assetLoad.ISVALID())
		{
			return null;
		}
		unused = Object.Instantiate(_assetLoad.Asset);
		if (unused == null)
		{
			return null;
		}
		return unused;
	}

	private GameObject GetUnused()
	{
		if (!Define.ReuseAssetInstance.Value)
		{
			return null;
		}
		if (_unusedQueue == null)
		{
			return null;
		}
		GameObject gameObject = null;
		do
		{
			if (_unusedQueue.Count == 0)
			{
				return null;
			}
			gameObject = _unusedQueue.Dequeue();
		}
		while (!(gameObject != null));
		gameObject.SetActive(value: true);
		gameObject.transform.parent = null;
		gameObject.transform.localPosition = _assetGo.transform.localPosition;
		gameObject.transform.localRotation = _assetGo.transform.localRotation;
		gameObject.transform.localScale = _assetGo.transform.localScale;
		return gameObject;
	}

	void IAssetFactory<GameObject>.Release(GameObject assetInstance)
	{
		if (!Define.ReuseAssetInstance.Value)
		{
			VespaObject.PendingDestroyGo(assetInstance);
			return;
		}
		if (_unusedQueue == null)
		{
			_unusedQueue = new Queue<GameObject>();
			_assetGo = _assetLoad.Asset;
		}
		assetInstance.SetActive(value: false);
		assetInstance.transform.parent = AssetUpdateManager.DummyRootTm;
		_unusedQueue.Enqueue(assetInstance);
	}
}
