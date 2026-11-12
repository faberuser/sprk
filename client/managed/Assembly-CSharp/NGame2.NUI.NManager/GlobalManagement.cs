using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class GlobalManagement : GlobalAutoBehaviour<GlobalManagement>, InitOnCreate
{
	private static LocalStorageBool _openedListFoldout = new LocalStorageBool(i_default: false, typeof(GlobalManagement).FullName + "_openedListFoldout");

	private static LocalStorageBool _openedWindowStackFoldout = new LocalStorageBool(i_default: false, typeof(GlobalManagement).FullName + "_openedWindowStackFoldout");

	private static LocalStorageString _centerPopupMessage = new LocalStorageString(string.Empty, typeof(GlobalManagement).FullName + "_centerPopupMessage");

	private List<IManagementWrapper> _managementList = new List<IManagementWrapper>();

	private List<BaseWindow> _openedWindowStackList = new List<BaseWindow>();

	private List<Action> _depthReorderedCallbackList = new List<Action>();

	private bool _reorderRequested;

	public List<BaseWindow> OpenedWindowList => _openedWindowStackList;

	void InitOnCreate.InitializeSingleton()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("GlobalManagement", this, delegate
		{
			GUILayout.Label("LoginId " + GlobalAutoBehaviour<AccountManager>.instance.LoginId);
			GUILayout.Label("FaceBookId " + GlobalAutoBehaviour<AccountManager>.instance.FaceBookId);
			GUILayout.Label("GooglePlusId " + GlobalAutoBehaviour<AccountManager>.instance.GooglePlusId);
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Center Popup");
				_centerPopupMessage.Value = GUILayout.TextField(_centerPopupMessage.Value);
				if (GUILayout.Button("Open", GUILayout.Width(60f)) && !String_Extension_Method.IsNullOrEmpty(_centerPopupMessage.Value))
				{
					CenterMessage.Open(_centerPopupMessage.Value);
				}
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Global Center");
				_centerPopupMessage.Value = GUILayout.TextField(_centerPopupMessage.Value);
				if (GUILayout.Button("Open", GUILayout.Width(60f)) && !String_Extension_Method.IsNullOrEmpty(_centerPopupMessage.Value))
				{
					GlobalCenterMessage.Open(_centerPopupMessage.Value);
				}
			}
			GUILayout.Label("");
			if (GUILayout.Button("Close All Windows"))
			{
				ForEach(delegate(IManagementWrapper managementWrapper)
				{
					if (!managementWrapper.IsPersistent())
					{
						_ = managementWrapper.GetType().Name;
						managementWrapper.CloseAll(immediately: false);
					}
				});
			}
			_openedListFoldout.Value = GUILayout.Toggle(_openedListFoldout.Value, "Opened Window");
			if (_openedListFoldout.Value)
			{
				ForEach(delegate(IManagementWrapper managementWrapper)
				{
					if (managementWrapper.IsPersistent())
					{
						return;
					}
					using PoolingList<PrefabWrapper> poolingList = managementWrapper.GetOpenedList();
					for (int i = 0; i < poolingList.COUNT(); i++)
					{
						PrefabWrapper prefabWrapper = poolingList[i];
						if (prefabWrapper != null && prefabWrapper.Window != null)
						{
							GUILayout.Label(prefabWrapper.Window.GetType().Name);
						}
					}
				});
			}
			_openedWindowStackFoldout.Value = GUILayout.Toggle(_openedWindowStackFoldout.Value, "Opened Window Stack");
			if (_openedWindowStackFoldout.Value)
			{
				for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_openedWindowStackList); num++)
				{
					BaseWindow baseWindow = _openedWindowStackList[num];
					GUILayout.Label($"{num}.{baseWindow.GetType().Name}");
				}
			}
		});
	}

	public bool ShowLobbyMenu()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			if (!_managementList[i].ShowLobbyMenu())
			{
				return false;
			}
		}
		return true;
	}

	public void Open(IManagementWrapper management)
	{
		if (management != null && Find(management) == null)
		{
			_managementList.Add(management);
		}
	}

	public void Close(IManagementWrapper management)
	{
		if (management != null)
		{
			_managementList.Remove(management);
			SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		}
	}

	public void CloseAllWindows(bool immediately = false)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			_managementList[i].CloseAll(immediately);
		}
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
	}

	public void ForEach(Action<IManagementWrapper> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			IManagementWrapper i_argA = _managementList[i];
			Vespa_System_Action_Extension_Method.fire(action, i_argA);
		}
	}

	public bool AnyOpenedWindowBut(params IManagementWrapper[] exception)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			IManagementWrapper managementWrapper = _managementList[i];
			if (managementWrapper != null && (__Extension_Method_ICollection__.IsNullOrEmpty(exception) || !exception.Contains(managementWrapper)) && managementWrapper.IsOpenedAny())
			{
				return true;
			}
		}
		return false;
	}

	public bool WindowOpened<T>() where T : Component, IWindow
	{
		T window = GetWindow<T>();
		if (window == null)
		{
			return false;
		}
		return window.IsOpen();
	}

	public T GetWindow<T>(Func<T, bool> selector = null) where T : Component, IWindow
	{
		Type typeFromHandle = typeof(T);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			T val = _managementList[i].GetInstance(typeFromHandle) as T;
			if (!(val == null) && (selector == null || selector(val)))
			{
				return val;
			}
		}
		return null;
	}

	public bool WindowOpenedUI3DUI()
	{
		bool flag = false;
		foreach (BaseWindow openedWindowStack in _openedWindowStackList)
		{
			flag |= openedWindowStack.UseUI3DUILayer;
			if (flag)
			{
				return true;
			}
		}
		return flag;
	}

	public BaseWindow GetTopOpenedWindow(bool excludePopup = false)
	{
		for (int num = _openedWindowStackList.Count - 1; num >= 0; num--)
		{
			if (!excludePopup || !GlobalAutoBehaviour<PopupManegement>.instance.Exists(_openedWindowStackList[num]))
			{
				return _openedWindowStackList[num];
			}
		}
		return null;
	}

	public IWindowShadowShader GetTopOpendWindwShadowShader()
	{
		if (__Extension_Method_ICollection__.COUNT(_openedWindowStackList) == 0)
		{
			return null;
		}
		for (int num = _openedWindowStackList.Count - 1; num >= 0; num--)
		{
			if (!GlobalAutoBehaviour<PopupManegement>.instance.Exists(_openedWindowStackList[num]))
			{
				return _openedWindowStackList[num].GetWindowShadowShader();
			}
		}
		return null;
	}

	private IManagementWrapper Find(IManagementWrapper management)
	{
		return _managementList.FirstOrDefault((IManagementWrapper e) => e == management);
	}

	public IWindow FindWindow(Func<IWindow, bool> func)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			PoolingList<PrefabWrapper> openedList = _managementList[i].GetOpenedList();
			for (int j = 0; j < openedList.COUNT(); j++)
			{
				IWindow window = openedList[j].Window;
				if (Vespa_System_Action_Extension_Method.fire(func, window))
				{
					return window;
				}
			}
		}
		return null;
	}

	public void AddDepthReorderedCallback(Action onReorder)
	{
		if (!_depthReorderedCallbackList.Contains(onReorder))
		{
			_depthReorderedCallbackList.Add(onReorder);
		}
	}

	public void RemoveDepthReorderedCallback(Action onReorder)
	{
		_depthReorderedCallbackList.Remove(onReorder);
	}

	public void ReorderDepth(bool immediately = false)
	{
		if (!immediately)
		{
			_reorderRequested = true;
			return;
		}
		_reorderRequested = false;
		if (__Extension_Method_ICollection__.COUNT(_managementList) == 0)
		{
			return;
		}
		List<IManagementWrapper> list = new List<IManagementWrapper>();
		List<PrefabWrapper> list2 = new List<PrefabWrapper>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
		{
			IManagementWrapper managementWrapper = _managementList[i];
			if (managementWrapper != null)
			{
				managementWrapper.ResetDepth();
				list.Add(managementWrapper);
				using PoolingList<PrefabWrapper> poolingList = managementWrapper.GetOpenedList();
				list2.ADDRANGE(poolingList);
			}
		}
		list.Sort(delegate(IManagementWrapper l, IManagementWrapper r)
		{
			PoolingList<PrefabWrapper> openedList = l.GetOpenedList();
			PoolingList<PrefabWrapper> openedList2 = r.GetOpenedList();
			int a = int.MinValue;
			int num4 = int.MinValue;
			for (int j = 0; j < openedList.COUNT(); j++)
			{
				BaseWindow baseWindow = openedList[j].Window as BaseWindow;
				if (baseWindow != null)
				{
					a = Mathf.Max(a, baseWindow.DepthScore);
				}
			}
			for (int k = 0; k < openedList2.COUNT(); k++)
			{
				BaseWindow baseWindow2 = openedList2[k].Window as BaseWindow;
				if (baseWindow2 != null)
				{
					num4 = Mathf.Max(num4, baseWindow2.DepthScore);
				}
			}
			return a.CompareTo(num4);
		});
		int baseDepth = 10;
		EnumLookup<EDepthOrder>.ValueData[] valueArray = EnumLookup<EDepthOrder>.ValueArray;
		for (int num = 0; num < valueArray.Length; num++)
		{
			EDepthOrder enumValue = valueArray[num].EnumValue;
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				list[num2]?.ReorderDepth(enumValue, ref baseDepth);
			}
		}
		HideInvisibleWindows(list2);
		GameUIRoot.instance.BgBlack.UpdateDepth();
		GlobalAutoBehaviour<PopupManegement>.instance.ReorderDepth(baseDepth);
		for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(_depthReorderedCallbackList); num3++)
		{
			Vespa_System_Action_Extension_Method.fire(_depthReorderedCallbackList[num3]);
		}
	}

	private void HideInvisibleWindows(List<PrefabWrapper> allOpenedWindowList)
	{
		int num = int.MinValue;
		int num2 = int.MinValue;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allOpenedWindowList); i++)
		{
			BaseWindow baseWindow = allOpenedWindowList[i].Window as BaseWindow;
			if (!(baseWindow == null) && !baseWindow.IsWindowAnimating())
			{
				int basePanelDepth = baseWindow.BasePanelDepth;
				if (baseWindow.Side == EWindowSide.Both)
				{
					num = Mathf.Max(num, basePanelDepth);
					num2 = Mathf.Max(num2, basePanelDepth);
				}
				else if (baseWindow.Side == EWindowSide.Left)
				{
					num = Mathf.Max(num, basePanelDepth);
				}
				else if (baseWindow.Side == EWindowSide.Right)
				{
					num2 = Mathf.Max(num2, basePanelDepth);
				}
				else
				{
					_ = baseWindow.Side;
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(allOpenedWindowList); j++)
		{
			BaseWindow baseWindow2 = allOpenedWindowList[j].Window as BaseWindow;
			if (baseWindow2 == null)
			{
				continue;
			}
			int basePanelDepth2 = baseWindow2.BasePanelDepth;
			bool show;
			if (baseWindow2.Side == EWindowSide.Both)
			{
				show = basePanelDepth2 >= num || basePanelDepth2 >= num2;
			}
			else if (baseWindow2.Side == EWindowSide.Left)
			{
				show = basePanelDepth2 >= num;
			}
			else if (baseWindow2.Side == EWindowSide.Right)
			{
				show = basePanelDepth2 >= num2;
			}
			else
			{
				if (baseWindow2.Side != EWindowSide.None)
				{
					continue;
				}
				show = true;
			}
			baseWindow2.ShowRoot(show);
		}
	}

	public bool HasHideWorldMapWindow()
	{
		bool result = false;
		using (PoolingList<PrefabWrapper> poolingList = default(PoolingList<PrefabWrapper>))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_managementList); i++)
			{
				IManagementWrapper managementWrapper = _managementList[i];
				if (managementWrapper != null)
				{
					using PoolingList<PrefabWrapper> addList = managementWrapper.GetOpenedList();
					poolingList.AddRange(addList);
				}
			}
			for (int j = 0; j < poolingList.COUNT(); j++)
			{
				PrefabWrapper prefabWrapper = poolingList[j];
				if (prefabWrapper != null)
				{
					BaseWindow baseWindow = prefabWrapper.Window as BaseWindow;
					if (!(baseWindow == null) && !baseWindow.IsClosingWindowAnimating() && baseWindow.HideWorldMap)
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	private void LateUpdate()
	{
		if (_reorderRequested)
		{
			ReorderDepth(immediately: true);
		}
	}

	public BaseWindow.ETopmostMenuType GetTopmostMenuType(out string backButtonTitle)
	{
		for (int num = _openedWindowStackList.Count - 1; num >= 0; num--)
		{
			BaseWindow baseWindow = _openedWindowStackList[num];
			BaseWindow.ETopmostMenuType topmostMenuType = baseWindow.TopmostMenuType;
			if (topmostMenuType != BaseWindow.ETopmostMenuType._undefined_ && topmostMenuType != BaseWindow.ETopmostMenuType.DontChange)
			{
				backButtonTitle = baseWindow.title.GetLocalize();
				return topmostMenuType;
			}
		}
		backButtonTitle = string.Empty;
		return BaseWindow.ETopmostMenuType._undefined_;
	}

	public void RegisterOpenedWindow(BaseWindow window)
	{
		if (!(window == null) && window.BackButtonType != BaseWindow.EBackButtonType.Pass && (window.BackButtonType != BaseWindow.EBackButtonType._undefined_ || window.BackRegister))
		{
			if (_openedWindowStackList.Contains(window))
			{
				_openedWindowStackList.Remove(window);
			}
			BaseWindow last = Vespa_Generic_IList_Extension_Method.GetLast(_openedWindowStackList);
			if (last != null)
			{
				last.OnOtherWindowOpened();
			}
			_openedWindowStackList.Add(window);
		}
	}

	public void ClearAllOpenedWindowStack()
	{
		_openedWindowStackList.Clear();
	}

	public void ForEachOpenedWindow(Action<BaseWindow> action)
	{
		_openedWindowStackList.ForEach(action);
	}

	public bool ExecuteBack()
	{
		if (QuitGamePopup.IsOpenGlobal())
		{
			QuitGamePopup.CloseGlobal();
			return true;
		}
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			BaseWindow last = Vespa_Generic_IList_Extension_Method.GetLast(_openedWindowStackList);
			bool flag = last != null && last.TopmostMenuType != BaseWindow.ETopmostMenuType.Tutorial_Guide;
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing && flag)
			{
				if (TutorialWindow.isValidInstance)
				{
					TutorialWindow.instance.OnSystemBackButton();
				}
				return true;
			}
		}
		if (GlobalAutoBehaviour<LoadingManager>.isValidInstance && GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			return true;
		}
		if (ExecuteBackWindow())
		{
			return true;
		}
		return false;
	}

	private bool ExecuteBackWindow()
	{
		if (SingletonUpdater<WebServiceManager>.instance.Requester.HasAnyRequestOfFlag(WebRequestFlag.ShowLoadingGauge))
		{
			return true;
		}
		if (GlobalTopmostWindow.instance.ExecuteBack())
		{
			return true;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_openedWindowStackList))
		{
			return false;
		}
		BaseWindow last = Vespa_Generic_IList_Extension_Method.GetLast(_openedWindowStackList);
		if (last != null)
		{
			if (last.BackButtonType != BaseWindow.EBackButtonType.HandleSkip)
			{
				last.OnSystemBackButton();
				if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
				{
					SceneAutoBehaviour<LobbyManagement>.instance.PlayTurnBackSound();
				}
			}
			return true;
		}
		return false;
	}

	public bool UnregisterOpenedWindowStack(BaseWindow ClosedWindow)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_openedWindowStackList))
		{
			return false;
		}
		BaseWindow last = Vespa_Generic_IList_Extension_Method.GetLast(_openedWindowStackList);
		if (last == null)
		{
			return false;
		}
		if (ClosedWindow != last)
		{
			_openedWindowStackList.Remove(ClosedWindow);
			return false;
		}
		_openedWindowStackList.RemoveAt(_openedWindowStackList.Count - 1);
		Vespa_Generic_IList_Extension_Method.RemoveNull(_openedWindowStackList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_openedWindowStackList))
		{
			return true;
		}
		last = Vespa_Generic_IList_Extension_Method.GetLast(_openedWindowStackList);
		last.ReOpen();
		return true;
	}
}
