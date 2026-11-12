using System.Collections.Generic;
using System.Text;
using NGameAsset.NBundleManager;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class BundleAssetManager : SingletonAuto<BundleAssetManager>
{
	private class InvalidAssetLoad<TAsset> where TAsset : Object
	{
		public static InvalidBundleAssetLoad<TAsset> InvalidLoad = new InvalidBundleAssetLoad<TAsset>(string.Empty, "Invalid Load");
	}

	private Dictionary<BundleAssetKey, IBundleAssetLoadBase> _dic = new Dictionary<BundleAssetKey, IBundleAssetLoadBase>(new BundleAssetKey.EqualityComparer());

	private List<IBundleAssetLoadBase> _releaseList = new List<IBundleAssetLoadBase>();

	public int Count => _dic.Count;

	public IBundleAssetLoad<TAsset> LockAsset<TAsset>(string bundleFileName, string assetName) where TAsset : Object
	{
		if (String_Extension_Method.IsNullOrEmpty(bundleFileName) || String_Extension_Method.IsNullOrEmpty(assetName))
		{
			return InvalidAssetLoad<TAsset>.InvalidLoad;
		}
		return LockAssetInternal<TAsset>(bundleFileName, assetName);
	}

	public IBundleAssetLoad<TAsset> LockMainAsset<TAsset>(string bundleFileName) where TAsset : Object
	{
		if (String_Extension_Method.IsNullOrEmpty(bundleFileName))
		{
			return InvalidAssetLoad<TAsset>.InvalidLoad;
		}
		return LockAssetInternal<TAsset>(bundleFileName, string.Empty);
	}

	private IBundleAssetLoad<TAsset> LockAssetInternal<TAsset>(string bundleFileName, string assetName) where TAsset : Object
	{
		lock (_dic)
		{
			BundleAssetKey bundleAssetKey = new BundleAssetKey(bundleFileName, assetName, typeof(TAsset));
			IBundleAssetLoad<TAsset> bundleAssetLoad = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, bundleAssetKey, null) as IBundleAssetLoad<TAsset>;
			if (bundleAssetLoad == null)
			{
				AssetLoad<TAsset> assetLoad = new AssetLoad<TAsset>();
				if (!assetLoad.Open(bundleAssetKey))
				{
					Vespa_Common_Class_Extension_Method.Interface((IBundleAssetLoadBase)assetLoad).Close(destroyImmediately: false);
					return InvalidAssetLoad<TAsset>.InvalidLoad;
				}
				bundleAssetLoad = assetLoad;
				_dic.Add(bundleAssetKey, bundleAssetLoad);
			}
			bundleAssetLoad.Lock.Increase();
			return bundleAssetLoad;
		}
	}

	public static void ReleaseAsset(IBundleAssetLoadBase assetLoad, bool destroyImmediately, CallData lockCall)
	{
		if (SingletonAuto<BundleAssetManager>.isValidInstance)
		{
			SingletonAuto<BundleAssetManager>.instance.ReleaseBuilderInternal(assetLoad, destroyImmediately, lockCall);
		}
	}

	private void ReleaseBuilderInternal(IBundleAssetLoadBase assetLoad, bool destroyImmediately, CallData lockCall)
	{
		if (assetLoad == null)
		{
			return;
		}
		assetLoad.Lock.Decrease(lockCall);
		if (assetLoad.Lock.Count <= 0)
		{
			if (destroyImmediately)
			{
				DestroyUnusedThisImmediately(assetLoad);
			}
			else
			{
				_releaseList.Add(assetLoad);
			}
		}
	}

	public static void DestroyUnused(bool unusedAll)
	{
		if (SingletonAuto<BundleAssetManager>.isValidInstance)
		{
			SingletonAuto<BundleAssetManager>.instance.DestroyUnusedInternal(unusedAll);
		}
	}

	public static void DestroyUnusedInFactory()
	{
		if (!SingletonAuto<BundleAssetManager>.isValidInstance)
		{
			return;
		}
		foreach (KeyValuePair<BundleAssetKey, IBundleAssetLoadBase> item in SingletonAuto<BundleAssetManager>.instance._dic)
		{
			item.Value.DestroyUnusedInFactory();
		}
	}

	private void DestroyUnusedInternal(bool unusedAll)
	{
		if (unusedAll)
		{
			DestroyUnusedAll();
		}
		else
		{
			DestroyUnusedLastOne();
		}
	}

	private void DestroyUnusedThisImmediately(IBundleAssetLoadBase This)
	{
		if (This.ISDONE() && This.Lock.Count <= 0)
		{
			BundleAssetKey key = This.Key;
			_dic.Remove(key);
			This.Close(destroyImmediately: true);
		}
	}

	private void DestroyUnusedLastOne()
	{
		int unscaledTick = UnitySystemTimer.unscaledTick;
		for (int i = 0; i < _releaseList.Count; i++)
		{
			IBundleAssetLoadBase bundleAssetLoadBase = _releaseList[i];
			if (!bundleAssetLoadBase.IsDone)
			{
				continue;
			}
			if (bundleAssetLoadBase.Lock.Count > 0)
			{
				_releaseList.RemoveAt(i);
				i--;
				continue;
			}
			if (unscaledTick - bundleAssetLoadBase.Lock.UnlockTick >= 10000)
			{
				_dic.Remove(bundleAssetLoadBase.Key);
				bundleAssetLoadBase.Close(destroyImmediately: false);
				_releaseList.RemoveAt(i);
			}
			break;
		}
	}

	private void DestroyUnusedAll()
	{
		for (int i = 0; i < _releaseList.Count; i++)
		{
			IBundleAssetLoadBase bundleAssetLoadBase = _releaseList[i];
			if (bundleAssetLoadBase.IsDone)
			{
				if (bundleAssetLoadBase.Lock.Count > 0)
				{
					_releaseList.RemoveAt(i);
					i--;
					continue;
				}
				_dic.Remove(bundleAssetLoadBase.Key);
				bundleAssetLoadBase.Close(destroyImmediately: false);
				_releaseList.RemoveAt(i);
				i--;
			}
		}
	}

	public void DestroyAll()
	{
		int num;
		for (num = 0; num < _releaseList.Count; num++)
		{
			IBundleAssetLoadBase bundleAssetLoadBase = _releaseList[num];
			_dic.Remove(bundleAssetLoadBase.Key);
			bundleAssetLoadBase.Close(destroyImmediately: true);
			_releaseList.RemoveAt(num);
			num--;
		}
	}

	public string GetSummary()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int unscaledTick = UnitySystemTimer.unscaledTick;
		foreach (IBundleAssetLoadBase value2 in _dic.Values)
		{
			int num = unscaledTick - value2.Lock.UnlockTick;
			string value = string.Format("lock={2,4}, {0}({1}) - unlockTick={3}", value2.KeyDesc, value2.IsDone, value2.Lock.Count, num);
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}
}
