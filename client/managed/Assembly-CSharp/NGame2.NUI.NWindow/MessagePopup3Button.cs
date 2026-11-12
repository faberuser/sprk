using System;
using NGame2.NUI.NManager;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MessagePopup3Button : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MessagePopup3Button";

	private static GlobalWindowPrefab<MessagePopup3Button> _windowPrefab = new GlobalWindowPrefab<MessagePopup3Button>("Resources/Prefab2/Window/MessagePopup/MessagePopup3Button", GlobalUIRoot.ELayerType.GlobalUI);

	public const string PrefabPath_Battle = "Resources/Prefab2/Window/MessagePopup/MessagePopup3Button_Battle";

	private static GlobalWindowPrefab<MessagePopup3Button> _windowPrefab_Battle = new GlobalWindowPrefab<MessagePopup3Button>("Resources/Prefab2/Window/MessagePopup/MessagePopup3Button_Battle", GlobalUIRoot.ELayerType.GlobalUI);

	private float _remainedTime;

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIButton Button1;

	public UIButton Button2;

	public UILabel ButtonLabel1;

	public UILabel ButtonLabel2;

	private Action _onButton1;

	private Action _onButton2;

	private Action _onClose;

	public string Title { get; set; }

	public string Message { get; set; }

	public static void OpenGlobal(string title, string message, Action onButton1, Action onButton2, Enum button1Label = null, Enum button2Label = null)
	{
		if (button1Label == null)
		{
			button1Label = NShared.NLocalization.Common.COMMON_YES;
		}
		if (button2Label == null)
		{
			button2Label = NShared.NLocalization.Common.COMMON_NO;
		}
		string button1Label2 = LocalizationManager.GetString(button1Label);
		string button2Label2 = LocalizationManager.GetString(button2Label);
		_windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, button1Label2, button2Label2, isButton2UseClose: true);
	}

	public static void OpenGlobal(string title, string message, Action onButton1, Action onButton2, Action onClose, string button1Label = null, string button2Label = null)
	{
		if (button1Label == null)
		{
			button1Label = "COMMON_YES";
		}
		if (button2Label == null)
		{
			button2Label = "COMMON_NO";
		}
		string stringDirect = LocalizationManager.GetStringDirect(button1Label);
		string stringDirect2 = LocalizationManager.GetStringDirect(button2Label);
		_windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, onClose, stringDirect, stringDirect2);
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}

	public static void Open(string title, string message, Action onButton1, Action onButton2, Enum button1Label = null, Enum button2Label = null)
	{
		if (button1Label == null)
		{
			button1Label = NShared.NLocalization.Common.COMMON_YES;
		}
		if (button2Label == null)
		{
			button2Label = NShared.NLocalization.Common.COMMON_NO;
		}
		string button1Label2 = LocalizationManager.GetString(button1Label);
		string button2Label2 = LocalizationManager.GetString(button2Label);
		Open(title, message, onButton1, onButton2, button1Label2, button2Label2);
	}

	public static void Open(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label)
	{
		WindowPrefab<MessagePopup3Button> windowPrefab = new WindowPrefab<MessagePopup3Button>("Resources/Prefab2/Window/MessagePopup/MessagePopup3Button", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, button1Label, button2Label);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(string title, string message, Action onButton1, Action onButton2, Action onClose, string button1Label, string button2Label)
	{
		WindowPrefab<MessagePopup3Button> windowPrefab = new WindowPrefab<MessagePopup3Button>("Resources/Prefab2/Window/MessagePopup/MessagePopup3Button", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, button1Label, button2Label);
		windowPrefab.instance.SetOnCloseAction(onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open_BattlePopup(string title, string message, Action onButton1, Action onButton2, Enum button1Label = null, Enum button2Label = null, int waitingTime = 0)
	{
		string stringWithDefaultKey = LocalizationManager.GetStringWithDefaultKey(button1Label, NShared.NLocalization.Common.COMMON_YES);
		string stringWithDefaultKey2 = LocalizationManager.GetStringWithDefaultKey(button2Label, NShared.NLocalization.Common.COMMON_NO);
		WindowPrefab<MessagePopup3Button> windowPrefab = new WindowPrefab<MessagePopup3Button>("Resources/Prefab2/Window/MessagePopup/MessagePopup3Button_Battle", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, stringWithDefaultKey, stringWithDefaultKey2, isButton2UseClose: true);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		windowPrefab.instance._remainedTime = waitingTime;
	}

	public static void OpenGlobal_BattlePopup(string title, string message, Action onButton1, Action onButton2, Enum button1Label = null, Enum button2Label = null, int waitingTime = 0)
	{
		string stringWithDefaultKey = LocalizationManager.GetStringWithDefaultKey(button1Label, NShared.NLocalization.Common.COMMON_YES);
		string stringWithDefaultKey2 = LocalizationManager.GetStringWithDefaultKey(button2Label, NShared.NLocalization.Common.COMMON_NO);
		_windowPrefab_Battle.instance.OpenInternal(title, message, onButton1, onButton2, stringWithDefaultKey, stringWithDefaultKey2, isButton2UseClose: true);
		_windowPrefab_Battle.instance._remainedTime = waitingTime;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!(_remainedTime <= 0f))
		{
			_remainedTime -= Time.deltaTime;
			if (_remainedTime <= 0f)
			{
				Close();
				return;
			}
			string stringDirect = LocalizationManager.GetStringDirect("MATCH_INVITE_WAIT_2", (int)_remainedTime);
			UIHelper.SetLabelText(MessageLabel, Message + "\n" + stringDirect);
		}
	}

	protected void OpenInternal(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label, bool isButton2UseClose)
	{
		base.Open();
		_onButton1 = onButton1;
		_onButton2 = ((onButton2 != null) ? onButton2 : new Action(OnClickClose));
		if (onButton2 != null && isButton2UseClose)
		{
			_onClose = _onButton2;
		}
		Title = title;
		Message = message;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.SetLabelText(ButtonLabel1, button1Label);
		UIHelper.SetLabelText(ButtonLabel2, button2Label);
	}

	protected void OpenInternal(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label)
	{
		base.Open();
		_onButton1 = onButton1;
		_onButton2 = ((onButton2 != null) ? onButton2 : new Action(OnClickClose));
		Title = title;
		Message = message;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.SetLabelText(ButtonLabel1, button1Label);
		UIHelper.SetLabelText(ButtonLabel2, button2Label);
	}

	protected void OpenInternal(string title, string message, Action onButton1, Action onButton2, Action onClose, string button1Label, string button2Label)
	{
		base.Open();
		_onButton1 = onButton1;
		_onButton2 = onButton2;
		_onClose = onClose;
		Title = title;
		Message = message;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		UIHelper.SetLabelText(ButtonLabel1, button1Label);
		UIHelper.SetLabelText(ButtonLabel2, button2Label);
	}

	public override void Reset()
	{
		base.Reset();
		_onButton1 = null;
		_onButton2 = null;
		_onClose = null;
		Title = null;
		Message = null;
	}

	public void OnClickClose()
	{
		if (!SprkPartyRetry.KeepOpen(this))
		{
			Close();
			Vespa_System_Action_Extension_Method.fire(_onClose);
		}
	}

	private void OnClickButton(Action onButton)
	{
		_onButton1 = null;
		_onButton2 = null;
		Close();
		Vespa_System_Action_Extension_Method.fire(onButton);
	}

	public void OnClickButton1()
	{
		OnClickButton(_onButton1);
	}

	public void OnClickButton2()
	{
		OnClickButton(_onButton2);
	}

	public void SetOnCloseAction(Action onClose)
	{
		if (onClose != null)
		{
			_onClose = onClose;
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
