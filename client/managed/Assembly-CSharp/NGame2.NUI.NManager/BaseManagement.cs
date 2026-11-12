using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager.NTask;
using NGame2.NUI.NWindow;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class BaseManagement<T> : SceneUpdateBehaviour<T>, IBackgroundBlackRequester where T : BaseManagement<T>
{
	private GenericManagementWrapper<T> _managementWrapper;

	public bool IsDirty;

	protected Dictionary<Type, PrefabWrapper> _wrapperDictionary = new Dictionary<Type, PrefabWrapper>();

	protected bool _useBackgroundBlack;

	protected bool _showLobbyMenu;

	private int _baseDepth;

	private int _maxDepth;

	private List<UIPanel> _panelSortList = new List<UIPanel>();

	private WindowTaskFactory<T> _taskFactory;

	public GenericManagementWrapper<T> ManagementWrapper
	{
		get
		{
			if (_managementWrapper == null)
			{
				_managementWrapper = new GenericManagementWrapper<T>(this);
			}
			return _managementWrapper;
		}
	}

	int IBackgroundBlackRequester.BaseDepth => _baseDepth;

	public WindowTaskFactory<T> TaskFactory => GetWindowTaskFactory();

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
			{
				GlobalAutoBehaviour<GlobalManagement>.instance.Close(ManagementWrapper);
			}
			_wrapperDictionary.Clear();
			_panelSortList.Clear();
		}
	}

	public PrefabWrapper AddInstance<TInstance>(string path, bool isPassiveWindow = false, bool preload = false) where TInstance : Component, IWindow
	{
		return AddInstance<TInstance>(path, GameUIRoot.ELayerType.UI, isPassiveWindow, preload);
	}

	public PrefabWrapper AddInstance<TInstance>(string path, GameUIRoot.ELayerType layerType, bool isPassiveWindow = false, bool preload = false) where TInstance : Component, IWindow
	{
		Type typeFromHandle = typeof(TInstance);
		WindowPrefab<TInstance> prefab = new WindowPrefab<TInstance>(path, layerType);
		PrefabWrapper prefabWrapper = new PrefabWrapper();
		prefabWrapper.Prefab = prefab;
		prefabWrapper.IsPassiveWindow = isPassiveWindow;
		if (preload)
		{
			prefabWrapper.ReserveCacheAction = prefab.ReserveCache;
		}
		prefabWrapper.InstancingAction = () => OnInstantiateWindowInstance(prefab);
		RemoveInstance<TInstance>();
		try
		{
			_wrapperDictionary.Add(typeFromHandle, prefabWrapper);
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
		return prefabWrapper;
	}

	private IWindow OnInstantiateWindowInstance<TInstance>(WindowPrefab<TInstance> prefab) where TInstance : Component, IWindow
	{
		if (prefab == null)
		{
			return null;
		}
		TInstance val = prefab.instance;
		if (val == null)
		{
			return null;
		}
		val.gameObject.SetActive(value: false);
		BaseWindow baseWindow = val as BaseWindow;
		if ((bool)baseWindow)
		{
			baseWindow.InitFromManagement(prefab, ManagementWrapper);
			MakeDirty();
		}
		return val;
	}

	public TInstance GetPrefab<TInstance>() where TInstance : Component, IWindow
	{
		Type typeFromHandle = typeof(TInstance);
		PrefabWrapper vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_wrapperDictionary, typeFromHandle, null);
		if (vALUE == null)
		{
			return null;
		}
		if (!(vALUE.Prefab is WindowPrefab<TInstance> windowPrefab))
		{
			return null;
		}
		return windowPrefab.Prefab;
	}

	public TInstance GetInstance<TInstance>(bool openedOnly = false) where TInstance : Component, IWindow
	{
		Type typeFromHandle = typeof(TInstance);
		PrefabWrapper vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_wrapperDictionary, typeFromHandle, null);
		if (vALUE == null)
		{
			return null;
		}
		if (!(vALUE.Prefab is WindowPrefab<TInstance> windowPrefab))
		{
			return null;
		}
		if (vALUE.Window == null)
		{
			if (openedOnly)
			{
				return null;
			}
			vALUE.Instantiate();
		}
		return windowPrefab.instance;
	}

	public void CloseInstance<TInstance>() where TInstance : Component, IWindow
	{
		if (IsOpened<TInstance>())
		{
			GetInstance<TInstance>().Close();
		}
	}

	public IWindow GetInstanceWindow(Type windowType)
	{
		PrefabWrapper vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_wrapperDictionary, windowType, null);
		if (vALUE == null)
		{
			return null;
		}
		IWindowPrefab prefab = vALUE.Prefab;
		if (prefab == null)
		{
			return null;
		}
		if (vALUE.Window == null)
		{
			vALUE.Instantiate();
		}
		return prefab.WindowRaw;
	}

	public void ForEachInstance(Action<PrefabWrapper> action)
	{
		foreach (KeyValuePair<Type, PrefabWrapper> item in _wrapperDictionary)
		{
			Vespa_System_Action_Extension_Method.fire(action, item.Value);
		}
	}

	public bool RemoveInstance<TInstance>()
	{
		PrefabWrapper value = null;
		Type type = SceneAutoBehaviour<T>.instance.GetType();
		if (_wrapperDictionary.TryGetValue(type, out value))
		{
			_wrapperDictionary.Remove(type);
			MakeDirty();
			return true;
		}
		return false;
	}

	public bool RemoveInstance(Type type)
	{
		PrefabWrapper value = null;
		if (_wrapperDictionary.TryGetValue(type, out value))
		{
			_wrapperDictionary.Remove(type);
			MakeDirty();
			return true;
		}
		return false;
	}

	public bool IsOpened<TInstance>() where TInstance : Component, IWindow
	{
		Type typeFromHandle = typeof(TInstance);
		return IsOpened(typeFromHandle);
	}

	public bool IsOpened(Type type)
	{
		PrefabWrapper vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_wrapperDictionary, type, null);
		if (vALUE == null)
		{
			return false;
		}
		if (vALUE.Prefab == null)
		{
			return false;
		}
		return vALUE.Prefab.IsOpen();
	}

	public bool IsOpenedAny()
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (value.Prefab.IsOpen())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOpenedAny(IList<Type> types)
	{
		if (types == null || types.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < types.Count; i++)
		{
			Type i_key = types[i];
			PrefabWrapper vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_wrapperDictionary, i_key, null);
			if (vALUE != null && vALUE.Prefab != null && vALUE.Prefab.IsOpen())
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsPersistent()
	{
		return false;
	}

	public void CloseBut(params IWindow[] exceptions)
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			Close(value.Window, exceptions);
		}
	}

	public void CloseBut<TInstance>() where TInstance : Component, IWindow
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (!(value.Prefab is WindowPrefab<TInstance>) && value.Window != null)
			{
				Close(value.Window, null);
			}
		}
	}

	public void CloseBut<TInstance1, TInstance2>() where TInstance1 : Component, IWindow where TInstance2 : Component, IWindow
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (!(value.Prefab is WindowPrefab<TInstance1>) && !(value.Prefab is WindowPrefab<TInstance2>) && value.Window != null)
			{
				Close(value.Window, null);
			}
		}
	}

	public void CloseBut<TInstance1, TInstance2, TInstance3>() where TInstance1 : Component, IWindow where TInstance2 : Component, IWindow where TInstance3 : Component, IWindow
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (!(value.Prefab is WindowPrefab<TInstance1>) && !(value.Prefab is WindowPrefab<TInstance2>) && !(value.Prefab is WindowPrefab<TInstance3>) && value.Window != null)
			{
				Close(value.Window, null);
			}
		}
	}

	public void CloseBut<TInstance1, TInstance2, TInstance3, TInstance4>() where TInstance1 : Component, IWindow where TInstance2 : Component, IWindow where TInstance3 : Component, IWindow where TInstance4 : Component, IWindow
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (!(value.Prefab is WindowPrefab<TInstance1>) && !(value.Prefab is WindowPrefab<TInstance2>) && !(value.Prefab is WindowPrefab<TInstance3>) && !(value.Prefab is WindowPrefab<TInstance4>) && value.Window != null)
			{
				Close(value.Window, null);
			}
		}
	}

	public void CloseBut<TInstance1, TInstance2, TInstance3, TInstance4, TInstance5>() where TInstance1 : Component, IWindow where TInstance2 : Component, IWindow where TInstance3 : Component, IWindow where TInstance4 : Component, IWindow where TInstance5 : Component, IWindow
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (!(value.Prefab is WindowPrefab<TInstance1>) && !(value.Prefab is WindowPrefab<TInstance2>) && !(value.Prefab is WindowPrefab<TInstance3>) && !(value.Prefab is WindowPrefab<TInstance4>) && !(value.Prefab is WindowPrefab<TInstance5>) && value.Window != null)
			{
				Close(value.Window, null);
			}
		}
	}

	public virtual void CloseActiveWindows(EDepthOrder depthOrder, bool immediately = false)
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (value.IsPassiveWindow || !value.Prefab.IsOpen())
			{
				continue;
			}
			if (value.Window is BaseWindow)
			{
				BaseWindow baseWindow = value.Window as BaseWindow;
				if (baseWindow.DepthOrder == depthOrder)
				{
					baseWindow.Close2(immediately);
				}
			}
			else if (value.Prefab.WindowRaw != null && value.Prefab.WindowRaw.DepthOrder == depthOrder)
			{
				value.Prefab.CloseWindow();
			}
			MakeDirty();
		}
	}

	public virtual void CloseAllActiveWindow(bool immediately = false, IEnumerable<IWindow> exceptions = null)
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			if (!value.IsPassiveWindow && value.Prefab.IsOpen() && (exceptions == null || !exceptions.Contains(value.Window)))
			{
				if (value.Window is BaseWindow)
				{
					(value.Window as BaseWindow).Close2(immediately);
				}
				else
				{
					value.Prefab.CloseWindow();
				}
				MakeDirty();
			}
		}
	}

	private void Close(IWindow window, IWindow[] exceptions)
	{
		if ((exceptions == null || !exceptions.Contains(window)) && window != null)
		{
			window.Close();
			MakeDirty();
		}
	}

	public void OnWindowOpened(IWindow window)
	{
		GlobalAutoBehaviour<GlobalManagement>.instance.Open(ManagementWrapper);
		MakeDirty();
	}

	public void MakeDirty()
	{
		IsDirty = true;
	}

	public PoolingList<PrefabWrapper> GetOpenedList()
	{
		PoolingList<PrefabWrapper> result = default(PoolingList<PrefabWrapper>);
		foreach (KeyValuePair<Type, PrefabWrapper> item in _wrapperDictionary)
		{
			PrefabWrapper value = item.Value;
			IWindow window = value.Window;
			if (window != null && window.IsOpen())
			{
				result.Add(value);
			}
		}
		return result;
	}

	public bool ShowLobbyMenu()
	{
		return _showLobbyMenu;
	}

	public void ReserveCache(UiPreloader preloader)
	{
		foreach (PrefabWrapper value in _wrapperDictionary.Values)
		{
			Vespa_System_Action_Extension_Method.fire(value.ReserveCacheAction, preloader);
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (IsDirty)
		{
			IsDirty = false;
			ReorderDepthIfNeed();
		}
	}

	private void ReorderDepthIfNeed()
	{
		using PoolingList<PrefabWrapper> poolingList = GetOpenedList();
		if (poolingList.Count == 0)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.Close(ManagementWrapper);
		}
		if (_useBackgroundBlack)
		{
			if (poolingList.Count > 0)
			{
				GameUIRoot.instance.BgBlack.Open(this);
			}
			else
			{
				GameUIRoot.instance.BgBlack.Close(this);
			}
		}
		bool flag = false;
		foreach (KeyValuePair<Type, PrefabWrapper> item in _wrapperDictionary)
		{
			BaseWindow baseWindow = item.Value.Window as BaseWindow;
			if (baseWindow != null && baseWindow.DepthScoreDirty)
			{
				baseWindow.DepthScoreDirty = false;
				flag = true;
			}
		}
		if (flag)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.ReorderDepth();
		}
	}

	public void UseBackgroundBlack(bool use)
	{
		_useBackgroundBlack = use;
	}

	public void ResetDepth()
	{
		_baseDepth = 0;
		_maxDepth = 0;
	}

	public virtual void ReorderDepth(EDepthOrder depthOrder, ref int baseDepth)
	{
		using PoolingList<PrefabWrapper> poolingList = GetOpenedList();
		poolingList.Sort(delegate(PrefabWrapper l, PrefabWrapper r)
		{
			int num4 = ((l.Window is BaseWindow) ? (l.Window as BaseWindow).DepthScore : 0);
			int value = ((r.Window is BaseWindow) ? (r.Window as BaseWindow).DepthScore : 0);
			return num4.CompareTo(value);
		});
		for (int num = 0; num < poolingList.Count; num++)
		{
			IWindow window = poolingList[num].Window;
			if (window == null || window.DepthOrder != depthOrder)
			{
				continue;
			}
			Component component = window as Component;
			if (component == null)
			{
				continue;
			}
			UIPanel[] componentsInChildren = component.gameObject.GetComponentsInChildren<UIPanel>(includeInactive: true);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(componentsInChildren))
			{
				if (_baseDepth == 0)
				{
					_baseDepth = baseDepth;
				}
				window.SetBasePanelDepth(baseDepth);
				_panelSortList.AddRange(componentsInChildren);
				_panelSortList.Sort((UIPanel l, UIPanel r) => l.depth.CompareTo(r.depth));
				for (int num2 = 0; num2 < _panelSortList.Count; num2++)
				{
					int num3 = baseDepth++;
					_maxDepth = Mathf.Max(_maxDepth, num3);
					_panelSortList[num2].depth = num3;
				}
				_panelSortList.Clear();
				baseDepth++;
			}
		}
	}

	private WindowTaskFactory<T> GetWindowTaskFactory()
	{
		if (_taskFactory == null)
		{
			_taskFactory = new WindowTaskFactory<T>(this, GetInstanceWindow);
		}
		return _taskFactory;
	}

	public void AddTask(ITask task)
	{
		SingletonUpdater<TaskManager>.instance.AddTask(task);
	}

	public void AddTask(params ITask[] tasks)
	{
		GroupTask groupTask = new GroupTask();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(tasks); i++)
		{
			ITask task = tasks[i];
			if (task == null)
			{
				task.NullTaskWarning();
			}
			else
			{
				groupTask.AddTask(task);
			}
		}
		groupTask.Commit();
	}
}
