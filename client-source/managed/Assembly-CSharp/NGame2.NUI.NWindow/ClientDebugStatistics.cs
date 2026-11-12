using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using NGame2.NScene.NPreloader;
using NGameAsset;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Profiling;

namespace NGame2.NUI.NWindow;

public class ClientDebugStatistics : MonoBehaviour, IGlobalWindow, IInspectorGuiDrawer
{
	private class ThreadInfoData
	{
		public float LastThreadUpdateTime;

		public string ThreadStatistics = string.Empty;

		public void Dump(StringBuilder sb)
		{
			sb.Append(ThreadStatistics);
		}

		public void Refresh()
		{
			if (Time.realtimeSinceStartup - LastThreadUpdateTime < 1f)
			{
				return;
			}
			LastThreadUpdateTime = Time.realtimeSinceStartup;
			Process currentProcess = Process.GetCurrentProcess();
			if (currentProcess == null)
			{
				ThreadStatistics = "null process\n";
				return;
			}
			Thread currentThread = Thread.CurrentThread;
			if (currentThread == null)
			{
				ThreadStatistics = "null thread\n";
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Process {currentProcess.ProcessName}({currentProcess.Id}) - PriorityClass={Vespa_String_Extension_Method.GetEnumString(currentProcess.PriorityClass)}");
			ProcessThreadCollection threads = currentProcess.Threads;
			if (threads != null)
			{
				for (int i = 0; i < threads.Count; i++)
				{
					ProcessThread processThread = threads[i];
					stringBuilder.AppendLine($"Thread{i}: Priority={Vespa_String_Extension_Method.GetEnumString(processThread.PriorityLevel)}({processThread.CurrentPriority}), Id={processThread.Id}");
				}
			}
			stringBuilder.AppendLine($"Thread {currentThread.Name}: ThreadId={currentThread.ManagedThreadId}, Priority={Vespa_String_Extension_Method.GetEnumString(currentThread.Priority)}");
			ThreadStatistics = stringBuilder.ToString();
		}
	}

	private class PerFrameData
	{
		public int BundleLoadingCount;

		public int BundleCount;

		public int BundleAssetCount;

		public int PlainAssetLoadingCount;

		public int WwwCount;

		public int AsyncOperationCount;

		public int AsyncManagerEntryCount;

		public void Dump(StringBuilder sb)
		{
			sb.AppendLine($"Bundle Count: {BundleCount}");
			sb.AppendLine($"Bundle Loading: {BundleLoadingCount}");
			sb.AppendLine($"Bundle Asset Count: {BundleAssetCount}");
			sb.AppendLine($"PlainAsset Loading: {PlainAssetLoadingCount}");
			sb.AppendLine($"WWW Count: {WwwCount}");
			sb.AppendLine($"AsyncOperation Count: {AsyncOperationCount}");
			sb.AppendLine($"AsyncManager Entry: {AsyncManagerEntryCount}");
		}

		public void Refresh()
		{
			BundleLoadingCount = 0;
			BundleCount = 0;
			BundleAssetCount = 0;
			PlainAssetLoadingCount = 0;
			WwwCount = 0;
			AsyncOperationCount = 0;
			AsyncManagerEntryCount = 0;
			if (SingletonAuto<BundleManager>.isValidInstance)
			{
				BundleCount = SingletonAuto<BundleManager>.instance.Loader.BundleCount;
				BundleLoadingCount = SingletonAuto<BundleManager>.instance.Loader.LoadingCount;
			}
			if (SingletonAuto<BundleAssetManager>.isValidInstance)
			{
				BundleAssetCount = SingletonAuto<BundleAssetManager>.instance.Count;
			}
			if (SingletonUpdater<PlainAssetManager>.isValidInstance)
			{
				PlainAssetLoadingCount = SingletonUpdater<PlainAssetManager>.instance.LoadingCount;
			}
			if (SingletonUpdater<AsyncManager>.isValidInstance)
			{
				WwwCount = SingletonUpdater<AsyncManager>.instance.WwwCount;
				AsyncOperationCount = SingletonUpdater<AsyncManager>.instance.AsyncOperationCount;
				AsyncManagerEntryCount = SingletonUpdater<AsyncManager>.instance.EntryCount;
			}
		}
	}

