using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using UnityEngine;
using UnityEngine.Profiling;

namespace NGameAsset;

public class AssetCacheService : SingletonUpdater<AssetCacheService>
{
	private class WindowedCounter
	{
		private List<int> request = new List<int>();

		private List<int> success = new List<int>();

		private long _lastTick;

		private const int WINDOW_TICKS = 5000;

		private const int NUM_WINDOWS = 6;

		public int NumRequest => request.Sum();

		public int NumSuccess => success.Sum();

		public void Inc(bool succeeded)
		{
			long milliTicks = GetMilliTicks();
			milliTicks /= 5000;
			if (_lastTick != milliTicks)
			{
				_lastTick = milliTicks;
				request.Insert(0, 1);
				success.Insert(0, succeeded ? 1 : 0);
				if (request.Count > 6)
				{
					request.RemoveAt(6);
					success.RemoveAt(6);
				}
				return;
			}
			List<int> list = request;
			int value = list[0] + 1;
			list[0] = value;
			if (succeeded)
			{
				List<int> list2 = success;
				value = list2[0] + 1;
				list2[0] = value;
			}
		}
	}

	private Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();

	private Dictionary<string, Dictionary<string, UnityEngine.Object>> _assetDict = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();

	private Dictionary<string, long> _cacheDict = new Dictionary<string, long>();

	private SortedList _sortedCachedTime = new SortedList();

	private Dictionary<string, UnityEngine.Object> _resourceDict = new Dictionary<string, UnityEngine.Object>();

	private Dictionary<string, long> _resourceCacheDict = new Dictionary<string, long>();

	private SortedList _sortedResourceCachedTime = new SortedList();

	private long _LastUniqueTick;

	private int _maxCachedBundleSize;

	private int _maxCachedResourceSize;

	private long _retainTicks = 10000L;

	private UnityCache.EMemorySpec _maximumSpecToDisableCache = UnityCache.EMemorySpec.Mid;

	private bool _enableCache;

	private WindowedCounter _bundleCounter = new WindowedCounter();

	private WindowedCounter _assetCounter = new WindowedCounter();

	private WindowedCounter _resourceCounter = new WindowedCounter();

	private bool IsCahceServiceEnabled()
	{
		return _enableCache;
	}

	public void StartService()
	{
		_enableCache = IsCahceServiceEnabled();
	}

	public void ConfigCache(int bundleSize, int resourceSize, int retainTicks, int memSizeInMb)
	{
		if (IsCahceServiceEnabled())
		{
			_maxCachedBundleSize = bundleSize;
			_maxCachedResourceSize = resourceSize;
			_retainTicks = retainTicks;
			_maximumSpecToDisableCache = UnityCache.SpecByMemorySize(memSizeInMb);
		}
	}

	public bool GetCachedAssetBundle(string bundle, out AssetBundle b)
	{
		SprkImageRecovery.BeforeCachedBundle(this, bundle);
		if (_assetBundleDict.TryGetValue(bundle, out b))
		{
			if (ReviveFromCache(_cacheDict, _sortedCachedTime, bundle))
			{
				_bundleCounter.Inc(succeeded: true);
			}
			return true;
		}
		_bundleCounter.Inc(succeeded: false);
		return false;
	}

	public void CacheAssetBundle(string bundle, AssetBundle b)
	{
		if (_assetBundleDict.ContainsKey(bundle))
		{
			VespaDebug.LogError("Redundant bundle cache: " + bundle);
			ReviveFromCache(_cacheDict, _sortedCachedTime, bundle);
		}
		else
		{
			_assetBundleDict[bundle] = b;
			_assetDict[bundle] = new Dictionary<string, UnityEngine.Object>();
		}
	}

	public void MarkBundleToBeDisposed(string bundle)
	{
		if (!_enableCache)
		{
			UnloadBundle(bundle);
		}
		else if (_cacheDict.ContainsKey(bundle))
		{
			VespaDebug.LogError("Redundant disposal of bundle cache: " + bundle);
		}
		else
		{
			AddToCache(_cacheDict, _sortedCachedTime, bundle);
		}
	}

