using System;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NCommon;

public class YesNoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab3/Common/YesNoPopup";

	private const int MinWidth = 800;

	private const int MinHeight = 100;

	public UILabel Label_Title;

	public UILabel Label_Message;

	public UIWidget Widget_GroupMessage;

	public UIWidget Widget_GroupFrame;

	private Action _onYes;

	private Action _onNo;

	public static void Open(string message, Action onYes = null, Action onNo = null)
	{
		Open(LocalizationManager.GetStringDirect("COMMON_NOTIFICATION"), message, onYes, onNo);
	}

	public static void Open(string title, string message, Action onYes = null, Action onNo = null)
	{
		WindowPrefab<YesNoPopup> windowPrefab = new WindowPrefab<YesNoPopup>("Resources/Prefab3/Common/YesNoPopup", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
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

	protected void OpenInternal(string title, string message, Action onYes, Action onNo = null)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		UIHelper.SetLabelText(Label_Title, title);
		UIHelper.SetLabelText(Label_Message, message);
		if (Label_Message != null && Widget_GroupMessage != null && Widget_GroupFrame != null)
		{
			int width = Label_Message.width;
			int height = Label_Message.height;
			Widget_GroupMessage.width = Mathf.Max(800, width);
			Widget_GroupMessage.height = Mathf.Max(100, height);
			Widget_GroupFrame.ForceUpdateAnchors(includeChildren: true);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onYes = null;
		_onNo = null;
	}

	public override void Close()
	{
		Action onNo = _onNo;
		_onYes = null;
		_onNo = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
	}

	public void OnClick_Close()
	{
		OnClick_No();
	}

	public void OnClick_Yes()
	{
		Action onYes = _onYes;
		_onYes = null;
		_onNo = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
	}

	public void OnClick_No()
	{
		Action onNo = _onNo;
		_onYes = null;
		_onNo = null;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
	}

	public override void OnSystemBackButton()
	{
		OnClick_No();
	}
}