	private class PerSecondData
	{
		public float LastUpdateTime;

		public int MaterialCount;

		public int RendererCount;

		public int RendererActiveCount;

		public int DrawCallCount;

		public int NguiPanelCount;

		public int NguiWidgetCount;

		public string CameraStatistics = string.Empty;

		public void Dump(StringBuilder sb)
		{
			sb.AppendLine($"Material : {MaterialCount}");
			sb.AppendLine($"Renderer : {RendererActiveCount}/{RendererCount}");
			sb.AppendLine($"DrawCall : {DrawCallCount}");
			sb.AppendLine($"NGUI Panel : {NguiPanelCount}");
			sb.AppendLine($"NGUI Widget : {NguiWidgetCount}");
			sb.Append(CameraStatistics);
		}

		public void Refresh()
		{
			if (Time.realtimeSinceStartup - LastUpdateTime < 1f)
			{
				return;
			}
			LastUpdateTime = Time.realtimeSinceStartup;
			Material[] array = Resources.FindObjectsOfTypeAll<Material>();
			MaterialCount = __Extension_Method_ICollection__.COUNT(array);
			Renderer[] array2 = Resources.FindObjectsOfTypeAll<Renderer>();
			RendererCount = __Extension_Method_ICollection__.COUNT(array2);
			RendererActiveCount = 0;
			DrawCallCount = 0;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array2); i++)
			{
				Renderer renderer = array2[i];
				if (renderer.enabled && renderer.gameObject.activeInHierarchy)
				{
					RendererActiveCount++;
					DrawCallCount += __Extension_Method_ICollection__.COUNT(renderer.sharedMaterials);
				}
			}
			NguiPanelCount = __Extension_Method_ICollection__.COUNT(UIPanel.list);
			NguiWidgetCount = 0;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(UIPanel.list); j++)
			{
				UIPanel uIPanel = UIPanel.list[j];
				if (!(uIPanel == null))
				{
					NguiWidgetCount += uIPanel.widgets.COUNT();
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			Camera[] array3 = UnityEngine.Object.FindObjectsOfType<Camera>();
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(array3); k++)
			{
				Camera camera = array3[k];
				stringBuilder.Append(Vespa_Unity_Object_Extension_Method.NAME(camera));
				if (!GlobalCameraManager.HasMainRenderTarget(camera))
				{
					stringBuilder.Append(", RT");
				}
				if (!camera.enabled)
				{
					stringBuilder.Append(", disabled");
				}
				stringBuilder.AppendLine();
			}
			CameraStatistics = stringBuilder.ToString();
		}
	}

	private class MemUsageData
	{
		public List<Tuple<int, UnityEngine.Object>> MemUsageList = new List<Tuple<int, UnityEngine.Object>>();

		public string ResultStr;

		public void CollectMemoryUsage<T>(int maxLine = 99999) where T : UnityEngine.Object
		{
			T[] array = Resources.FindObjectsOfTypeAll<T>();
			int num = 0;
			MemUsageList = new List<Tuple<int, UnityEngine.Object>>();
			for (int i = 0; i < array.Length; i++)
			{
				int runtimeMemorySize = Profiler.GetRuntimeMemorySize(array[i]);
				num += runtimeMemorySize;
				MemUsageList.Add(Tuple.Create(runtimeMemorySize, (UnityEngine.Object)array[i]));
			}
			MemUsageList.Sort((Tuple<int, UnityEngine.Object> lhs, Tuple<int, UnityEngine.Object> rhs) => rhs.Item1.CompareTo(lhs.Item1));
			StringBuilder stringBuilder = new StringBuilder("");
			stringBuilder.AppendLine($"--- Total Memory Usage {num} ---");
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(MemUsageList) && num2 < maxLine; num2++)
			{
				Tuple<int, UnityEngine.Object> tuple = MemUsageList[num2];
				if (!tuple.Item2.name.StartsWith("d_"))
				{
					stringBuilder.AppendLine($"{tuple.Item2.name}: Mem={tuple.Item1}");
				}
			}
			if (__Extension_Method_ICollection__.COUNT(MemUsageList) > maxLine)
			{
				MemUsageList.RemoveRange(maxLine, __Extension_Method_ICollection__.COUNT(MemUsageList) - maxLine);
			}
			ResultStr = stringBuilder.ToString();
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/ClientDebugStatistics";

	private static GlobalWindowPrefab<ClientDebugStatistics> WindowPrefab = new GlobalWindowPrefab<ClientDebugStatistics>("Resources/Prefab2/Window/ClientDebugStatistics", GlobalUIRoot.ELayerType.GlobalUI);

	public static LocalStorageBool OpenStatistics = new LocalStorageBool(i_default: false, typeof(ClientDebugStatistics).FullName + "OpenStatistics");

	public static LocalStorageFloat StatisticsAlpha = new LocalStorageFloat(1f, typeof(ClientDebugStatistics).FullName + "StatisticsAlpha");

	public static IncreaseDic<string, string> DisplayDic = new IncreaseDic<string, string>();

	public UIWidget StatisticsRoot;

	public UILabel StatisticsLabel;

	public UILabel DisplayLabel;

	private LocalStorageBool _enablePerFrame = new LocalStorageBool(i_default: true, typeof(ClientDebugStatistics).FullName + "_enablePerFrame");

	private LocalStorageBool _enableThreadInfo = new LocalStorageBool(i_default: false, typeof(ClientDebugStatistics).FullName + "_enableThreadInfo");

	private LocalStorageBool _enablePerSecond = new LocalStorageBool(i_default: false, typeof(ClientDebugStatistics).FullName + "_enablePerSecond");

	private LocalStorageBool _enableDisplay = new LocalStorageBool(i_default: false, typeof(ClientDebugStatistics).FullName + "_enableDisplay");

	private PerFrameData _perFrame = new PerFrameData();

	private PerSecondData _perSecond = new PerSecondData();

	private ThreadInfoData _threadInfo = new ThreadInfoData();

	private MemUsageData _memUsage = new MemUsageData();

	private StringBuilder _stringBuilder = new StringBuilder();

	public static void Open()
	{
		if (!WindowPrefab.IsOpen())
		{
			ClientDebugStatistics instance = WindowPrefab.instance;
			if (!(instance == null))
			{
				OpenStatistics.Value = true;
				instance.OpenInternal();
			}
		}
	}

	public static bool IsOpen()
	{
		return WindowPrefab.IsOpen();
	}

	public static void Close()
	{
		OpenStatistics.Value = false;
		WindowPrefab.CloseWindow();
	}

	internal void OpenInternal()
	{
		base.gameObject.SetActive(value: true);
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	bool IGlobalWindow.IsOpen()
	{
		return base.gameObject.activeSelf;
	}

	void IGlobalWindow.Close()
	{
		base.gameObject.SetActive(value: false);
	}

	public void CollectObjMemoryUsage()
	{
		_memUsage.CollectMemoryUsage<UnityEngine.Object>(100);
	}

	public void CollectTexMemoryUsage()
	{
		_memUsage.CollectMemoryUsage<Texture>(100);
	}

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("Client Debug Statistics", this, delegate
		{
			_enablePerFrame.Value = GUILayout.Toggle(_enablePerFrame.Value, "Enable PerFrame Data");
			_enablePerSecond.Value = GUILayout.Toggle(_enablePerSecond.Value, "Enable PerSecond Data");
			_enableThreadInfo.Value = GUILayout.Toggle(_enableThreadInfo.Value, "Enable Thread Info");
			_enableDisplay.Value = GUILayout.Toggle(_enableDisplay.Value, "Enable Display");
			VespaProfiler.Enabled.Value = GUILayout.Toggle(VespaProfiler.Enabled.Value, "VespaProfiler Enabled");
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("Sort Profile"))
				{
					SingletonAuto<VespaProfiler>.instance.Root.Sort();
				}
				if (GUILayout.Button("Clear Profile"))
				{
					SingletonAuto<VespaProfiler>.instance.Clear();
				}
			}
			if (GUILayout.Button("Clear Display"))
			{
				DisplayDic.Clear();
			}
		});
	}

	private void Update()
	{
		Update2();
	}

	private void Update1()
	{
		RefreshCount();
		_stringBuilder.Length = 0;
		_stringBuilder.AppendLine($"Resource Preload Time: {RootPreloader.ResourcePreloadTime:F3}");
		long num = Profiler.usedHeapSize;
		long num2 = Profiler.GetMonoHeapSize();
		long num3 = Profiler.GetMonoUsedSize();
		long num4 = (long)SystemInfo.graphicsMemorySize * 1000L * 1000;
		long num5 = (long)SystemInfo.systemMemorySize * 1000L * 1000;
		long num6 = Profiler.GetTotalAllocatedMemory();
		long num7 = Profiler.GetTotalReservedMemory();
		long num8 = Profiler.GetTotalUnusedReservedMemory();
		long num9 = num6 - SingletonCreator.InitTotalAlloc;
		long num10 = num7 - SingletonCreator.InitTotalReserved;
		long num11 = num8 - SingletonCreator.InitUnusedReserved;
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		_stringBuilder.AppendLine($"ProcessorCount={SystemInfo.processorCount}");
		_stringBuilder.AppendLine(string.Format("UsedHeapSize={0}, MonoHeapSize={1}, MonoUsedSize={1}", Vespa_Long_Extension_Method.ToSizeString(num), Vespa_Long_Extension_Method.ToSizeString(num2), Vespa_Long_Extension_Method.ToSizeString(num3)));
		_stringBuilder.AppendLine($"System Mem={Vespa_Long_Extension_Method.ToCommaString(num5)}, Graphics Mem={Vespa_Long_Extension_Method.ToCommaString(num4)}");
		_stringBuilder.AppendLine(string.Format("Total Alloc={0}, Reserved={1}, Unused={1}", Vespa_Long_Extension_Method.ToSizeString(num6), Vespa_Long_Extension_Method.ToSizeString(num7), Vespa_Long_Extension_Method.ToSizeString(num8)));
		_stringBuilder.AppendLine(string.Format("Delta Alloc={0}, Reserved={1}, Unused={1}", Vespa_Long_Extension_Method.ToSizeString(num9), Vespa_Long_Extension_Method.ToSizeString(num10), Vespa_Long_Extension_Method.ToSizeString(num11)));
		_stringBuilder.AppendLine($"GC Total Memory={Vespa_Long_Extension_Method.ToSizeString(totalMemory)}");
		if (!String_Extension_Method.IsNullOrEmpty(_memUsage.ResultStr))
		{
			_stringBuilder.AppendLine(_memUsage.ResultStr);
		}
		UIHelper.SetWidgetAlpha(StatisticsRoot, StatisticsAlpha.Value);
		UIHelper.SetLabelText(StatisticsLabel, _stringBuilder.ToString());
		UIHelper.EnableGameObject(DisplayLabel, _enableDisplay.Value);
		if (!_enableDisplay.Value)
		{
			return;
		}
		_stringBuilder.Length = 0;
		SingletonAuto<VespaProfiler>.instance.Dump(_stringBuilder);
		for (int i = 0; i < DisplayDic.Count; i++)
		{
			if (i > 0)
			{
				_stringBuilder.AppendLine();
			}
			KeyValuePair<string, string> keyValuePairByIndex = DisplayDic.GetKeyValuePairByIndex(i);
			_stringBuilder.Append($"{keyValuePairByIndex.Key}: {keyValuePairByIndex.Value}");
		}
		UIHelper.SetLabelText(DisplayLabel, _stringBuilder.ToString());
	}

	private void Update2()
	{
		RefreshCount();
		_stringBuilder.Length = 0;
		_stringBuilder.AppendLine($"Resource Preload Time: {RootPreloader.ResourcePreloadTime:F3}");
		if (_enablePerFrame.Value)
		{
			_stringBuilder.AppendLine($"ChainUpdate: {Counter.ChainUpdateCount}");
			_stringBuilder.AppendLine($"Update: {Counter.UpdateCount}");
			long num = Profiler.usedHeapSize;
			long num2 = Profiler.GetMonoHeapSize();
			long num3 = Profiler.GetMonoUsedSize();
			long num4 = (long)SystemInfo.graphicsMemorySize * 1000L * 1000;
			long num5 = (long)SystemInfo.systemMemorySize * 1000L * 1000;
			long num6 = Profiler.GetTotalAllocatedMemory();
			long num7 = Profiler.GetTotalReservedMemory();
			long num8 = Profiler.GetTotalUnusedReservedMemory();
			long num9 = num6 - SingletonCreator.InitTotalAlloc;
			long num10 = num7 - SingletonCreator.InitTotalReserved;
			long num11 = num8 - SingletonCreator.InitUnusedReserved;
			_stringBuilder.AppendLine($"ProcessorCount={SystemInfo.processorCount}");
			_stringBuilder.AppendLine(string.Format("UsedHeapSize={0}, MonoHeapSize={1}, MonoUsedSize={1}", Vespa_Long_Extension_Method.ToSizeString(num), Vespa_Long_Extension_Method.ToSizeString(num2), Vespa_Long_Extension_Method.ToSizeString(num3)));
			_stringBuilder.AppendLine($"System Mem={Vespa_Long_Extension_Method.ToCommaString(num5)}, Graphics Mem={Vespa_Long_Extension_Method.ToCommaString(num4)}");
			_stringBuilder.AppendLine(string.Format("Total Alloc={0}, Reserved={1}, Unused={1}", Vespa_Long_Extension_Method.ToSizeString(num6), Vespa_Long_Extension_Method.ToSizeString(num7), Vespa_Long_Extension_Method.ToSizeString(num8)));
			_stringBuilder.AppendLine(string.Format("Delta Alloc={0}, Reserved={1}, Unused={1}", Vespa_Long_Extension_Method.ToSizeString(num9), Vespa_Long_Extension_Method.ToSizeString(num10), Vespa_Long_Extension_Method.ToSizeString(num11)));
			_perFrame.Dump(_stringBuilder);
		}
		if (_enablePerSecond.Value)
		{
			_perSecond.Dump(_stringBuilder);
		}
		if (_enableThreadInfo.Value)
		{
			_threadInfo.Dump(_stringBuilder);
		}
		UIHelper.SetWidgetAlpha(StatisticsRoot, StatisticsAlpha.Value);
		UIHelper.SetLabelText(StatisticsLabel, _stringBuilder.ToString());
		UIHelper.EnableGameObject(DisplayLabel, _enableDisplay.Value);
		if (!_enableDisplay.Value)
		{
			return;
		}
		_stringBuilder.Length = 0;
		SingletonAuto<VespaProfiler>.instance.Dump(_stringBuilder);
		for (int i = 0; i < DisplayDic.Count; i++)
		{
			if (i > 0)
			{
				_stringBuilder.AppendLine();
			}
			KeyValuePair<string, string> keyValuePairByIndex = DisplayDic.GetKeyValuePairByIndex(i);
			_stringBuilder.Append($"{keyValuePairByIndex.Key}: {keyValuePairByIndex.Value}");
		}
		UIHelper.SetLabelText(DisplayLabel, _stringBuilder.ToString());
	}

	private void RefreshCount()
	{
		_perFrame.Refresh();
		if (_enablePerSecond.Value)
		{
			_perSecond.Refresh();
		}
		if (_enableThreadInfo.Value)
		{
			_threadInfo.Refresh();
		}
	}
}
