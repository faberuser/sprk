using System;

namespace NGame2.NUI.NWindow;

public class QuitGamePopup : BaseWindow, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/QuitGamePopup";

	private static GlobalWindowPrefab<QuitGamePopup> _windowPrefab = new GlobalWindowPrefab<QuitGamePopup>("Resources/Prefab2/Window/MessagePopup/QuitGamePopup", GlobalUIRoot.ELayerType.GlobalUI);

	private Action _onOk;

	private Action _onCancel;

	public static bool IsOpenGlobal()
	{
		return _windowPrefab.IsOpen();
	}

	public static void CloseGlobal()
	{
		if (IsOpenGlobal())
		{
			_windowPrefab.instance.Close();
		}
	}

	public static void OpenGlobal(Action onOk, Action onCancel = null)
	{
		_windowPrefab.instance.OpenInternal(onOk, onCancel);
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	protected void OpenInternal(Action onOK, Action onCancel = null)
	{
		base.Open();
		_onOk = onOK;
		_onCancel = onCancel;
	}

	public override void Reset()
	{
		base.Reset();
		_onOk = null;
		_onCancel = null;
	}

	public override void Close()
	{
		_onOk = null;
		Action onCancel = _onCancel;
		_onCancel = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onCancel);
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnClickOk()
	{
		Action onOk = _onOk;
		_onOk = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onOk);
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
