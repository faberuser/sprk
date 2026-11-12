using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NDevice;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.ProfileLog;
using NVespa.NGui;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace NVespa.NGlobal;

public class ProfileManager : GlobalAutoBehaviour<ProfileManager>
{
	private class FrameRecorder
	{
		private float _frameCap;

		private int _numBuckets;

		private int[] _frameDistribution;

		private float _prevTime;

		private int _count;

		public int Count => _count;

		public FrameRecorder(float framedCap, int numBuckets)
		{
			_frameCap = framedCap;
			_numBuckets = numBuckets;
			_frameDistribution = new int[numBuckets];
			BeginNewRecord();
		}

		public void BeginNewRecord()
		{
			Array.Clear(_frameDistribution, 0, _numBuckets);
			_prevTime = Time.realtimeSinceStartup;
			_count = 0;
		}

		public void Framemove()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			RecordDelta(realtimeSinceStartup - _prevTime);
			_prevTime = realtimeSinceStartup;
		}

		public void RecordDelta(float deltaTime)
		{
			int value = (int)((float)_numBuckets * deltaTime / _frameCap);
			_frameDistribution[Mathf.Clamp(value, 0, _numBuckets - 1)]++;
			_count++;
		}

		public float GetAverageFps()
		{
			float num = 0f;
			for (int i = 0; i < _frameDistribution.Length; i++)
			{
				num += ((float)i + 0.5f) * (float)_frameDistribution[i];
			}
			if (num * _frameCap == 0f)
			{
				return 0f;
			}
			return (float)(_numBuckets * _count) / (num * _frameCap);
		}

		public float GetAverageTime()
		{
			float num = 0f;
			for (int i = 0; i < _frameDistribution.Length; i++)
			{
				num += ((float)i + 0.5f) * (float)_frameDistribution[i];
			}
			if (_numBuckets * _count == 0)
			{
				return 0f;
			}
			return num * _frameCap / (float)(_numBuckets * _count);
		}

		public float GetBadFrameRatio()
		{
			if (_count == 0)
			{
				return 0f;
			}
			return 1f - (float)_frameDistribution[0] / (float)_count;
		}

		public int GetSuperSlowFrames()
		{
			return _frameDistribution[_frameDistribution.Length - 1];
		}

