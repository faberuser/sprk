using System.Collections.Generic;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NGame2.NUtil;

public class FrameManager : GlobalAutoBehaviour<FrameManager>
{
	private const string DEFAULT_FRAME = "Default";

	private const int FRAME_DATA_STACK_POSITION = 1;

	private List<FrameControlData> _frameControlDataStack = new List<FrameControlData>(10);

	private FrameControlData defaultFrameData = new FrameControlData
	{
		Index = 0,
		Name = "Default",
		ContentType = EContentType.Scene,
		DelayType = EDelayType.Immediate,
		Auto = 60,
		AutoPlus = 60
	};

	private FrameControlDataContainer _frameTable;

	private GUILayoutOption width50 = GUILayout.Width(50f);

	private GUILayoutOption width70 = GUILayout.Width(70f);

	private GUILayoutOption width100 = GUILayout.Width(100f);

	private GUILayoutOption width200 = GUILayout.Width(200f);

	public static int DeviceRefreshRate { get; private set; }

	public bool IsFrameDataTableLoaded => SingletonAuto<TableDataManager>.instance.IsLoaded();

	public bool IsAutoFrameEnabled => SystemOptionData.Graphic_IsAutoFrameEnabled;

	public bool CanAdjustAutoFrame
	{
		get
		{
			if (IsFrameDataTableLoaded)
			{
				return IsAutoFrameEnabled;
			}
			return false;
		}
	}

