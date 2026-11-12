using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using MiniJSON;
using NGame2.NNodeTree;
using NGame2.NPatch;
using NGame2.NScene;
using NGame2.NUtil.NSystemInfo;
using NGameAsset;
using NGameAsset.NPlainAsset;
using NShared;
using NVespa.NGui;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace NVespa.NGlobal;

public class GCManager : SingletonUpdater<GCManager>
{
	public enum ECollectType
	{
		None,
		Lite,
		Normal,
		Deep
	}

	public enum MemoryType
	{
		All,
		Native,
		Managed,
		Summary
	}

	private class LoadingLogData
	{
		public string Name;

		public float Time;

		public long Memory;
	}

	private ECollectType _pendingCollect;

	private float _pendingTime;

	private float _lastCollectTime;

	private float _lastTimeForIPhoneLowMemCollectLite;

	private List<LoadingLogData> _loadingLogList = new List<LoadingLogData>();

	private float _globalClearTime;

	private long _prevTotalAlloc;

	private long _prevGcTotalMemory;

	private UnityCache.EMemorySpec _minimumSpecForActiveGc = UnityCache.EMemorySpec.Mid;

	private SupportTextureFormatChecker _supportTextureFormatChecker;

	private float prevUpdateTime;

	private MemoryType showMemType;

	private string overlayText = string.Empty;

	private long usedNativeHeap;

	private long reservedNativeHeap;

	private long usedManageHeap;

	private long reservedManageHeap;

	private long usedTotalHeap;

	private long reservedTotalHeap;

	private LocalStorageFloat _overlayUpdateCycle = new LocalStorageFloat(1f, typeof(GCManager).FullName + "_overlayUpdateCycle");

	private LocalStorageBool _showMemoryStateOverlay = new LocalStorageBool(i_default: false, typeof(GCManager).FullName + "_showMemoryState");

	public GCManager()
	{
		_supportTextureFormatChecker = new SupportTextureFormatChecker();
		GlobalAutoBehaviour<GuiManager>.instance.Add("GCManager", this, DrawGCMangerUI);
		if (_showMemoryStateOverlay.Value)
		{
			GlobalAutoBehaviour<GuiManager>.instance.AddOverlay("MemoryStatusOverlay", DrawOverlayGUI, OnUnsubscribeOverlayEvent);
		}
		Application.lowMemory += OnLowMemory;
		GarbageCollector.GCModeChanged += OnGCModeChanged;
	}

	private void OnLowMemory()
	{
		SingletonUpdater<GCManager>.instance.Collect();
	}

	private void OnGCModeChanged(GarbageCollector.Mode mode)
	{
	}

	public void UpdateMinimumSpecForActiveGc(int memSizeInMb)
	{
		_minimumSpecForActiveGc = UnityCache.SpecByMemorySize(memSizeInMb);
	}

	public void EnableGC(Action complete = null)
	{
		GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
		SingletonUpdater<GCManager>.instance.Collect(ECollectType.Normal, complete);
	}

	public void DisableGC()
	{
		GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
	}