		public void OnGUI()
		{
			GUILayout.Label("AverageFps=" + GetAverageFps());
			GUILayout.Label("BadFrameRatio=" + GetBadFrameRatio());
			for (int i = 0; i < _frameDistribution.Length; i++)
			{
				if (_frameDistribution[i] > 0)
				{
					GUILayout.Label($"Frame{i}={_frameDistribution[i]}");
				}
			}
		}
	}

	private StringBuilder toggleLogBuilder = new StringBuilder();

	private string toggleLog = string.Empty;

	private DateTime lastTimelogSentToServer;

	public static bool IsLogEnabled;

	private int _logLimit = 10;

	private Queue<string> _logs = new Queue<string>();

	public float SetUserIdleAfterSeconds = 3f;

	private FrameRecorder _sceneFrameRecorder;

	private FrameRecorder _battleFrameRecorder;

	private FrameRecorder _battleLogRecorder;

	private List<float> _rttTimes;

	private float _rttAvg;

	private float _rttStdDiv;

	private List<float> _httpRttTimes;

	private int _httpRttAvg;

	private int _httpRttMax;

	private string _httpRttMaxName;

	public int _httpRttThreshold;

	private bool _battleInProgress;

	private string _sceneName = "";

	private string _topWindow = "";

	private string _chapterIndice = "";

	private float _lastReportTime;

	private long _peakMemory;

	private string _cpuArch = "";

	public int ProfileReportBasisPoint;

	private int _lazyUpdateCnt;

	private static List<float> _BattleBridge = new List<float>();

	private static float _lastBattleLogTime;

	private static List<float> _BattleLogBridge = new List<float>();

	private void Start()
	{
		RegisterToggleGUI();
		if (IsLogEnabled)
		{
			_sceneFrameRecorder = new FrameRecorder(1f, 30);
			_battleFrameRecorder = new FrameRecorder(0.5f, 30);
			_battleLogRecorder = new FrameRecorder(5f, 10);
			_rttTimes = new List<float>();
			_httpRttTimes = new List<float>();
			_cpuArch = DeviceHelper.GetCpuFamily();
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		if (IsLogEnabled)
		{
			if (!String_Extension_Method.IsNullOrEmpty(_sceneName))
			{
				AddLogAndBeginNewSession();
			}
			_sceneName = SceneManager.GetActiveScene().name;
			_chapterIndice = "";
			if (SingletonAuto<BattleContext>.isValidInstance)
			{
				_chapterIndice = $"{SingletonAuto<BattleContext>.instance.LastBattleChapterIndex}-{SingletonAuto<BattleContext>.instance.LastBattleDungeonIndex}";
			}
		}
	}

	private void AddProfilerLog()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		ClientLogItem clientLogItem = new ClientLogItem();
		ClientLogItem clientLogItem2 = clientLogItem;
		clientLogItem2.AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager?.Id ?? 0;
		clientLogItem2.Version = Application.version;
		clientLogItem2.DeviceModel = SystemInfo.deviceModel;
		clientLogItem2.CpuArch = _cpuArch;
		clientLogItem2.MemorySpec = Vespa_String_Extension_Method.GetEnumString(UnityCache.MemorySpec);
		clientLogItem2.TextureSpec = Vespa_String_Extension_Method.GetEnumString(SystemOptionData.Spec);
		clientLogItem2.ShaderLod = Shader.globalMaximumLOD;
		clientLogItem2.ShadowType = Vespa_String_Extension_Method.GetEnumString(SystemOptionData.Render_ShadowType);
		clientLogItem2.SceneName = _sceneName;
		if (_sceneName == "Lobby" && _topWindow != "")
		{
			clientLogItem2.TopWindow = _topWindow;
		}
		clientLogItem2.ChapterIndice = _chapterIndice;
		clientLogItem2.PeakMemoryMb = _peakMemory / 1000000;
		clientLogItem2.TimeRange = realtimeSinceStartup - _lastReportTime;
		clientLogItem2.FrameCount = _sceneFrameRecorder.Count;
		clientLogItem2.SceneFps = _sceneFrameRecorder.GetAverageFps();
		clientLogItem2.BadSceneFrameRatio = _sceneFrameRecorder.GetBadFrameRatio();
		if (_battleFrameRecorder.Count > 0)
		{
			clientLogItem2.BattleFps = _battleFrameRecorder.GetAverageFps();
			clientLogItem2.BadBattleFrameRatio = _battleFrameRecorder.GetBadFrameRatio();
			clientLogItem2.SuperSlowBattleFrames = _battleFrameRecorder.GetSuperSlowFrames();
		}
		if (_battleLogRecorder.Count > 0)
		{
			clientLogItem2.BattleLogUpdateTime = _battleLogRecorder.GetAverageTime();
			clientLogItem2.SuperSlowBattleLogs = _battleLogRecorder.GetSuperSlowFrames();
			if (_rttTimes.Count > 0)
			{
				clientLogItem2.RttAvgMs = _rttAvg;
				clientLogItem2.RttSd = _rttStdDiv;
			}
		}
		if (_httpRttTimes.Count > 0)
		{
			clientLogItem2.HttpRttAvgMs = _httpRttAvg;
			clientLogItem2.HttpRttMax = _httpRttMax;
			clientLogItem2.HttpRttMaxName = _httpRttMaxName;
		}
		AddLog(clientLogItem);
	}

	public void AddLog(ClientLogItem items)
	{
		string text = JsonMarshaler.EncodeJsonString(items);
		if (_logs.Count() >= _logLimit)
		{
			_logs.Dequeue();
		}
		AddToToggleLog("[Add Log] {0} at {1}\n", text, DateTime.Now);
		_logs.Enqueue(text);
	}

	public bool HasLogItem()
	{
		return _logs.Count() > 0;
	}

	public void SendLogToServer()
	{
		if (HasLogItem())
		{
			Request request = new Request();
			request.LogItems = _logs.ToArray();
			lastTimelogSentToServer = DateTime.Now;
			AddToToggleLog("==== Sent {0} log(s) to the server at {1} ====\n", _logs.Count(), lastTimelogSentToServer);
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.SilenceError, delegate(Response obj)
			{
				AddToToggleLog("Result : {0}\n\n", obj.Result);
			});
			_logs.Clear();
		}
	}

	private void AddLogAndBeginNewSession()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup - _lastReportTime > 15f && UnityEngine.Random.Range(0, 10000) < ProfileReportBasisPoint)
		{
			AddProfilerLog();
		}
		_sceneFrameRecorder.BeginNewRecord();
		_battleFrameRecorder.BeginNewRecord();
		_battleLogRecorder.BeginNewRecord();
		_lastBattleLogTime = 0f;
		_rttTimes.Clear();
		_peakMemory = 0L;
		_lastReportTime = realtimeSinceStartup;
	}

	private void CheckTopWindow()
	{
		if (!(_sceneName != "Lobby"))
		{
			BaseWindow topOpenedWindow = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopOpenedWindow(excludePopup: true);
			string text = (topOpenedWindow ? topOpenedWindow.name : "");
			if (text != _topWindow && (text == "" || _topWindow == ""))
			{
				AddLogAndBeginNewSession();
				_topWindow = text;
			}
		}
	}

	private void Update()
	{
		if (!IsLogEnabled)
		{
			return;
		}
		if (_lazyUpdateCnt > 10)
		{
			CheckTopWindow();
			_peakMemory = Math.Max(_peakMemory, Profiler.GetTotalAllocatedMemoryLong() + GC.GetTotalMemory(forceFullCollection: false));
			_lazyUpdateCnt = 0;
		}
		else
		{
			_lazyUpdateCnt++;
		}
		_sceneFrameRecorder.Framemove();
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		_battleInProgress = battleCache != null && battleCache.IsBattleStarted && !battleCache.IsBattleEnded && !GlobalTimer.pause;
		if (!_battleInProgress)
		{
			return;
		}
		lock (_BattleBridge)
		{
			foreach (float item in _BattleBridge)
			{
				_battleFrameRecorder.RecordDelta(item);
			}
			_BattleBridge.Clear();
		}
		lock (_BattleLogBridge)
		{
			foreach (float item2 in _BattleLogBridge)
			{
				_battleLogRecorder.RecordDelta(item2);
			}
			_BattleLogBridge.Clear();
		}
	}

	public static void RecordBattleDelta(float delta)
	{
		lock (_BattleBridge)
		{
			_BattleBridge.Add(delta);
		}
	}

	public static void RecordBattleLogDelta()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (_lastBattleLogTime > 0f)
		{
			lock (_BattleLogBridge)
			{
				_BattleLogBridge.Add(realtimeSinceStartup - _lastBattleLogTime);
			}
		}
		_lastBattleLogTime = realtimeSinceStartup;
	}

	public void RecordResponseTime(float time)
	{
		_rttTimes.Add(time);
		_rttAvg = _rttTimes.Average();
		float num = _rttTimes.Select((float val) => (val - _rttAvg) * (val - _rttAvg)).Sum();
		_rttStdDiv = Mathf.Sqrt(num / (float)_rttTimes.Count);
	}

	public void RecordHttpResponseTime(int time, string apiName)
	{
		_httpRttTimes.Add(time);
		_httpRttAvg = Mathf.RoundToInt(_httpRttTimes.Average());
		if ((double)time > (double)_httpRttAvg * ((double)_httpRttThreshold * 0.001) && time > _httpRttMax)
		{
			_httpRttMax = time;
			_httpRttMaxName = apiName;
		}
	}

	public void AddToToggleLog(string appendFormatString, params object[] args)
	{
		if (toggleLogBuilder.Length != 0)
		{
			toggleLogBuilder.Clear();
		}
		toggleLogBuilder.Append(toggleLog).AppendFormat(appendFormatString, args);
		toggleLog = toggleLogBuilder.ToString();
		toggleLogBuilder.Clear();
	}

	private void RegisterToggleGUI()
	{
		if (!IsLogEnabled)
		{
			GlobalAutoBehaviour<GuiManager>.instance.Add("ProfileManager", this, delegate
			{
				GUILayout.Label($"Profiler is disabled.");
			});
			return;
		}
		GlobalAutoBehaviour<GuiManager>.instance.Add("ProfileManager", this, delegate
		{
			long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
			long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
			float num = GlobalAutoBehaviour<GuiManager>.instance.DisplayWidth - 150f;
			float num2 = GlobalAutoBehaviour<GuiManager>.instance.DisplayHeight - 50f;
			using (new BlockManager().VerticalBox(Color.black))
			{
				using (new BlockManager().Horizontal())
				{
					if (GUILayout.Button("Add Log"))
					{
						AddProfilerLog();
					}
					if (GUILayout.Button("Send Log"))
					{
						SendLogToServer();
					}
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("ProfileReportBasisPoint");
					String_Extension_Method.TryToInt(GUILayout.TextField(ProfileReportBasisPoint.ToString()), out ProfileReportBasisPoint);
				}
				GUILayout.Label($"Enable Profiler Status={IsLogEnabled}");
				GUILayout.Label($"Current Log Count:{_logs.Count()}");
				GUILayout.Label($"Last Log Sent Time:{lastTimelogSentToServer.ToString()}");
				using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(num - 60f), GUILayout.Height(num2 - 150f)).Scroll(this, "Output", alwaysShowHorizontal: false, alwaysShowVertical: true))
				{
					GUILayout.Label(toggleLog);
				}
			}
			GUILayout.Label($"Device Model={SystemInfo.deviceModel} ({_cpuArch})");
			GUILayout.Label("ScriptingRuntime=" + DeviceHelper.GetScriptingBackend());
			GUILayout.Label("MemorySpec=" + Vespa_String_Extension_Method.GetEnumString(UnityCache.MemorySpec));
			GUILayout.Label($"Total={Vespa_Long_Extension_Method.ToSizeString(totalAllocatedMemoryLong + totalMemory)}, Unity Alloc={Vespa_Long_Extension_Method.ToSizeString(totalAllocatedMemoryLong)}, Mono Heap={Vespa_Long_Extension_Method.ToSizeString(totalMemory)}");
			GUILayout.Label("Scene=" + _sceneName);
			GUILayout.Label("TopWindow=" + _topWindow);
			GUILayout.Label("ChapterIndice=" + _chapterIndice);
			if (_rttTimes.Count > 0)
			{
				GUILayout.Label("---------Online Battle Log (Max 5s)---------");
				GUILayout.Label($"RTT avg={_rttAvg} sd={_rttStdDiv}");
				lock (_battleLogRecorder)
				{
					_battleLogRecorder.OnGUI();
				}
			}
			GUILayout.Label("---------Battle Frame (Max 0.5s)---------");
			lock (_battleFrameRecorder)
			{
				_battleFrameRecorder.OnGUI();
			}
			GUILayout.Label("---------Render Frame (Max 1s)---------");
			_sceneFrameRecorder.OnGUI();
		});
	}
}