	public bool GetCachedAsset(string bundle, string asset, out UnityEngine.Object o)
	{
		SprkImageRecovery.BeforeCachedAsset(this, bundle, asset);
		if (_assetDict.TryGetValue(bundle, out var value) && !_cacheDict.ContainsKey(bundle))
		{
			if (value.TryGetValue(asset, out o))
			{
				_assetCounter.Inc(succeeded: true);
				return true;
			}
		}
		else
		{
			VespaDebug.LogError("GetCachedAssetBundle() should be called first: " + bundle);
		}
		o = null;
		_assetCounter.Inc(succeeded: false);
		return false;
	}

	public void CacheAsset(string bundle, string asset, UnityEngine.Object o)
	{
		if (_assetDict.TryGetValue(bundle, out var value))
		{
			if (value.ContainsKey(asset))
			{
				VespaDebug.LogError("Redundant asset cache: " + bundle + ", " + asset);
			}
			else
			{
				value.Add(asset, o);
			}
		}
		else
		{
			VespaDebug.LogError("Caching asset for non existing bundle: " + bundle);
		}
	}

	public bool GetCachedResource(string resource, out UnityEngine.Object o)
	{
		SprkImageRecovery.BeforeCachedResource(this, resource);
		bool flag = _resourceDict.TryGetValue(resource, out o);
		_resourceCounter.Inc(flag);
		if (flag)
		{
			_resourceCacheDict[resource] = GetUniqueMilliTicks();
		}
		return flag;
	}

	public void CacheResource(string resource, UnityEngine.Object o)
	{
		if (_enableCache)
		{
			if (_resourceDict.ContainsKey(resource))
			{
				VespaDebug.LogError("Redundant resource: " + resource);
			}
			else
			{
				_resourceDict[resource] = o;
			}
			AddToCache(_resourceCacheDict, _sortedResourceCachedTime, resource);
		}
	}

	public override void ChainUpdate()
	{
		long num = GetMilliTicks() - _retainTicks;
		while (_sortedCachedTime.Count > _maxCachedBundleSize && (long)_sortedCachedTime.GetKey(0) < num)
		{
			string text = _sortedCachedTime.GetByIndex(0) as string;
			UnloadBundle(text);
			_cacheDict.Remove(text);
			_sortedCachedTime.RemoveAt(0);
		}
		while (_sortedResourceCachedTime.Count > _maxCachedResourceSize && (long)_sortedResourceCachedTime.GetKey(0) < num)
		{
			string key = _sortedResourceCachedTime.GetByIndex(0) as string;
			_resourceCacheDict.Remove(key);
			_sortedResourceCachedTime.RemoveAt(0);
		}
	}

	private void UnloadBundle(string bundle)
	{
		if (_assetBundleDict.ContainsKey(bundle))
		{
			if (_assetBundleDict[bundle] != null)
			{
				_assetBundleDict[bundle].Unload(unloadAllLoadedObjects: false);
			}
			_assetBundleDict.Remove(bundle);
		}
		else
		{
			VespaDebug.LogError("Missing bundle cache, name = " + bundle);
		}
		_assetDict.Remove(bundle);
	}

	private bool ReviveFromCache(Dictionary<string, long> dict, SortedList list, string name)
	{
		if (dict.TryGetValue(name, out var value))
		{
			dict.Remove(name);
			list.Remove(value);
			return true;
		}
		return false;
	}

	private void AddToCache(Dictionary<string, long> dict, SortedList list, string name)
	{
		long num = (dict[name] = GetUniqueMilliTicks());
		list[num] = name;
	}

	public static long GetMilliTicks()
	{
		return DateTime.UtcNow.Ticks / 10000;
	}

	private long GetUniqueMilliTicks()
	{
		_LastUniqueTick = System.Math.Max(GetMilliTicks(), _LastUniqueTick + 1);
		return _LastUniqueTick;
	}

	public void ForceEmptyBundle()
	{
		for (int i = 0; i < _sortedCachedTime.Count; i++)
		{
			UnloadBundle(_sortedCachedTime.GetByIndex(i) as string);
		}
		_cacheDict.Clear();
		_sortedCachedTime.Clear();
	}

