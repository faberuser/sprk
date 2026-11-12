using System;
using System.Collections.Generic;
using NGameAsset.NAsyncLoad;
using NVespa;

namespace NGameAsset;

public class PlainAssetManager : SingletonUpdater<PlainAssetManager>
{
	public const int MaxLoadingCount = 99;

	private IncreaseDic<string, IPlainAssetLoad2> _dic = new IncreaseDic<string, IPlainAssetLoad2>(StringComparer.OrdinalIgnoreCase);

	private List<IPlainAssetLoad2> _removedList = new List<IPlainAssetLoad2>();

	private List<IPlainAssetLoad2> _loadingList = new List<IPlainAssetLoad2>();

	private Queue<IPlainAssetLoad2> _pendingQueue = new Queue<IPlainAssetLoad2>();

	public int LoadingCount => __Extension_Method_ICollection__.COUNT(_loadingList);

	public IPlainAssetLoad2 LockAsset(string assetPath, bool reload)
	{
		IPlainAssetLoad2 plainAssetLoad = _dic.GETVALUE(assetPath, null);
		if (reload && plainAssetLoad != null)
		{
			_removedList.Add(plainAssetLoad);
			_dic.RemoveSlow(assetPath);
			plainAssetLoad = null;
		}
		if (plainAssetLoad == null)
		{
			plainAssetLoad = CreatePlainAssetLoad(assetPath);
			_dic.Add(assetPath, plainAssetLoad);
		}
		plainAssetLoad.Lock.Increase();
		return plainAssetLoad;
	}

	private IPlainAssetLoad2 CreatePlainAssetLoad(string assetPath)
	{
		PlainAssetLoad plainAssetLoad = new PlainAssetLoad();
		plainAssetLoad.Open(assetPath);
		_pendingQueue.Enqueue(plainAssetLoad);
		return plainAssetLoad;
	}

	public static void ReleaseAsset(IPlainAssetLoad2 assetLoad, bool destroyImmediately)
	{
		if (SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			assetLoad.Lock.Decrease(null);
		}
	}

	public void DestroyUnused(bool unusedAll)
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			IPlainAssetLoad2 valueByIndex = _dic.GetValueByIndex(i);
			if (valueByIndex.IsDone && valueByIndex.Lock.Count <= 0)
			{
				valueByIndex.Close();
				_dic.RemoveByIndex(i);
				i--;
			}
		}
		for (int j = 0; j < _removedList.Count; j++)
		{
			IPlainAssetLoad2 plainAssetLoad = _removedList[j];
			if (plainAssetLoad.IsDone && plainAssetLoad.Lock.Count <= 0)
			{
				plainAssetLoad.Close();
				_removedList.RemoveAt(j);
				j--;
			}
		}
	}

	public override void ChainUpdate()
	{
		if (!Define.PatchDownloaded)
		{
			return;
		}
		for (int i = 0; i < _loadingList.Count; i++)
		{
			IPlainAssetLoad2 plainAssetLoad = _loadingList[i];
			if (plainAssetLoad.IsDone)
			{
				_loadingList.RemoveAt(i);
				plainAssetLoad.Lock.Decrease(null);
				i--;
			}
		}
		for (int j = 0; j < 99; j++)
		{
			if (_loadingList.Count >= 99)
			{
				break;
			}
			if (_pendingQueue.Count <= 0)
			{
				break;
			}
			IPlainAssetLoad2 plainAssetLoad2 = _pendingQueue.Dequeue();
			plainAssetLoad2.StartLoad();
			plainAssetLoad2.Lock.Increase();
			_loadingList.Add(plainAssetLoad2);
		}
	}
}
