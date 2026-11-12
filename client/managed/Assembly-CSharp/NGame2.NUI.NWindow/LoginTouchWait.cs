using System;

namespace NGame2.NUI.NWindow;

public class LoginTouchWait : BaseWindow, MemoryWarningPopup.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginTouchWait";

	private Action _onTouch;

	public UIWidget TouchScreen;

	public new void Open(Action onTouch)
	{
		Open(delegate
		{
			Init(onTouch);
		}, null);
	}

	public void Init(Action onTouch)
	{
		_onTouch = onTouch;
		SprkAccounts.ContinueAccountSwitch(this);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void OnTouchScreen()
	{
		Vespa_System_Action_Extension_Method.fire(_onTouch);
		_onTouch = null;
	}

	public void OnOpenedMemoryWarningPopup()
	{
		if (TouchScreen != null)
		{
			UIHelper.EnableGameObject(TouchScreen, enable: false);
		}
	}

	public void OnClosedMemoryWarningPopup()
	{
		if (TouchScreen != null)
		{
			UIHelper.EnableGameObject(TouchScreen, enable: true);
		}
	}
}