	public void ForceEmptyResource()
	{
		_resourceDict.Clear();
		_resourceCacheDict.Clear();
		_sortedResourceCachedTime.Clear();
	}

	public void DrawGui()
	{
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Increase Cache Size by 10"))
			{
				_maxCachedBundleSize += 10;
			}
			if (GUILayout.Button("Decrease Cache Size by 10"))
			{
				_maxCachedBundleSize -= 10;
			}
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Increase Retain Tick by 10 sec"))
			{
				_retainTicks += 10000L;
			}
			if (GUILayout.Button("Decrease Retain Tick by 10 sec"))
			{
				_retainTicks -= 10000L;
			}
		}
		int count = _sortedCachedTime.Count;
		int num = _assetBundleDict.Count - count;
		GUILayout.Label($"Total Alloc: {Vespa_Long_Extension_Method.ToSizeString(Profiler.GetTotalAllocatedMemoryLong())} GC Total: {Vespa_Long_Extension_Method.ToSizeString(GC.GetTotalMemory(forceFullCollection: false))} UnityHeap: {Vespa_Long_Extension_Method.ToSizeString(Profiler.usedHeapSizeLong)}");
		GUILayout.Label("Active Bundle: " + num);
		GUILayout.Label("Cached Bundle: " + count + ", max: " + _maxCachedBundleSize);
		GUILayout.Label("Bundle Hit (last 30 sec): " + 100 * _bundleCounter.NumSuccess / (_bundleCounter.NumRequest + 1) + "% over " + _bundleCounter.NumRequest);
		GUILayout.Label("Asset Hit (last 30 sec): " + 100 * _assetCounter.NumSuccess / (_assetCounter.NumRequest + 1) + "% over " + _assetCounter.NumRequest);
		long milliTicks = GetMilliTicks();
		if (count > 0)
		{
			GUILayout.Label("Oldest cache: " + (milliTicks - (long)_sortedCachedTime.GetKey(0)) / 1000 + " expire after " + _retainTicks / 1000 + " sec");
			GUILayout.Label("25 percentile: " + (milliTicks - (long)_sortedCachedTime.GetKey(count / 4)) / 1000 + ",  50 percentile: " + (milliTicks - (long)_sortedCachedTime.GetKey(count / 2)) / 1000 + ",  75 percentile: " + (milliTicks - (long)_sortedCachedTime.GetKey(count * 3 / 4)) / 1000);
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Increase Resource Size by 5"))
			{
				_maxCachedResourceSize += 5;
			}
			if (GUILayout.Button("Decrease Resource Size by 5"))
			{
				_maxCachedResourceSize -= 5;
			}
		}
		int count2 = _sortedResourceCachedTime.Count;
		GUILayout.Label("Cached Resource: " + count2 + ", max: " + _maxCachedResourceSize);
		GUILayout.Label("Resource Hit (last 30 sec): " + 100 * _resourceCounter.NumSuccess / (_resourceCounter.NumRequest + 1) + "% over " + _resourceCounter.NumRequest);
		if (count2 > 0)
		{
			GUILayout.Label("Oldest cache: " + (milliTicks - (long)_sortedResourceCachedTime.GetKey(0)) / 1000 + " expire after " + _retainTicks / 1000 + " sec");
			GUILayout.Label("25 percentile: " + (milliTicks - (long)_sortedResourceCachedTime.GetKey(count2 / 4)) / 1000 + ",  50 percentile: " + (milliTicks - (long)_sortedResourceCachedTime.GetKey(count2 / 2)) / 1000 + ",  75 percentile: " + (milliTicks - (long)_sortedResourceCachedTime.GetKey(count2 * 3 / 4)) / 1000);
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Force empty bundle"))
			{
				ForceEmptyBundle();
			}
			if (GUILayout.Button("Force empty resource"))
			{
				ForceEmptyResource();
			}
			if (GUILayout.Button("Garbage Collect"))
			{
				SingletonUpdater<GCManager>.instance.Collect();
			}
			if (GUILayout.Button(_enableCache ? "Disalbe Cache" : "Enable Cache"))
			{
				_enableCache = !_enableCache;
			}
		}
	}
}
