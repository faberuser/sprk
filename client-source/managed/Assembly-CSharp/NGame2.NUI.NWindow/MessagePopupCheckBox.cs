using System;
using NGame2.NUI.NManager;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MessagePopupCheckBox : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MessagePopupCheckBox";

	private static GlobalWindowPrefab<MessagePopupCheckBox> _windowPrefab = new GlobalWindowPrefab<MessagePopupCheckBox>("Resources/Prefab2/Window/MessagePopup/MessagePopupCheckBox", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel LabelTitle;

	public UILabel LabelMessage;

	public UIButton ButtonOk;

	public UIButton ButtonClose;

	public Transform TransformCheck;

	private Action _onClose;

	private Action _onYes;

	private Action _onNo;

	private LocalAccountBool _localValue;

	public override void Reset()
	{
		base.Reset();
		_onClose = null;
		_onYes = null;
		_onNo = null;
		UIHelper.EnableGameObject(TransformCheck, enable: false);
	}

	public static void Open(string message, Action onYes, LocalAccountBool localValue)
	{
		if (localValue.Value)
		{
			Vespa_System_Action_Extension_Method.fire(onYes);
		}
		else
		{
			Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message, onYes, localValue);
		}
	}

	public static void Open(string title, string message, Action onYes, LocalAccountBool localValue)
	{
		WindowPrefab<MessagePopupCheckBox> windowPrefab = new WindowPrefab<MessagePopupCheckBox>("Resources/Prefab2/Window/MessagePopup/MessagePopupCheckBox", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, onYes, localValue);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string title, string message, Action onYes, LocalAccountBool localValue)
	{
		base.Open();
		_onYes = onYes;
		_onNo = null;
		_onClose = null;
		_localValue = localValue;
		UIHelper.EnableGameObject(ButtonOk, enable: true);
		UIHelper.EnableGameObject(TransformCheck, _localValue.Value);
		UIHelper.SetLabelText(LabelTitle, title);
		UIHelper.SetLabelText(LabelMessage, message);
	}

	public override void Close()
	{
		Action onNo = _onNo;
		Action onClose = _onClose;
		_onYes = null;
		_onNo = null;
		_onClose = null;
		_localValue.Value = TransformCheck.gameObject.activeSelf;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
		Vespa_System_Action_Extension_Method.fire(onClose);
	}

	public void OnClickClose()
	{
		OnClickNo();
	}

	public void OnClickYes()
	{
		Action onYes = _onYes;
		Action onClose = _onClose;
		_onYes = null;
		_onNo = null;
		_onClose = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onClose);
		Vespa_System_Action_Extension_Method.fire(onYes);
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
		OnClickNo();
	}

	public void OnClickCheckBox()
	{
		_localValue.Value = !_localValue.Value;
		UIHelper.EnableGameObject(TransformCheck, _localValue.Value);
	}
}