	private FrameControlDataContainer FrameTable
	{
		get
		{
			if (_frameTable == null)
			{
				_frameTable = GameRoot.Instance.FrameControlDataContainer;
			}
			return _frameTable;
		}
	}

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("Frame Controller", this, DrawGui);
		Init();
	}

	private void Init()
	{
		DeviceRefreshRate = Screen.currentResolution.refreshRate;
		SceneManager.sceneLoaded += OnSceneLoad;
	}

	private void DrawGui()
	{
		using (new BlockManager().Vertical())
		{
			DrawVSyncToggleButton();
			GUILayout.Space(10f);
			DrawApplicationFrameSetting();
			GUILayout.Space(10f);
			DrawGameOptionFrameSetting();
			GUILayout.Space(15f);
			DrawTargetFrameController();
			GUILayout.Space(15f);
			DrawFrameStack();
		}
	}

	private void DrawApplicationFrameSetting()
	{
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Current resolution: ");
			GUILayout.Label(Screen.currentResolution.width + "X" + Screen.currentResolution.height, width200);
			GUILayout.FlexibleSpace();
		}
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Device rendering rate: ");
			GUILayout.Label(Screen.currentResolution.refreshRate.ToString(), width200);
			GUILayout.FlexibleSpace();
		}
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Current Target Frame Rate: ");
			GUILayout.Label(Application.targetFrameRate.ToString(), width200);
			GUILayout.FlexibleSpace();
		}
	}

	private void DrawGameOptionFrameSetting()
	{
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Frame auto control mode: ");
			GUILayout.Label(SystemOptionData.Graphic_IsAutoFrameEnabled ? "enable" : "false", width200);
			GUILayout.FlexibleSpace();
		}
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("GameOption FrameRateSetting: ");
			GUILayout.Label(SystemOptionData.Graphic_FrameControl.ToString(), width200);
			GUILayout.FlexibleSpace();
		}
	}

	private void DrawTargetFrameController()
	{
		using (new BlockManager().Horizontal())
		{
			GUILayout.Label("Set Frame rate", width100);
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("Default", width100))
			{
				Application.targetFrameRate = -1;
			}
			if (GUILayout.Button("30", width50))
			{
				Application.targetFrameRate = 30;
			}
			if (GUILayout.Button("60", width50))
			{
				Application.targetFrameRate = 60;
			}
			if (GUILayout.Button("120", width50))
			{
				Application.targetFrameRate = 120;
			}
			if (GUILayout.Button("240", width50))
			{
				Application.targetFrameRate = 240;
			}
		}
	}

	private void DrawVSyncToggleButton()
	{
		using (new BlockManager().Vertical())
		{
			if (QualitySettings.vSyncCount == 0)
			{
				if (GUILayout.Button("VSync is Off", width100))
				{
					SetVSyncCount(1);
				}
			}
			else if (GUILayout.Button("VSync is On", width100))
			{
				SetVSyncCount(0);
			}
		}
	}

	private void DrawFrameStack()
	{
		using (new BlockManager().Vertical())
		{
			foreach (FrameControlData item in _frameControlDataStack)
			{
				if (item != null)
				{
					GUILayout.Label("[" + item.Name + "]: " + ((SystemOptionData.Graphic_FrameControl == EFrameControl.Auto) ? item.Auto : item.AutoPlus));
				}
			}
		}
	}

	private void OnSceneLoad(Scene scene, LoadSceneMode mode)
	{
		FrameStackReset();
	}

	public void FrameStackReset()
	{
		_frameControlDataStack.Clear();
		_frameControlDataStack.Add(defaultFrameData);
		if (CanAdjustAutoFrame)
		{
			string key = SceneManager.GetActiveScene().name;
			FrameControlData data = FrameTable.GetData(key);
			if (data != null)
			{
				_frameControlDataStack.Add(data);
			}
		}
		SetTargetFrameAuto();
	}

	public void PushContentFrameInfo(string contentName)
	{
		if (CanAdjustAutoFrame)
		{
			FrameControlData data = FrameTable.GetData(contentName);
			if (data != null && !IsContain(contentName))
			{
				_frameControlDataStack.Add(data);
				SetTargetFrameAuto();
			}
		}
	}

	public void PopContentFrameInfo(string contentName)
	{
		if (CanAdjustAutoFrame)
		{
			int indexOfFrameData = GetIndexOfFrameData(contentName);
			if (indexOfFrameData != -1 && Vespa_Generic_IList_Extension_Method.IsValidIndex(_frameControlDataStack, indexOfFrameData))
			{
				_frameControlDataStack.RemoveAt(indexOfFrameData);
				SetTargetFrameAuto();
			}
		}
	}

	private void SetTargetFrameAuto()
	{
		FrameControlData last = Vespa_Generic_IList_Extension_Method.GetLast(_frameControlDataStack);
		switch (SystemOptionData.Graphic_FrameControl)
		{
		case EFrameControl.Auto:
			// "Auto" means the display rate. Desktop gets it from VSync; mobile ignores VSync
			// and treats -1 as 30 fps, so ask for the display's fastest refresh rate there.
			Application.targetFrameRate = Application.isMobilePlatform ? Mathf.Max(30, ApplyDisplayRefreshRate(0)) : (-1);
			QualitySettings.vSyncCount = 1;
			break;
		case EFrameControl.AutoPlus:
			Application.targetFrameRate = last.AutoPlus;
			ApplyDisplayRefreshRate(last.AutoPlus);
			break;
		default:
			Application.targetFrameRate = (int)SystemOptionData.Graphic_FrameControl;
			ApplyDisplayRefreshRate(Application.targetFrameRate);
			break;
		}
	}

	private static int _requestedDisplayModeId;

	// SPRK: Android keeps the panel at its default mode (often 60 Hz on 90/120/144 Hz
	// devices; MIUI also drops to 60 Hz when no app asks for more) unless the app requests
	// a mode, so a 144 fps choice could never pass 60. Request, through the window's
	// preferredDisplayModeId, the slowest display mode of the current size that can show
	// targetFps (at least 60 Hz, so touch stays responsive); targetFps <= 0 or above every
	// mode asks for the fastest mode. (Screen.SetResolution with a refresh rate sends no
	// request on such devices.) Returns that mode's refresh rate, or targetFps off Android.
	public static int ApplyDisplayRefreshRate(int targetFps)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return targetFps;
		}
		try
		{
			using AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
			using AndroidJavaObject windowManager = activity.Call<AndroidJavaObject>("getWindowManager");
			using AndroidJavaObject display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay");
			using AndroidJavaObject current = display.Call<AndroidJavaObject>("getMode");
			int width = current.Call<int>("getPhysicalWidth");
			int height = current.Call<int>("getPhysicalHeight");
			int wanted = Mathf.Max(targetFps, 60);
			int chosenId = 0, fastestId = 0;
			float chosenRate = 0f, fastestRate = 0f;
			foreach (AndroidJavaObject mode in display.Call<AndroidJavaObject[]>("getSupportedModes"))
			{
				using (mode)
				{
					if (mode.Call<int>("getPhysicalWidth") != width || mode.Call<int>("getPhysicalHeight") != height)
					{
						continue;
					}
					int id = mode.Call<int>("getModeId");
					float rate = mode.Call<float>("getRefreshRate");
					if (rate > fastestRate)
					{
						fastestRate = rate;
						fastestId = id;
					}
					if (targetFps > 0 && rate + 0.5f >= wanted && (chosenId == 0 || rate < chosenRate))
					{
						chosenRate = rate;
						chosenId = id;
					}
				}
			}
			if (chosenId == 0)
			{
				chosenId = fastestId;
				chosenRate = fastestRate;
			}
			if (chosenId == 0)
			{
				return Mathf.Max(targetFps, 60);
			}
			if (chosenId != _requestedDisplayModeId)
			{
				_requestedDisplayModeId = chosenId;
				int modeId = chosenId;
				// Window attributes belong to the UI thread.
				activity.Call("runOnUiThread", new AndroidJavaRunnable(delegate
				{
					using AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");
					using AndroidJavaObject attributes = window.Call<AndroidJavaObject>("getAttributes");
					attributes.Set("preferredDisplayModeId", modeId);
					window.Call("setAttributes", attributes);
				}));
			}
			return Mathf.RoundToInt(chosenRate);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning("SPRK display refresh: " + e.Message);
			return Mathf.Max(targetFps, 60);
		}
	}

	private void SetVSyncCount(int vSyncCount)
	{
		QualitySettings.vSyncCount = vSyncCount;
	}

	protected override void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoad;
		_frameControlDataStack.Clear();
		base.OnDestroy();
	}

	private int GetIndexOfFrameData(string contentName)
	{
		int result = -1;
		for (int i = 1; i < __Extension_Method_ICollection__.COUNT(_frameControlDataStack); i++)
		{
			FrameControlData frameControlData = _frameControlDataStack[i];
			if (frameControlData != null && !String_Extension_Method.IsNullOrEmpty(frameControlData.Name) && frameControlData.Name == contentName)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private bool IsContain(string contentName)
	{
		return GetIndexOfFrameData(contentName) != -1;
	}
}
