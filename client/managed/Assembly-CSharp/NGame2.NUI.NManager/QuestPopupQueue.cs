using System;
using NGame2.NScene;
using NGame2.NUI.NWindow;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class QuestPopupQueue : SceneUpdateBehaviour<QuestPopupQueue>
{
	private PopupQueue _popupQueue = new PopupQueue();

	private bool _opened;

	public void Open()
	{
		_opened = true;
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdatePopup();
	}

	public void UpdatePopup()
	{
		if (_opened && !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			_popupQueue.UpdatePopup();
		}
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, Func<bool> canOpenFunc, PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.None) where T : BaseWindow
	{
		if (!(popupWindow == null))
		{
			_popupQueue.AddPopup(popupWindow, openAction, canOpenFunc, flag);
		}
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.None) where T : BaseWindow
	{
		AddPopup(popupWindow, openAction, null, flag);
	}
}