	public override void ChainUpdate()
	{
		if (_pendingCollect != ECollectType.None && Time.realtimeSinceStartup - _pendingTime > 5f)
		{
			Collect(_pendingCollect);
		}
		if (UnityCache.MemorySpec <= _minimumSpecForActiveGc)
		{
			_ = GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading;
		}
		if (_showMemoryStateOverlay.Value)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup - prevUpdateTime > _overlayUpdateCycle.Value)
			{
				prevUpdateTime = realtimeSinceStartup;
				UpdateOverlayInfo();
			}
		}
	}

	public void GlobalClear()
	{
		_globalClearTime = Time.realtimeSinceStartup;
		GlobalUpdateBehaviour.Recreate();
	}

	public void PendingCollect(ECollectType collectType = ECollectType.Normal)
	{
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			if (_pendingCollect == ECollectType.None)
			{
				_pendingTime = Time.realtimeSinceStartup;
			}
			if (_pendingCollect < collectType)
			{
				_pendingCollect = collectType;
			}
		});
	}

	public void IPhoneLowMemCollect()
	{
		_ = UnityCache.MemorySpec;
		_ = _minimumSpecForActiveGc;
	}

	public void IPhoneLowMemCollectLite()
	{
	}

	public static void CollectSafe(ECollectType collectType = ECollectType.Normal, Action onComplete = null)
	{
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			SingletonUpdater<GCManager>.instance.Collect(collectType, onComplete);
		});
	}

	public Coroutine Collect(ECollectType collectType = ECollectType.Normal, Action onComplete = null)
	{
		switch (collectType)
		{
		case ECollectType.Lite:
			_pendingCollect = ECollectType.None;
			return VespaCoroutine.Start(coCollectLite(onComplete));
		case ECollectType.Normal:
			_pendingCollect = ECollectType.None;
			return VespaCoroutine.Start(coCollect(0, onComplete));
		case ECollectType.Deep:
			_pendingCollect = ECollectType.None;
			return VespaCoroutine.Start(coCollect(999, onComplete));
		default:
			return VespaCoroutine.Start(coCollect(0, onComplete));
		}
	}

	public Coroutine CollectAll(Action onComplete = null)
	{
		_pendingCollect = ECollectType.None;
		return VespaCoroutine.Start(coCollectAll(999, onComplete));
	}

	public void CollectAndDump(string title, Action onComplete)
	{
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coCollectAndDump(string title, Action onComplete)
	{
		SingletonAuto<LitJson.JsonPool>.instance.OnSceneChanged();
		MiniJSON.JsonPool.instance.OnSceneChanged();
		bool isComplete = false;
		SingletonUpdater<GCManager>.instance.Collect(ECollectType.Normal, delegate
		{
			isComplete = true;
		});
		float beginTime = Time.realtimeSinceStartup;
		while (!isComplete && !(Time.realtimeSinceStartup - beginTime > 3f))
		{
			yield return null;
		}
		long num = Profiler.GetTotalAllocatedMemory();
		long num2 = num - _prevTotalAlloc;
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		long num3 = totalMemory - _prevGcTotalMemory;
		string text = Vespa_Long_Extension_Method.ToSizeString(num3);
		if (num3 > 5242880)
		{
			text = Vespa_String_Extension_Method.ColorTag(text, Color.red);
		}
		else if (num3 > 1048576)
		{
			text = Vespa_String_Extension_Method.ColorTag(text, Color.yellow);
		}
		else if (num3 < 0)
		{
			text = Vespa_String_Extension_Method.ColorTag(text, Color.green);
		}
		Debug.LogWarning($"[{title}] Total Alloc={Vespa_Long_Extension_Method.ToSizeString(num)}({Vespa_Long_Extension_Method.ToSizeString(num2)}), GC Memory={Vespa_Long_Extension_Method.ToSizeString(totalMemory)}({text})");
		_prevTotalAlloc = num;
		_prevGcTotalMemory = totalMemory;
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coCollectLite(Action onComplete)
	{
		_lastCollectTime = Time.realtimeSinceStartup;
		yield return null;
		yield return null;
		GC.Collect(0);
		yield return null;
		yield return null;
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coCollect(int generation, Action onComplete)
	{
		_lastCollectTime = Time.realtimeSinceStartup;
		_loadingLogList.Clear();
		AppendLoadingLog("GCManager.coCollect begin");
		for (int index = 0; index < 5; index++)
		{
			yield return null;
		}
		AppendLoadingLog("JsonContainer.DestroyUnused");
		JsonContainer.DestroyUnused();
		yield return null;
		yield return null;
		AppendLoadingLog("BundleAssetManager.DestroyUnused");
		BundleAssetManager.DestroyUnused(unusedAll: true);
		BundleAssetManager.DestroyUnusedInFactory();
		yield return null;
		yield return null;
		AppendLoadingLog("BundleManager.DestroyUnused");
		BundleManager.DestroyUnused(unusedAll: true);
		yield return null;
		yield return null;
		if (SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			AppendLoadingLog("PlainAssetManager.instance.DestroyUnused");
			SingletonUpdater<PlainAssetManager>.instance.DestroyUnused(unusedAll: true);
		}
		yield return null;
		yield return null;
		AppendLoadingLog("Resources.UnloadUnusedAssets");
		yield return Resources.UnloadUnusedAssets();
		yield return null;
		yield return null;
		AppendLoadingLog("GC.Collect");
		GC.Collect(generation);
		yield return null;
		yield return null;
		AppendLoadingLog("GCManager.coCollect end");
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coCollectAll(int generation, Action onComplete)
	{
		_lastCollectTime = Time.realtimeSinceStartup;
		_loadingLogList.Clear();
		AppendLoadingLog("GCManager.coCollectAll begin");
		for (int index = 0; index < 5; index++)
		{
			yield return null;
		}
		AppendLoadingLog("JsonContainer.DestroyUnused");
		JsonContainer.DestroyUnused();
		AppendLoadingLog("NguiAtlasContainer.DestroyUnusedAll");
		NguiAtlasContainer.DestroyUnusedAll();
		yield return null;
		yield return null;
		AppendLoadingLog("BundleAssetManager.DestroyUnused");
		BundleAssetManager.DestroyUnused(unusedAll: true);
		BundleAssetManager.DestroyUnusedInFactory();
		yield return null;
		yield return null;
		AppendLoadingLog("BundleManager.DestroyUnused");
		BundleManager.DestroyUnused(unusedAll: true);
		yield return null;
		yield return null;
		if (SingletonUpdater<PlainAssetManager>.isValidInstance)
		{
			AppendLoadingLog("PlainAssetManager.instance.DestroyUnused");
			SingletonUpdater<PlainAssetManager>.instance.DestroyUnused(unusedAll: true);
		}
		yield return null;
		yield return null;
		AppendLoadingLog("Resources.UnloadUnusedAssets");
		yield return Resources.UnloadUnusedAssets();
		yield return null;
		yield return null;
		AppendLoadingLog("GC.Collect");
		GC.Collect(generation);
		yield return null;
		yield return null;
		AppendLoadingLog("GCManager.coCollectAll end");
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private void AppendLoadingLog(string log)
	{
		LoadingLogData loadingLogData = new LoadingLogData();
		loadingLogData.Name = log;
		loadingLogData.Time = Time.realtimeSinceStartup;
		loadingLogData.Memory = GC.GetTotalMemory(forceFullCollection: false);
		_loadingLogList.Add(loadingLogData);
	}

	public void DrawGCMangerUI()
	{
		long usedHeapSizeLong = Profiler.usedHeapSizeLong;
		long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
		long totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong();
		long monoUsedSizeLong = Profiler.GetMonoUsedSizeLong();
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		long monoHeapSizeLong = Profiler.GetMonoHeapSizeLong();
		_ = SystemInfo.graphicsMemorySize;
		_ = SystemInfo.systemMemorySize;
		Profiler.GetTotalUnusedReservedMemoryLong();
		using (new BlockManager().Horizontal())
		{
			if (_showMemoryStateOverlay.Value)
			{
				GUILayout.Label("Memory status overlay: ");
				if (GUILayout.Button("Enabled"))
				{
					_showMemoryStateOverlay.Value = false;
					GlobalAutoBehaviour<GuiManager>.instance.RemoveOverlay();
				}
			}
			else
			{
				GUILayout.Label("Memory status overlay: ");
				if (GUILayout.Button("Disabled"))
				{
					_showMemoryStateOverlay.Value = true;
					GlobalAutoBehaviour<GuiManager>.instance.AddOverlay("MemoryStatusOverlay", DrawOverlayGUI, OnUnsubscribeOverlayEvent);
				}
			}
			GUILayout.FlexibleSpace();
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("RealTime", GUILayout.Width(100f)))
			{
				_overlayUpdateCycle.Value = -1f;
			}
			if (GUILayout.Button("0.5", GUILayout.Width(50f)))
			{
				_overlayUpdateCycle.Value = 0.5f;
			}
			if (GUILayout.Button("1", GUILayout.Width(50f)))
			{
				_overlayUpdateCycle.Value = 1f;
			}
			if (GUILayout.Button("2", GUILayout.Width(50f)))
			{
				_overlayUpdateCycle.Value = 2f;
			}
			if (GUILayout.Button("3", GUILayout.Width(50f)))
			{
				_overlayUpdateCycle.Value = 3f;
			}
			if (GUILayout.Button("5", GUILayout.Width(50f)))
			{
				_overlayUpdateCycle.Value = 5f;
			}
			GUILayout.FlexibleSpace();
		}
		GUILayout.Label("Graphics API =" + SystemInfo.graphicsDeviceType);
		GUILayout.Label("Supported Texture Format =" + _supportTextureFormatChecker.GetLog("/ "));
		GUILayout.Label("Device Model =" + SystemInfo.deviceModel);
		GUILayout.Label("MemorySpec=" + Vespa_String_Extension_Method.GetEnumString(UnityCache.MemorySpec));
		GUILayout.Label("SystemMemory=" + SystemInfo.systemMemorySize + " MB");
		if (GameRoot.isValidInstance)
		{
			bool num = GameRoot.Instance.PatchDownloadSpecDataContainer != null;
			bool flag = false;
			if (num)
			{
				flag = GameRoot.Instance.PatchDownloadSpecDataContainer.IsLoaded;
			}
			if (num && flag)
			{
				PatchDownloadSpecData recommandSpecData = PatchDownloadHelper.GetRecommandSpecData();
				GUILayout.Label("PatchPlatform =" + recommandSpecData.Platform);
				GUILayout.Label("PatchSpec =" + recommandSpecData.Spec);
			}
		}
		using (new BlockManager().VerticalBox(Color.black, 0.7f))
		{
			using (new BlockManager().Vertical())
			{
				GUILayout.Label("-- Native Heap Memory(Managed from Unity engine, C++) --");
				GUILayout.Label($"Used={Vespa_Long_Extension_Method.ToSizeString(usedHeapSizeLong)}, Allocated={Vespa_Long_Extension_Method.ToSizeString(totalAllocatedMemoryLong)}, Reserved={Vespa_Long_Extension_Method.ToSizeString(totalReservedMemoryLong)}");
			}
			using (new BlockManager().Vertical())
			{
				GUILayout.Label("-- Managed Heap Memory(Managed from Mono Framework, C#) --");
				GUILayout.Label($"Used={Vespa_Long_Extension_Method.ToSizeString(monoUsedSizeLong)}, Allocated={Vespa_Long_Extension_Method.ToSizeString(totalMemory)}, Reserved={Vespa_Long_Extension_Method.ToSizeString(monoHeapSizeLong)}, ");
			}
			using (new BlockManager().Vertical())
			{
				GUILayout.Label("-- Total (C++ & C#) --");
				GUILayout.Label($"Used={Vespa_Long_Extension_Method.ToSizeString(usedHeapSizeLong + monoUsedSizeLong)}, Allocated={Vespa_Long_Extension_Method.ToSizeString(totalAllocatedMemoryLong + totalMemory)}, Reserved={Vespa_Long_Extension_Method.ToSizeString(totalReservedMemoryLong + monoHeapSizeLong)}");
			}
		}
		GUILayout.Label($"Pack Count={__Extension_Method_ICollection__.COUNT(SingletonAuto<PackContainer>.instance.Dic)}");
		GUILayout.Label($"PlainAssetJson Count={__Extension_Method_ICollection__.COUNT(SingletonAuto<JsonContainer>.instance.Dic)}");
		GUILayout.Label("GC.MaxGeneration=" + GC.MaxGeneration);
		if (GUILayout.Button("Collect"))
		{
			VespaCoroutine.Start(coCollect(0, null));
		}
		if (GUILayout.Button("Resources.UnloadUnusedAssets()"))
		{
			Resources.UnloadUnusedAssets();
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_loadingLogList); i++)
		{
			LoadingLogData loadingLogData = _loadingLogList[i];
			float time = loadingLogData.Time;
			long memory = loadingLogData.Memory;
			if (i < __Extension_Method_ICollection__.COUNT(_loadingLogList) - 1)
			{
				time = _loadingLogList[i + 1].Time;
				memory = _loadingLogList[i + 1].Memory;
			}
			GUILayout.Label($"{loadingLogData.Name} {time - loadingLogData.Time}s, Mem Delta={Vespa_Long_Extension_Method.ToSizeString(memory - loadingLogData.Memory)}");
		}
	}

	public void UpdateOverlayInfo()
	{
		usedNativeHeap = Profiler.usedHeapSizeLong;
		reservedNativeHeap = Profiler.GetTotalReservedMemoryLong();
		usedManageHeap = Profiler.GetMonoUsedSizeLong();
		reservedManageHeap = Profiler.GetMonoHeapSizeLong();
		usedTotalHeap = usedNativeHeap + usedManageHeap;
		reservedTotalHeap = reservedNativeHeap + reservedManageHeap;
		switch (showMemType)
		{
		case MemoryType.All:
			overlayText = $"Native = Used:{Vespa_Long_Extension_Method.ToSizeString(usedNativeHeap)} Reserv:{Vespa_Long_Extension_Method.ToSizeString(reservedNativeHeap)}\nManaged = Used:{Vespa_Long_Extension_Method.ToSizeString(usedManageHeap)} Reserv:{Vespa_Long_Extension_Method.ToSizeString(reservedManageHeap)}\nTotal = Used:{Vespa_Long_Extension_Method.ToSizeString(usedTotalHeap)} Reserv:{Vespa_Long_Extension_Method.ToSizeString(reservedTotalHeap)}";
			break;
		case MemoryType.Native:
			overlayText = $"Used:{Vespa_Long_Extension_Method.ToSizeString(usedNativeHeap)} Reserv:{Vespa_Long_Extension_Method.ToSizeString(reservedNativeHeap)}";
			break;
		case MemoryType.Managed:
			overlayText = $"Used:{Vespa_Long_Extension_Method.ToSizeString(usedManageHeap)} Reserv:{Vespa_Long_Extension_Method.ToSizeString(reservedManageHeap)}";
			break;
		default:
			overlayText = $"Used:{Vespa_Long_Extension_Method.ToSizeString(usedTotalHeap)} Reserv:{Vespa_Long_Extension_Method.ToSizeString(reservedTotalHeap)}";
			break;
		}
	}

	public void DrawOverlayGUI()
	{
		using (new BlockManager().VerticalBox(Color.black, 0.9f))
		{
			if (!_showMemoryStateOverlay.Value)
			{
				return;
			}
			if (GUILayout.Button(showMemType switch
			{
				MemoryType.All => "Memory Status", 
				MemoryType.Native => "Native", 
				MemoryType.Managed => "Managed", 
				_ => "Total Summary", 
			}, GUILayout.Width(400f)))
			{
				if (showMemType == MemoryType.Summary)
				{
					showMemType = MemoryType.All;
				}
				else
				{
					showMemType++;
				}
				UpdateOverlayInfo();
			}
			GUILayout.Box(overlayText, GUILayout.Width(400f));
		}
	}

	public void OnUnsubscribeOverlayEvent()
	{
		_showMemoryStateOverlay.Value = false;
	}
}
