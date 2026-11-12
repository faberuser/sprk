using System;
using System.Collections.Generic;
using NGame2.NBattleSimulator;
using NGame2.NDevice;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUtil;
using NGame2.NUtil.NQA;
using NVespa.NGlobal.NGuiManager;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NGlobal;

public class GuiManager : GlobalAutoBehaviour<GuiManager>
{
	private class FPSChecker
	{
		public float m_FPS;

		public int m_prevFrameCount;

		public float m_prevTime;

		public float m_timeLength;

		public int m_accumulateFrameCount;

		public float m_accumulateTime;

		public FPSChecker(float i_timeLength)
		{
			m_prevTime = Time.realtimeSinceStartup;
			m_timeLength = i_timeLength;
			m_prevFrameCount = Time.frameCount;
		}

		public void Framemove()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - m_prevTime;
			if (!(num < m_timeLength))
			{
				int num2 = Time.frameCount - m_prevFrameCount;
				m_FPS = (float)num2 / num;
				m_prevFrameCount = Time.frameCount;
				m_prevTime = realtimeSinceStartup;
			}
		}

		public void Accumulate(bool activated)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - m_prevTime;
			int num2 = Time.frameCount - m_prevFrameCount;
			if (activated)
			{
				m_accumulateFrameCount += num2;
				m_accumulateTime += num;
				if (m_accumulateTime > 0f)
				{
					m_FPS = (float)m_accumulateFrameCount / m_accumulateTime;
				}
			}
			m_prevFrameCount = Time.frameCount;
			m_prevTime = realtimeSinceStartup;
		}
	}

	private class GuiAction
	{
		public WeakReference Owner;

		public Action DrawGui;
	}

	private class TouchDisabler : ITouchDisabler
	{
		public bool AddThis;

		public bool IsValid => true;
	}

	public int testCount = 1000000;

	public bool FoldoutGui;

	public string CurrentActionName = string.Empty;

	public LocalStorageBool RunInBackground = new LocalStorageBool(i_default: false, "GlobalSetting_RunInBackground");

	private LocalStorageBool _enableGui = new LocalStorageBool(i_default: false, typeof(GuiManager).FullName + "_enableGui");

	private LocalStorageBool _enableOverlay = new LocalStorageBool(i_default: false, typeof(GuiManager).FullName + "_enableOverlay");

	private LocalStorageBool _blockBg = new LocalStorageBool(i_default: false, typeof(GuiManager).FullName + "_blockBg");

	private LocalStorageFloat _guiScale = new LocalStorageFloat(1f, typeof(GuiManager).FullName + "_guiScale");

	private string devConsoleButtonLabel = string.Empty;

	private FPSChecker _shortFPS;

	private FPSChecker _longFPS;

	private IncreaseDic<string, List<GuiAction>> _dic = new IncreaseDic<string, List<GuiAction>>();

	private Action _drawOverlayGUIAction;

	private Action _onUnsubscribeOveryGUIAction;

	private bool _isInit;

	private static TouchDisabler _touchDisabler = new TouchDisabler();

	public float DisplayWidth
	{
		get
		{
			int num = Mathf.Min(Screen.width, Screen.height);
			return 400f * (float)Screen.width / (float)num / GuiSclae;
		}
	}

	public float DisplayHeight
	{
		get
		{
			int num = Mathf.Min(Screen.width, Screen.height);
			return 400f * (float)Screen.height / (float)num / GuiSclae;
		}
	}

	public bool EnableOverlay => _enableOverlay.Value;

	private float GuiSclae
	{
		get
		{
			return _guiScale.Value;
		}
		set
		{
			_guiScale.Value = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_shortFPS = new FPSChecker(0.5f);
		_longFPS = new FPSChecker(3f);
		string cpuFamily = DeviceHelper.GetCpuFamily();
		devConsoleButtonLabel = "Application Architecture: " + cpuFamily;
	}

	private void Start()
	{
		if (_enableGui.Value)
		{
			Init();
		}
	}

	public void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			Add("", this, delegate
			{
				DrawCommonGui();
			});
			GameConsole.InitConsole();
			GlobalAutoBehaviour<ClientValueManager>.CreateInstance();
			GlobalAutoBehaviour<ContentValueViewer>.CreateInstance();
			GlobalAutoBehaviour<MarketingUIOffManager>.CreateInstance();
			GlobalAutoBehaviour<BattleSimulationViewer>.CreateInstance();
			GlobalAutoBehaviour<QAManager>.CreateInstance();
			GlobalAutoBehaviour<FrameManager>.CreateInstance();
			GlobalAutoBehaviour<WebViewManager>.CreateInstance();
			GlobalAutoBehaviour<BattleThreadDebugger>.CreateInstance();
		}
	}

	private void OnGUI()
	{
		if (!_enableGui.Value)
		{
			return;
		}
		int num = Mathf.Min(Screen.width, Screen.height);
		float num2 = GuiSclae * (float)num / 400f;
		GUI.matrix = Matrix4x4.Scale(Vector3.one * num2);
		using (new BlockManager().Horizontal())
		{
			if (_shortFPS != null && _longFPS != null)
			{
				GUILayout.Box($"FPS={(int)_shortFPS.m_FPS},{(int)_longFPS.m_FPS} ({Application.targetFrameRate})", GUILayout.Width(150f));
			}
			if (GUILayout.Button("<<Toggle>>", GUILayout.Width(100f), GUILayout.Height(30f)))
			{
				FoldoutGui = !FoldoutGui;
				return;
			}
			if (!FoldoutGui)
			{
				if (_enableOverlay.Value)
				{
					GUILayout.FlexibleSpace();
					DrawOverlayGUI();
				}
				return;
			}
			if (GUILayout.Button("-", GUILayout.Width(30f), GUILayout.Height(30f)))
			{
				GuiSclae *= 0.8f;
			}
			else if (GUILayout.Button("+", GUILayout.Width(30f), GUILayout.Height(30f)))
			{
				GuiSclae *= 1.25f;
			}
			_blockBg.Value = GUILayout.Toggle(_blockBg.Value, "Block Bg");
		}
		using (new BlockManager().Vertical())
		{
			DrawGUI();
		}
	}

	private void DrawCommonGui()
	{
		GUILayout.Box(devConsoleButtonLabel);
		if (GUILayout.Button("Hide This"))
		{
			_enableGui.Value = false;
		}
		if (GUILayout.Button("Toggle Overlay UI: " + _enableOverlay.Value))
		{
			_enableOverlay.Value = !_enableOverlay.Value;
			if (!_enableOverlay.Value)
			{
				GlobalAutoBehaviour<GuiManager>.instance.RemoveOverlay();
			}
		}
		string text = (UICamera.DisableEventHandler ? "Enable UI Event Handler" : "Disable UI Event Handler");
		UICamera.DisableEventHandler = GUILayout.Toggle(UICamera.DisableEventHandler, text, GUI.skin.button);
		if (GUILayout.Button("Load EmptyScene"))
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Empty);
		}
		if (GUILayout.Button("Load LoginScene"))
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Login);
		}
		if (GUILayout.Button("Load LobbyScene"))
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
		}
	}

	public void SetEnable(bool enabled)
	{
		_enableGui.Value = enabled;
		if (enabled)
		{
			Init();
		}
	}

	public static bool IsShowDebugButton()
	{
		if (!GlobalAutoBehaviour<GuiManager>.isValidInstance)
		{
			return false;
		}
		return GlobalAutoBehaviour<GuiManager>.instance.IsEnable();
	}

	public bool IsEnable()
	{
		return _enableGui.Value;
	}

	public void Add(string actionName, object owner, Action guiAction)
	{
		if (owner != null)
		{
			actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
			Remove(actionName, owner);
			GetGUIAction(actionName).Add(new GuiAction
			{
				Owner = new WeakReference(owner),
				DrawGui = guiAction
			});
		}
	}

	public void Remove(string actionName, object owner)
	{
		if (owner == null)
		{
			return;
		}
		actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
		List<GuiAction> vALUE = _dic.GETVALUE(actionName, null);
		if (vALUE == null)
		{
			return;
		}
		for (int i = 0; i < vALUE.Count; i++)
		{
			if (vALUE[i].Owner.Target == owner)
			{
				vALUE.RemoveAt(i);
				break;
			}
		}
	}

	public void AddOverlay(string label, Action drawOverlay, Action onUnsubscribeOverlay)
	{
		Vespa_System_Action_Extension_Method.fire(_onUnsubscribeOveryGUIAction);
		_drawOverlayGUIAction = drawOverlay;
		_onUnsubscribeOveryGUIAction = onUnsubscribeOverlay;
	}

	public void RemoveOverlay()
	{
		_drawOverlayGUIAction = null;
		Vespa_System_Action_Extension_Method.fire(_onUnsubscribeOveryGUIAction);
		_onUnsubscribeOveryGUIAction = null;
	}

	private PoolingList<string> CollectActionNameList()
	{
		PoolingList<string> actionNameList = default(PoolingList<string>);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_dic); i++)
		{
			actionNameList.Add(_dic.GetKeyByIndex(i));
		}
		if (SceneAutoBehaviour<NVespa.NScene.GuiManager>.isValidInstance)
		{
			SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.CollectView(ref actionNameList);
		}
		actionNameList.Sort((string i_a, string i_b) => i_a.CompareTo(i_b));
		return actionNameList;
	}

	private void DrawGUI()
	{
		using (new BlockManager().Horizontal())
		{
			float width = 150f;
			float height = DisplayHeight - 35f;
			using (PoolingList<string> poolingList = CollectActionNameList())
			{
				using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(width), GUILayout.Height(height)).Scroll(this, "ListScroll", alwaysShowHorizontal: false, alwaysShowVertical: false))
				{
					for (int i = 0; i < poolingList.Count; i++)
					{
						string text = poolingList[i];
						bool num = text.Equals(CurrentActionName);
						bool flag = GUILayout.Toggle(num, text);
						if (!num && flag)
						{
							CurrentActionName = text;
						}
					}
				}
			}
			float width2 = DisplayWidth - 160f;
			float height2 = DisplayHeight - 35f;
			using (new BlockManager().Vertical(GUI.skin.box, GUILayout.Width(width2), GUILayout.Height(height2)).Scroll(this, "MainScroll", alwaysShowHorizontal: false, alwaysShowVertical: false))
			{
				List<GuiAction> gUIActionValid = GetGUIActionValid(CurrentActionName);
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(gUIActionValid); j++)
				{
					GuiAction guiAction = gUIActionValid[j];
					if (!Vespa_Common_Class_Extension_Method.IsUnityNull(guiAction.Owner.Target))
					{
						Vespa_System_Action_Extension_Method.fire(guiAction.DrawGui);
					}
				}
				if (!SceneAutoBehaviour<NVespa.NScene.GuiManager>.isValidInstance)
				{
					return;
				}
				List<NVespa.NScene.GuiManager.GuiAction> gUIActionValid2 = SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.GetGUIActionValid(CurrentActionName);
				for (int k = 0; k < __Extension_Method_ICollection__.COUNT(gUIActionValid2); k++)
				{
					NVespa.NScene.GuiManager.GuiAction guiAction2 = gUIActionValid2[k];
					if (!Vespa_Common_Class_Extension_Method.IsUnityNull(guiAction2.Owner.Target))
					{
						Vespa_System_Action_Extension_Method.fire(guiAction2.DrawGui);
					}
				}
			}
		}
	}

	private void DrawOverlayGUI()
	{
		if (_drawOverlayGUIAction == null)
		{
			if (GUILayout.Button("Click to disable overlay"))
			{
				_enableOverlay.Value = false;
			}
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(_drawOverlayGUIAction);
		}
	}

	private List<GuiAction> GetGUIActionValid(string actionName)
	{
		actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
		return _dic.GETVALUE(actionName, null);
	}

	private List<GuiAction> GetGUIAction(string actionName)
	{
		actionName = (String_Extension_Method.IsNullOrEmpty(actionName) ? "(default)" : actionName);
		if (_dic.TryGetValue(actionName, out var o_value))
		{
			return o_value;
		}
		o_value = new List<GuiAction>();
		_dic.Add(actionName, o_value);
		return o_value;
	}

	private void Update()
	{
		EnableBlockBg(_blockBg.Value && FoldoutGui && _enableGui.Value);
		if (_shortFPS != null)
		{
			_shortFPS.Framemove();
		}
		if (_longFPS != null)
		{
			_longFPS.Framemove();
		}
		for (int i = 0; i < _dic.Count; i++)
		{
			List<GuiAction> valueByIndex = _dic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				if (Vespa_Common_Class_Extension_Method.IsUnityNull(valueByIndex[j].Owner.Target))
				{
					valueByIndex.RemoveAt(j);
					j--;
				}
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(valueByIndex))
			{
				_dic.RemoveByIndex(i);
				i--;
			}
		}
	}

	private void EnableBlockBg(bool enable)
	{
		if (enable)
		{
			if (!_touchDisabler.AddThis)
			{
				GlobalTopmostWindow.instance.AddTouchDisabler(_touchDisabler);
				_touchDisabler.AddThis = true;
			}
		}
		else if (_touchDisabler.AddThis)
		{
			GlobalTopmostWindow.instance.RemoveTouchDisabler(_touchDisabler);
			_touchDisabler.AddThis = false;
		}
	}
}
