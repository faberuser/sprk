using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Profiling;

namespace NGame2.NScene;

public class EmptyScene2 : BaseScene, IInspectorGuiDrawer
{
	private List<byte[]> _memList = new List<byte[]>();

	private void Awake()
	{
	}

	private void OnDestroy()
	{
	}

	private void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("EmptyScene2", this, DrawGui);
	}

	public override IPreload PreloadAsset()
	{
		return base.PreloadAsset();
	}

	public override void OnSceneUnloading(LoadingManager.ESceneType nextScene)
	{
	}

	public override void OnSceneLoaded(Action onComplete)
	{
		base.OnSceneLoaded(onComplete);
	}

	private void DrawGui()
	{
		long num = Profiler.usedHeapSize;
		long num2 = Profiler.GetMonoHeapSize();
		long num3 = Profiler.GetMonoUsedSize();
		long num4 = (long)SystemInfo.graphicsMemorySize * 1000L * 1000;
		long num5 = (long)SystemInfo.systemMemorySize * 1000L * 1000;
		long num6 = Profiler.GetTotalAllocatedMemory();
		long num7 = Profiler.GetTotalReservedMemory();
		long num8 = Profiler.GetTotalUnusedReservedMemory();
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		GUILayout.Label($"ProcessorCount={SystemInfo.processorCount}");
		GUILayout.Label(string.Format("UsedHeapSize={0}, MonoHeapSize={1}, MonoUsedSize={1}", Vespa_Long_Extension_Method.ToSizeString(num), Vespa_Long_Extension_Method.ToSizeString(num2), Vespa_Long_Extension_Method.ToSizeString(num3)));
		GUILayout.Label($"System Mem={Vespa_Long_Extension_Method.ToCommaString(num5)}, Graphics Mem={Vespa_Long_Extension_Method.ToCommaString(num4)}");
		GUILayout.Label(string.Format("Total Alloc={0}, Reserved={1}, Unused={1}", Vespa_Long_Extension_Method.ToSizeString(num6), Vespa_Long_Extension_Method.ToSizeString(num7), Vespa_Long_Extension_Method.ToSizeString(num8)));
		GUILayout.Label($"GC Total Memory={Vespa_Long_Extension_Method.ToSizeString(totalMemory)}");
		long num9 = 0L;
		for (int i = 0; i < _memList.Count; i++)
		{
			byte[] array = _memList[i];
			num9 += array.Length;
		}
		GUILayout.Label($"Current Total={Vespa_Long_Extension_Method.ToSizeString(num9)})");
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Alloc 1 MB", GUILayout.Height(50f)))
			{
				byte[] item = new byte[1000000];
				_memList.Add(item);
			}
			if (GUILayout.Button("Alloc 1 MB * 10", GUILayout.Height(50f)))
			{
				for (int j = 0; j < 10; j++)
				{
					byte[] item2 = new byte[1000000];
					_memList.Add(item2);
				}
			}
			if (GUILayout.Button("Alloc 1 MB * 100", GUILayout.Height(50f)))
			{
				for (int k = 0; k < 100; k++)
				{
					byte[] item3 = new byte[1000000];
					_memList.Add(item3);
				}
			}
			if (GUILayout.Button("Alloc 10 MB", GUILayout.Height(50f)))
			{
				byte[] item4 = new byte[10000000];
				_memList.Add(item4);
			}
			if (GUILayout.Button("Alloc 10 MB * 10", GUILayout.Height(50f)))
			{
				for (int l = 0; l < 10; l++)
				{
					byte[] item5 = new byte[10000000];
					_memList.Add(item5);
				}
			}
			if (GUILayout.Button("Alloc 100 MB", GUILayout.Height(50f)))
			{
				byte[] item6 = new byte[100000000];
				_memList.Add(item6);
			}
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Free: " + _memList.Count, GUILayout.Height(50f)))
			{
				_memList.Clear();
			}
			if (GUILayout.Button("Free2: " + _memList.Count, GUILayout.Height(50f)))
			{
				_memList = new List<byte[]>();
			}
		}
		GUILayout.Label("GC.MaxGeneration=" + GC.MaxGeneration);
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("GC", GUILayout.Height(50f)))
			{
				GC.Collect();
			}
			if (GUILayout.Button("GC(1)", GUILayout.Height(50f)))
			{
				GC.Collect(1);
			}
			if (GUILayout.Button("GC(2)", GUILayout.Height(50f)))
			{
				GC.Collect(2);
			}
			if (GUILayout.Button("GC(3)", GUILayout.Height(50f)))
			{
				GC.Collect(3);
			}
			if (GUILayout.Button("GC(99)", GUILayout.Height(50f)))
			{
				GC.Collect(99);
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}
}
