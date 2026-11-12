using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NCutScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWorldMap;
using NGame2.NUtil;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask;

namespace NGame2.NUI.NManager;

public class PopupQueue
{
	[Flags]
	public enum PopupFlag
	{
		None = 0,
		HideOnWorldMapHidden = 1,
		HideOnEntryMenuOpen = 2,
		HideOnTopmostMenuOpen = 4,
		DefaultPopupFlag = 3
	}

	public interface IQueuedPopup
	{
		void Open();

		void Close();

		bool IsOpen();

		bool CanOpen();

		PopupFlag GetFlag();

		Type GetPopupType();

		string GetGroupName();
	}

	private class QueuedPopup<T> : IQueuedPopup where T : BaseWindow
	{
		public T PopupWindow;

		public Action<T> OpenAction;

		public Func<bool> CanOpenFunc;

		public PopupFlag Flag;

		public string GroupName;

		void IQueuedPopup.Open()
		{
			Vespa_System_Action_Extension_Method.fire(OpenAction, PopupWindow);
		}

		bool IQueuedPopup.IsOpen()
		{
			return PopupWindow.IsOpen();
		}

		bool IQueuedPopup.CanOpen()
		{
			if (CanOpenFunc != null)
			{
				return Vespa_System_Action_Extension_Method.fire(CanOpenFunc);
			}
			return true;
		}

		void IQueuedPopup.Close()
		{
			PopupWindow.Close();
		}

		PopupFlag IQueuedPopup.GetFlag()
		{
			return Flag;
		}

		Type IQueuedPopup.GetPopupType()
		{
			return typeof(T);
		}

		string IQueuedPopup.GetGroupName()
		{
			return GroupName;
		}
	}

	private class QueuedAction : IQueuedPopup
	{
		public Action OpenAction;

		public Func<bool> IsOpenFunc;

		public Func<bool> CanOpenFunc;

		public PopupFlag Flag;

		public string GroupName;

		void IQueuedPopup.Open()
		{
			Vespa_System_Action_Extension_Method.fire(OpenAction);
		}

		bool IQueuedPopup.IsOpen()
		{
			if (IsOpenFunc != null)
			{
				return Vespa_System_Action_Extension_Method.fire(IsOpenFunc);
			}
			return false;
		}

		bool IQueuedPopup.CanOpen()
		{
			if (CanOpenFunc != null)
			{
				return Vespa_System_Action_Extension_Method.fire(CanOpenFunc);
			}
			return true;
		}

		void IQueuedPopup.Close()
		{
		}

		PopupFlag IQueuedPopup.GetFlag()
		{
			return Flag;
		}

		Type IQueuedPopup.GetPopupType()
		{
			return typeof(Action);
		}

		string IQueuedPopup.GetGroupName()
		{
			return GroupName;
		}
	}

	protected List<IQueuedPopup> _queuedPopupList = new List<IQueuedPopup>();

	private IQueuedPopup _currentPopup;

	public bool Empty()
	{
		return _queuedPopupList.Count == 0;
	}

	public bool IsProgressing()
	{
		return _currentPopup != null;
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, Func<bool> canOpenFunc, PopupFlag flag = PopupFlag.None, string groupName = "") where T : BaseWindow
	{
		if (!(popupWindow == null))
		{
			AddPopup(new QueuedPopup<T>
			{
				PopupWindow = popupWindow,
				OpenAction = openAction,
				CanOpenFunc = canOpenFunc,
				Flag = flag,
				GroupName = groupName
			});
		}
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, PopupFlag flag = PopupFlag.None, string groupName = "") where T : BaseWindow
	{
		AddPopup(popupWindow, openAction, null, flag, groupName);
	}

	public void AddAction(Action openAction, Func<bool> isOpenFunc = null, PopupFlag flag = PopupFlag.None, string groupName = "")
	{
		AddPopup(new QueuedAction
		{
			OpenAction = openAction,
			IsOpenFunc = isOpenFunc,
			Flag = flag,
			GroupName = groupName
		});
	}

	protected virtual void AddPopup(IQueuedPopup popup)
	{
		if (popup != null)
		{
			_queuedPopupList.Add(popup);
		}
	}

