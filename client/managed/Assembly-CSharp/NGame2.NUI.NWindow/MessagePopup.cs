using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class MessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MessagePopup";

	public const string PrefabPath2 = "Resources/Prefab2/Window/MessagePopup/MessagePopup2";

	public const string PrefabPath2Button = "Resources/Prefab2/Window/MessagePopup/MessagePopup2Button";

	public const string PrefabPathCS = "Resources/Prefab2/Window/MessagePopup/MessagePopupCS";

	public const string PrefabPathLog = "Resources/Prefab2/Window/MessagePopup/MessagePopupLog";

	public const string PrefabPathTypePass = "Resources/Prefab2/Window/MessagePopup/MessagePopupTypePass";

	public const string PrefabPath_PayShop = "Resources/Prefab2/Window/PayShop/PayShopMessagePopup";

	private static List<WindowPrefab<MessagePopup>> _windowPrefabList = new List<WindowPrefab<MessagePopup>>();

	private const int MinWidth = 400;

	private const int MinHeight = 100;

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UILabel MessageLabel2;

	public UIButton ButtonOk;

	public UIButton ButtonYes;

	public UIButton ButtonNo;

	public UIButton ButtonClose;

	protected Action _onClose;

	protected Action _onYes;

	protected Action _onNo;

	public UILabel LabelYes;

	public UILabel LabelNo;

	public UILabel LabelOk;

	public UILabel LabelRetry;

	private bool _ignoreOnClickClose;

	private bool _ignoreBackButton;

	public string Title { get; set; }

	public string Message { get; set; }

	public static void OpenInform(string message)
	{
		Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message, null);
	}

	public static void Open(string title, string message)
	{
		Open(title, message, null);
	}

	public static void Open(string title, string message, Action onOk)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onOk);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenYesOnly(string title, string message, Action onYes, bool ignoreOnClickClose = false)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternalYesOnly(title, message, onYes, ignoreOnClickClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenYesOnly(string title, string message, Action onYes, bool ignoreBackButton, bool ignoreOnClickClose = false)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternalYesOnly(title, message, onYes, null, ignoreBackButton, ignoreOnClickClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenYesOnly(string title, string message, Action onYes, string yesLabel, bool ignoreBackButton, bool ignoreOnClickClose = false)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternalYesOnly(title, message, onYes, yesLabel, ignoreBackButton, ignoreOnClickClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(string title, string message, Action onYes, Action onNo)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open2Button(string title, string message, Action onYes, Action onNo)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup2Button", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open2(string title, string message1, string message2, Action onYes, Action onNo)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup2", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message1, message2, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(string title, string message, Action onYes, Action onNo, bool isIgnoreOnClose)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo, isIgnoreOnClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(string title, string message, Action onYes, Action onNo, Enum YesLabelKey, Enum NoLabelKey)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo, YesLabelKey, NoLabelKey);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenCS(string title, string message, Action onYes, Action onNo, Enum YesLabelKey, Enum NoLabelKey)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopupCS", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo, YesLabelKey, NoLabelKey);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenLog(string title, string message, string message2)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopupLog", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, message2, null, null);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenTypePass(string title, string message, Action onYes, Action onNo)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopupTypePass", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenEnabled(string title, string message, Action onYes, Action onNo, string YesLabelKey, bool bYesEnabled)
	{
		WindowPrefab<MessagePopup> windowPrefab = new WindowPrefab<MessagePopup>("Resources/Prefab2/Window/MessagePopup/MessagePopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo, YesLabelKey, bYesEnabled);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenError(string message, Action onOk = null)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		if (string.IsNullOrEmpty(message))
		{
			message = text;
		}
		VespaDebug.LogError(message + "\n" + StackTraceUtility.ExtractStackTrace());
		Open(text, message, onOk);
	}

	public static void OpenError(Enum error, Action onOk = null)
	{
		if (!(error.GetType() == typeof(BaseResultType)) || (BaseResultType)(object)error != BaseResultType.Disabled)
		{
			OpenError(LocalizationManager.GetString(error), onOk);
		}
	}

	public static ITask OpenTask(string title, string message)
	{
		return new DelegateTask(coOpenTask(title, message));
	}

	private static IEnumerator coOpenTask(string title, string message)
	{
		bool closed = false;
		Open(title, message, delegate
		{
			closed = true;
		});
		while (!closed)
		{
			yield return null;
		}
	}

	public static ITask OpenErrorTask(Enum error)
	{
		return new DelegateTask(coOpenErrorTask(error));
	}

	private static IEnumerator coOpenErrorTask(Enum error)
	{
		bool closed = false;
		OpenError(error, delegate
		{
			closed = true;
		});
		while (!closed)
		{
			yield return null;
		}
	}

	public static bool IsPopupExist(string title, string message)
	{
		return GlobalAutoBehaviour<PopupManegement>.instance.Find(delegate(IWindowPrefab popup)
		{
			if (!(popup is WindowPrefab<MessagePopup> windowPrefab))
			{
				return false;
			}
			return (string.Equals(windowPrefab.instance.Title, title) && string.Equals(windowPrefab.instance.Message, message)) ? true : false;
		}) != null;
	}

	public static GameUIRoot.ELayerType GetCurrentLayerType()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			MissionMilestonePopup.ReleaseWindow();
			return GameUIRoot.ELayerType.UI3DUI;
		}
		return GameUIRoot.ELayerType.UI;
	}

	public void OpenInternalYesOnly(string title, string message, Action onYes, bool ignoreOnClickClose, bool showOnClickClose)
	{
		base.Open();
		_ignoreOnClickClose = ignoreOnClickClose;
		_onYes = onYes;
		_onNo = null;
		_onClose = null;
		UIHelper.EnableGameObject(ButtonOk, enable: false);
		UIHelper.EnableGameObject(ButtonYes, enable: true);
		UIHelper.EnableGameObject(ButtonNo, enable: false);
		UIHelper.EnableButton(ButtonYes, enable: true);
		UIHelper.EnableGameObject(ButtonClose, showOnClickClose);
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		SetYesButtonLabel(LocalizationManager.GetStringDirect("COMMON_YES"));
		Title = title;
		Message = message;
	}

	public void OpenInternalYesOnly(string title, string message, Action onYes, bool ignoreOnClickClose)
	{
		base.Open();
		_ignoreOnClickClose = ignoreOnClickClose;
		_onYes = onYes;
		_onNo = null;
		_onClose = null;
		UIHelper.EnableGameObject(ButtonOk, enable: false);
		UIHelper.EnableGameObject(ButtonYes, enable: true);
		UIHelper.EnableGameObject(ButtonNo, enable: false);
		UIHelper.EnableButton(ButtonYes, enable: true);
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		SetYesButtonLabel(LocalizationManager.GetStringDirect("COMMON_YES"));
		Title = title;
		Message = message;
	}

	private void OpenInternalYesOnly(string title, string message, Action onYes, string yesLabel, bool ignoreBackButton, bool ignoreOnClickClose)
	{
		base.Open();
		_ignoreOnClickClose = ignoreOnClickClose;
		_ignoreBackButton = ignoreBackButton;
		_onYes = onYes;
		_onNo = null;
		_onClose = null;
		UIHelper.EnableGameObject(ButtonOk, enable: false);
		UIHelper.EnableGameObject(ButtonYes, enable: true);
		UIHelper.EnableGameObject(ButtonNo, enable: false);
		UIHelper.EnableButton(ButtonYes, enable: true);
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		string yesButtonLabel = (String_Extension_Method.IsNullOrEmpty(yesLabel) ? LocalizationManager.GetStringDirect("COMMON_YES") : yesLabel);
		SetYesButtonLabel(yesButtonLabel);
		Title = title;
		Message = message;
	}

	protected void OpenInternal(string title, string message, Action onOK = null, bool retryButton = false)
	{
		base.Open();
		_onClose = onOK;
		_onYes = null;
		_onNo = null;
		UIHelper.EnableGameObject(ButtonOk, enable: true);
		UIHelper.EnableGameObject(ButtonYes, enable: false);
		UIHelper.EnableGameObject(ButtonNo, enable: false);
		UIHelper.EnableGameObject(LabelOk, !retryButton);
		UIHelper.EnableGameObject(LabelRetry, retryButton);
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		Title = title;
		Message = message;
	}

	protected void OpenInternal(string title, string message, Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		_onClose = null;
		UIHelper.EnableGameObject(ButtonOk, enable: false);
		UIHelper.EnableGameObject(ButtonYes, enable: true);
		UIHelper.EnableGameObject(ButtonNo, enable: true);
		UIHelper.EnableButton(ButtonYes, enable: true);
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		SetYesButtonLabel(LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES));
		SetNoButtonLabel(LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO));
		Title = title;
		Message = message;
	}

	protected void OpenInternal(string title, string message, Action onYes, Action onNo, Enum YesLabelKey, Enum NoLabelKey)
	{
		OpenInternal(title, message, onYes, onNo);
		SetYesButtonLabel(LocalizationManager.GetString(YesLabelKey));
		SetNoButtonLabel(LocalizationManager.GetString(NoLabelKey));
	}

	protected void OpenInternal(string title, string message, string message2, Action onYes, Action onNo)
	{
		OpenInternal(title, message, onYes, onNo);
		UIHelper.SetLabelText(MessageLabel2, message2);
	}

	protected void OpenInternal(string title, string message, Action onYes, Action onNo, string YesLabelKey, bool bYesEnabled)
	{
		OpenInternal(title, message, onYes, onNo);
		SetYesButtonLabel(LocalizationManager.GetStringDirect(YesLabelKey));
		SetNoButtonLabel(LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO));
		UIHelper.EnableButton(ButtonYes, bYesEnabled);
	}

	protected void OpenInternal(string title, string message, Action onYes, Action onNo, bool ignoreOnClickClose)
	{
		OpenInternal(title, message, onYes, onNo);
		_ignoreOnClickClose = ignoreOnClickClose;
	}

	public void SetYesButtonLabel(string text)
	{
		UIHelper.SetLabelText(LabelYes, text);
	}

	public void SetOKButtonLabel(string text)
	{
		UIHelper.SetLabelText(LabelOk, text);
	}

	public void SetNoButtonLabel(string text)
	{
		UIHelper.SetLabelText(LabelNo, text);
	}

	public override void Reset()
	{
		base.Reset();
		_onClose = null;
		_onYes = null;
		_onNo = null;
		_ignoreOnClickClose = false;
		_ignoreBackButton = false;
	}

	public override void Close()
	{
		Action onNo = _onNo;
		Action onClose = _onClose;
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
		Vespa_System_Action_Extension_Method.fire(onClose);
	}

	public void OnClickClose()
	{
		if (!IgnoreOnClickClose())
		{
			OnClickNo();
		}
	}

	private bool IgnoreOnClickClose()
	{
		if (_onClose == null && _onNo == null)
		{
			return _ignoreOnClickClose;
		}
		return false;
	}

	public void OnClickYes()
	{
		Action onYes = _onYes;
		Action onClose = _onClose;
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
		Vespa_System_Action_Extension_Method.fire(onClose);
	}

	public void OnClickNo()
	{
		Action onNo = _onNo;
		Action onClose = _onClose;
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
		Vespa_System_Action_Extension_Method.fire(onClose);
	}

	public override void OnSystemBackButton()
	{
		if (!_ignoreBackButton)
		{
			OnClickNo();
		}
	}

	public void OnClickCopy()
	{
		UniClipboard.SetText("message1\n" + MessageLabel.text + "\n\nmessage2\n" + MessageLabel2.text);
	}
}
