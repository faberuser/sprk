using System.Collections;
using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GlobalTopmostWindow : MonoBehaviour, IGlobalWindow, IInspectorGuiDrawer
{
	public enum ESceneChangeType
	{
		BlackScreen,
		Cloud,
		EclipseNextCampaign,
		EclipseNextTeam,
		ChangeBattleField
	}

	public interface IPrivateCaller
	{
		void Release();
	}

	private class TouchDisablerHolder
	{
		private int _lastValidTick;

		public ITouchDisabler Disabler { get; private set; }

		public TouchDisablerHolder(ITouchDisabler disabler)
		{
			Disabler = disabler;
			_lastValidTick = UnitySystemTimer.SystemTick;
		}

		public void Reset()
		{
			_lastValidTick = UnitySystemTimer.SystemTick;
		}

		public bool IsValid()
		{
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(Disabler))
			{
				return false;
			}
			if (Disabler.IsValid)
			{
				_lastValidTick = UnitySystemTimer.SystemTick;
				return true;
			}
			if (UnitySystemTimer.SystemTick - _lastValidTick < 100)
			{
				return true;
			}
			return false;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GlobalTopmostWindow";

	private static GlobalWindowPrefab<GlobalTopmostWindow> _prefab;

	public UIGame3DWidgetFromRes TouchEffectWidget;

	public UIWidget DisableTouchWidget;

	private bool _oneTimeDisable;

	private List<TouchDisablerHolder> _touchDisablerList = new List<TouchDisablerHolder>();

	private List<SceneChanger> _sceneChangerList = new List<SceneChanger>();

	public static GlobalTopmostWindow instance
	{
		get
		{
			CreateInstance();
			return _prefab.instance;
		}
	}

	public static bool IsValidInstance
	{
		get
		{
			if (_prefab != null)
			{
				return _prefab.IsValidInstance;
			}
			return false;
		}
	}

	public static void CreateInstance()
	{
		if (_prefab == null)
		{
			_prefab = new GlobalWindowPrefab<GlobalTopmostWindow>("Resources/Prefab2/Window/GlobalTopmostWindow", GlobalUIRoot.ELayerType.GlobalUI);
			_prefab.CreateInstance();
		}
	}

	public void Reset()
	{
		_touchDisablerList.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_sceneChangerList); i++)
		{
			_sceneChangerList[i].Close();
		}
	}

	public void TouchEffect(Vector3 offset)
	{
		TouchEffectWidget.Play(offset, null);
	}

	public void AddTouchDisabler(ITouchDisabler touchDisabler)
	{
		for (int i = 0; i < _touchDisablerList.Count; i++)
		{
			TouchDisablerHolder touchDisablerHolder = _touchDisablerList[i];
			if (touchDisablerHolder.Disabler == touchDisabler)
			{
				touchDisablerHolder.Reset();
				return;
			}
		}
		_touchDisablerList.Add(new TouchDisablerHolder(touchDisabler));
		UpdateDisableTouch();
	}

	public void RemoveTouchDisabler(ITouchDisabler touchDisabler)
	{
		for (int i = 0; i < _touchDisablerList.Count; i++)
		{
			if (_touchDisablerList[i].Disabler == touchDisabler)
			{
				_touchDisablerList.RemoveAt(i);
				UpdateDisableTouch();
				break;
			}
		}
	}

	public SceneChanger CreateSceneChanger(float timeOut, ESceneChangeType type, float timeScale = 1f)
	{
		SceneChanger sceneChanger = new SceneChanger();
		sceneChanger.Open(timeOut, type, timeScale);
		_sceneChangerList.Add(sceneChanger);
		return sceneChanger;
	}

	public void CloseSceneChangers()
	{
		foreach (SceneChanger sceneChanger in _sceneChangerList)
		{
			sceneChanger.Resume();
			sceneChanger.Close();
		}
	}

	public bool ExecuteBack()
	{
		return UIHelper.IsEnabledGameObjet(DisableTouchWidget);
	}

	public void SetOneTimeDisable()
	{
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
	}

	private void Start()
	{
		if (TouchEffectWidget != null)
		{
			TouchEffectWidget.ReserveContents();
		}
		VespaCoroutine.StartGlobal(coLoop(), "(oneTimeDisable)");
	}

	private void Update()
	{
		UpdateSceneChanger();
		UpdateDisableTouch();
	}

	private IEnumerator coLoop()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			_oneTimeDisable = false;
		}
	}

	private void UpdateDisableTouch()
	{
		bool flag = false;
		for (int i = 0; i < _touchDisablerList.Count; i++)
		{
			if (_touchDisablerList[i].IsValid())
			{
				flag = true;
				continue;
			}
			_touchDisablerList.RemoveAt(i);
			i--;
		}
		if (!flag)
		{
			for (int j = 0; j < _sceneChangerList.Count; j++)
			{
				if (!_sceneChangerList[j].IsCompleteAnimation())
				{
					flag = true;
					break;
				}
			}
		}
		if (_oneTimeDisable)
		{
			flag = true;
		}
		UIHelper.EnableGameObject(DisableTouchWidget, flag);
	}

	private void UpdateSceneChanger()
	{
		for (int i = 0; i < _sceneChangerList.Count; i++)
		{
			SceneChanger sceneChanger = _sceneChangerList[i];
			if (sceneChanger.IsTimeout() || sceneChanger.IsClosed())
			{
				Vespa_Common_Class_Extension_Method.Interface((IPrivateCaller)sceneChanger).Release();
				_sceneChangerList.RemoveAt(i);
				i--;
			}
		}
	}
}
