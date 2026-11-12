using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LitJson;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace NGame2.NScene;

public class EmptyScene : BaseScene, IInspectorGuiDrawer
{
	private static bool _stopThread = false;

	private static int _jsonTestCount;

	private static int _threadStart;

	private static int _threadEnd;

	private static int _threadException;

	private List<Thread> _threadList = new List<Thread>();

	private static int _bundlePackLoopCount;

	private static string _bundlePackMessage = string.Empty;

	private LocalStorageString _resolutionHeight = new LocalStorageString("1080", typeof(EmptyScene).FullName + "_resolutionHeight");

	private static CreatureData[] AllCreatureData = null;

	private void Awake()
	{
	}

	private void OnDestroy()
	{
	}

	private void Start()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("EmptyScene", this, DrawGui);
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
		GUILayout.Label($"Resolution {Screen.width}x{Screen.height}");
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Assign Height");
			_resolutionHeight.Value = GUILayout.TextField(_resolutionHeight.Value);
		}
		int output;
		bool i_enable = String_Extension_Method.TryToInt(_resolutionHeight.Value, out output);
		using (new BlockManager().Enable(i_enable))
		{
			int num = output * Screen.width / Screen.height;
			if (GUILayout.Button($"Asset {num}x{output}"))
			{
				Screen.SetResolution(num, output, fullscreen: true);
			}
		}
		if (GUILayout.Button("Load LobbyScene"))
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
		}
		if (GUILayout.Button("Load EmptyScene"))
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Empty);
		}
		if (GUILayout.Button("Load EmptyScene Direct"))
		{
			SceneManager.LoadScene(EnumTools.GetName(LoadingManager.ESceneType.Empty));
		}
		using (new BlockManager().Enable(SingletonAuto<TableDataManager>.instance.IsLoaded()))
		{
			if (GUILayout.Button("Load All NodeTree"))
			{
				VespaCoroutine.Start(coLoadAllNodeTree());
			}
		}
		GUILayout.Label("Total Alloc=" + Profiler.GetTotalAllocatedMemory());
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Json Test"))
			{
				ExecuteJsonTest();
			}
			if (GUILayout.Button("Stop"))
			{
				_stopThread = true;
			}
		}
		using (new BlockManager().Vertical(GUILayout.Width(400f)))
		{
			GUILayout.Label("Json Pool\n" + SingletonAuto<JsonPool>.instance.Dump());
			GUILayout.Label("Thread Start " + _threadStart);
			GUILayout.Label("Thread End " + _threadEnd);
			GUILayout.Label("Thread Exception" + _threadException);
			GUILayout.Label("Json Test Count " + _jsonTestCount);
			GUILayout.Label("BundlePack Loop " + _bundlePackLoopCount);
			GUILayout.Label("BundlePack Msg: " + _bundlePackMessage);
		}
	}

	private static IEnumerator coLoadAllNodeTree()
	{
		if (AllCreatureData == null)
		{
			AllCreatureData = GameRoot.Instance.CreatureDataContainer.GetAllData().ToArray();
		}
		yield return SingletonUpdater<GCManager>.instance.Collect();
		long num = Profiler.GetTotalAllocatedMemory();
		VespaDebug.LogError($"Load Pack Start: totalAlloc={num}");
		List<PackLoad> packLoadList = new List<PackLoad>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AllCreatureData); i++)
		{
			string pathNameFromCode = PackHelper.GetPathNameFromCode(AllCreatureData[i].CodeName);
			PackLoad pack = SingletonAuto<PackContainer>.instance.GetPack(pathNameFromCode);
			packLoadList.Add(pack);
		}
		while (packLoadList.Count > 0)
		{
			yield return null;
			if (packLoadList[0].ISDONE())
			{
				packLoadList.RemoveAt(0);
			}
		}
		num = Profiler.GetTotalAllocatedMemory();
		VespaDebug.LogError($"Load Pack End: totalAlloc={num}");
		yield return SingletonUpdater<GCManager>.instance.Collect();
		num = Profiler.GetTotalAllocatedMemory();
		VespaDebug.LogError($"Load Pack End2: totalAlloc={num}");
	}

	public override void Update()
	{
		base.Update();
	}

	private void ExecuteJsonTest()
	{
		_stopThread = false;
		for (int i = 0; i < 16; i++)
		{
			Thread thread = new Thread(ThreadJsonTest);
			thread.Priority = System.Threading.ThreadPriority.Normal;
			thread.Name = $"VespaThread {i}";
			thread.Start();
			_threadList.Add(thread);
		}
	}

	private static void ThreadJsonTest()
	{
		Interlocked.Increment(ref _threadStart);
		try
		{
			while (SystemStatus.IsPlaying && !_stopThread)
			{
				Interlocked.Increment(ref _jsonTestCount);
				JsonData json = JsonData.CreateJsonData();
				for (int i = 0; i < 10; i++)
				{
					json.AddField("item" + i, CreateTestJson());
				}
				SingletonAuto<JsonPool>.instance.SafeRelease(ref json);
				Thread.Sleep(1);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			Interlocked.Increment(ref _threadException);
		}
		Interlocked.Increment(ref _threadEnd);
		Debug.Log("Application not playing");
	}

	private static JsonData CreateTestJson(int depth = 0)
	{
		JsonData jsonData = JsonData.CreateJsonData();
		jsonData.AddField("double", JsonData.CreateJsonDouble(depth));
		jsonData.AddField("long", JsonData.CreateJsonLong(depth));
		jsonData.AddField("string", JsonData.CreateJsonString(depth.ToString()));
		if (depth > 10)
		{
			return jsonData;
		}
		jsonData.AddField("child" + depth, CreateTestJson(depth + 1));
		return jsonData;
	}
}