	public void RemovePopup<T>()
	{
		List<IQueuedPopup> list = new List<IQueuedPopup>();
		foreach (IQueuedPopup queuedPopup in _queuedPopupList)
		{
			if (queuedPopup.GetPopupType() == typeof(T))
			{
				list.Add(queuedPopup);
			}
		}
		foreach (IQueuedPopup item in list)
		{
			_queuedPopupList.Remove(item);
		}
	}

	public void RemoveAll(string groupName)
	{
		lock (_queuedPopupList)
		{
			List<IQueuedPopup> list = new List<IQueuedPopup>();
			foreach (IQueuedPopup queuedPopup in _queuedPopupList)
			{
				if (queuedPopup.GetGroupName() == groupName)
				{
					list.Add(queuedPopup);
				}
			}
			foreach (IQueuedPopup item in list)
			{
				_queuedPopupList.Remove(item);
			}
		}
	}

	protected virtual bool CanProcessOpen()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return false;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				return false;
			}
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			return false;
		}
		if (GlobalAutoBehaviour<PopupManegement>.isValidInstance && GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist())
		{
			return false;
		}
		return true;
	}

	public void UpdatePopup()
	{
		if ((_queuedPopupList.Count != 0 || _currentPopup != null) && CanProcessOpen())
		{
			OpenNextQueuePopupIfPossible();
		}
	}

	private void OpenNextQueuePopupIfPossible()
	{
		if (_currentPopup == null || !_currentPopup.IsOpen())
		{
			_currentPopup = null;
			IQueuedPopup queuedPopup = PopFirstQueuedPopup();
			if (queuedPopup != null)
			{
				_currentPopup = queuedPopup;
				queuedPopup.Open();
			}
		}
	}

	private IQueuedPopup PopFirstQueuedPopup()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_queuedPopupList); i++)
		{
			IQueuedPopup queuedPopup = _queuedPopupList[i];
			if (queuedPopup != null && queuedPopup.CanOpen() && !queuedPopup.IsOpen())
			{
				PopupFlag flag = queuedPopup.GetFlag();
				if (CheckFlag(flag))
				{
					_queuedPopupList.RemoveAt(i);
					return queuedPopup;
				}
			}
		}
		return null;
	}

	protected bool CheckFlag(PopupFlag flag)
	{
		if ((flag & PopupFlag.HideOnWorldMapHidden) != PopupFlag.None && (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance || SceneAutoBehaviour<WorldMapManagement>.instance.IsHidden))
		{
			return false;
		}
		if ((flag & PopupFlag.HideOnEntryMenuOpen) != PopupFlag.None && SceneAutoBehaviour<WorldMapManagement>.isValidInstance && SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>().IsEntryMenuVisible)
		{
			return false;
		}
		if ((flag & PopupFlag.HideOnTopmostMenuOpen) != PopupFlag.None)
		{
			if (SceneAutoBehaviour<LobbyManagement>.isValidInstance && SceneAutoBehaviour<LobbyManagement>.instance.AnyTopmostDepthOrderWindowOpen())
			{
				return false;
			}
			bool topmostDepthOrderWindowWillOpen = false;
			SingletonUpdater<TaskManager>.instance.ForEachTask(delegate(ITask task)
			{
				if (!topmostDepthOrderWindowWillOpen && task is WindowOpenTask windowOpenTask)
				{
					BaseWindow window = windowOpenTask.GetWindow();
					if (window != null && window.DepthOrder == EDepthOrder.Topmost)
					{
						topmostDepthOrderWindowWillOpen = true;
					}
				}
			});
			if (topmostDepthOrderWindowWillOpen)
			{
				return false;
			}
		}
		return true;
	}

	public void OpenReservedPopup(string popupName, Action onEnd)
	{
		VespaCoroutine.Start(coOpenReservedPopup(popupName, onEnd));
	}

	private IEnumerator coOpenReservedPopup(string popupName, Action onEnd)
	{
		IQueuedPopup foundPopup = null;
		foreach (IQueuedPopup queuedPopup in _queuedPopupList)
		{
			if (string.Equals(queuedPopup.GetPopupType().Name, popupName, StringComparison.InvariantCultureIgnoreCase))
			{
				foundPopup = queuedPopup;
				break;
			}
		}
		if (foundPopup == null)
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
			yield break;
		}
		_queuedPopupList.Remove(foundPopup);
		while (_currentPopup != null && _currentPopup.IsOpen())
		{
			yield return null;
		}
		if (!foundPopup.IsOpen())
		{
			foundPopup.Open();
		}
		while (foundPopup.IsOpen())
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
