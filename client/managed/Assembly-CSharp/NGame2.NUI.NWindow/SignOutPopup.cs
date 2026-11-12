using System;

namespace NGame2.NUI.NWindow;

public class SignOutPopup : BaseWindow, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SignOutPopup";

	private static GlobalWindowPrefab<SignOutPopup> _windowPrefab = new GlobalWindowPrefab<SignOutPopup>("Resources/Prefab2/Window/MessagePopup/SignOutPopup", GlobalUIRoot.ELayerType.GlobalUI);

	private const int MinWidth = 400;

	private const int MinHeight = 100;

	public UIButton ButtonYes;

	public UILabel LabelConfirm;

	public UIInput InputConfirm;

	private Action _onYes;

	private Action _onNo;

	public static bool IsOpenGlobal()
	{
		return _windowPrefab.IsOpen();
	}

	public static void OpenGlobal(Action onYes, Action onNo)
	{
		_windowPrefab.instance.OpenInternal(onYes, onNo);
	}

	protected void OpenInternal(Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		InputConfirm.value = string.Empty;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (ButtonYes != null && LabelConfirm != null && InputConfirm != null)
		{
			UIHelper.EnableButton(ButtonYes, LabelConfirm.text == InputConfirm.value);
		}
	}

	public override void Close()
	{
		Vespa_System_Action_Extension_Method.fire(_onNo);
		_onYes = null;
		_onNo = null;
		base.Close();
	}

	public void OnClickYes()
	{
		if (!(LabelConfirm.text != InputConfirm.value))
		{
			Vespa_System_Action_Extension_Method.fire(_onYes);
			_onYes = null;
			_onNo = null;
			base.Close();
		}
	}

	public void OnClickClose()
	{
		Close();
	}
}
