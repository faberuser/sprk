using System;
using NGame2.NUI.NManager;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

internal class SignOutCreateNewAccountPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SignOutCreateNewAccountPopup";

	private static GlobalWindowPrefab<NGame2.NUI.NWindow.SignOutCreateNewAccountPopup> _windowPrefab = new GlobalWindowPrefab<NGame2.NUI.NWindow.SignOutCreateNewAccountPopup>("Resources/Prefab2/Window/MessagePopup/SignOutCreateNewAccountPopup", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIButton ButtonYes;

	public UIButton ButtonNo;

	public UILabel ButtonLabelYes;

	public UILabel ButtonLabelNo;

	private Action _onYes;

	private Action _onNo;

	private Action _onClose;

	public static void Open(Action onYes, Action onNo, Action onClose)
	{
		WindowPrefab<NGame2.NUI.NWindow.SignOutCreateNewAccountPopup> windowPrefab = new WindowPrefab<NGame2.NUI.NWindow.SignOutCreateNewAccountPopup>("Resources/Prefab2/Window/MessagePopup/SignOutCreateNewAccountPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onYes, onNo, onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenGlobal(Action onYes, Action onNo, Action onClose)
	{
		_windowPrefab.instance.OpenInternal(onYes, onNo, onClose);
	}

	protected void OpenInternal(Action onYes, Action onNo, Action onClose)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		_onClose = onClose;
		UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect("NEW_ACCOUNT_CREATE_TITLE"));
		UIHelper.SetLabelText(MessageLabel, LocalizationManager.GetStringDirect("NEW_ACCOUNT_CREATE_MESSAGE"));
		UIHelper.SetLabelText(ButtonLabelYes, LocalizationManager.GetStringDirect("COMMON_YES"));
		UIHelper.SetLabelText(ButtonLabelNo, LocalizationManager.GetStringDirect("COMMON_NO"));
	}

	public void OnClickNo()
	{
		Vespa_System_Action_Extension_Method.fire(_onNo);
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_onYes);
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
	}

	public void OnClickClose()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
