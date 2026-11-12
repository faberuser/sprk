using System;
using NGame2.NUI.NManager;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class AdultMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/AdultMessagePopup";

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIButton Button1;

	public UIButton Button2;

	public UILabel ButtonLabel1;

	public UILabel ButtonLabel2;

	private Action _onButton1;

	private Action _onButton2;

	public string Title { get; set; }

	public string Message { get; set; }

	public static void Open(string title, string message, Action onButton1, Action onButton2, Enum button1Label, Enum button2Label)
	{
		string button1Label2 = LocalizationManager.GetString(button1Label);
		string button2Label2 = LocalizationManager.GetString(button2Label);
		Open(title, message, onButton1, onButton2, button1Label2, button2Label2);
	}

	public static void Open(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label)
	{
		WindowPrefab<AdultMessagePopup> windowPrefab = new WindowPrefab<AdultMessagePopup>("Resources/Prefab2/Window/MessagePopup/AdultMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, button1Label, button2Label);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string title, string message, Action onButton1, Action onButton2, string button1Label, string button2Label)
	{
		base.Open();
		_onButton1 = onButton1;
		_onButton2 = onButton2;
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
	}

	public void OnClickClose()
	{
		Close();
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
}
