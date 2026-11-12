using System;
using NGame2.NUI.NManager;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopMessage3ButtonPopup : MessagePopup3Button
{
	public new const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopMessage3ButtonPopup";

	[Header("[Portal Group]")]
	public Transform Portal_Message;

	public UILabel Label_Name;

	public UILabel Label_Message;

	public static bool isPortalStyle { get; private set; }

	public new static void Open(string title, string message, Action onButton1 = null, Action onButton2 = null, string button1Label = "", string button2Label = "")
	{
		if (String_Extension_Method.IsNullOrEmpty(button1Label))
		{
			button1Label = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		}
		if (String_Extension_Method.IsNullOrEmpty(button2Label))
		{
			button2Label = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		}
		WindowPrefab<PayShopMessage3ButtonPopup> windowPrefab = new WindowPrefab<PayShopMessage3ButtonPopup>("Resources/Prefab2/Window/PayShop/PayShopMessage3ButtonPopup", MessagePopup.GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onButton1, onButton2, button1Label, button2Label);
		UIHelper.EnableGameObject(windowPrefab.instance.Portal_Message, enable: false);
		UIHelper.EnableGameObject(windowPrefab.instance.MessageLabel, enable: true);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		isPortalStyle = false;
	}

	public static void OpenPortalStyle(string title, string chapterName, string subMsg, Action onButton1 = null, Action onButton2 = null, string button1Label = "", string button2Label = "")
	{
		if (String_Extension_Method.IsNullOrEmpty(button1Label))
		{
			button1Label = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		}
		if (String_Extension_Method.IsNullOrEmpty(button2Label))
		{
			button2Label = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		}
		WindowPrefab<PayShopMessage3ButtonPopup> windowPrefab = new WindowPrefab<PayShopMessage3ButtonPopup>("Resources/Prefab2/Window/PayShop/PayShopMessage3ButtonPopup", MessagePopup.GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, chapterName, onButton1, onButton2, button1Label, button2Label);
		UIHelper.EnableGameObject(windowPrefab.instance.Portal_Message, enable: true);
		UIHelper.EnableGameObject(windowPrefab.instance.MessageLabel, enable: false);
		UIHelper.SetLabelText(windowPrefab.instance.Label_Name, chapterName);
		UIHelper.SetLabelText(windowPrefab.instance.Label_Message, subMsg);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		isPortalStyle = true;
	}

	public static void ClosePortalMovePopup()
	{
		PayShopMessage3ButtonPopup payShopMessage3ButtonPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<PayShopMessage3ButtonPopup>();
		if (!(payShopMessage3ButtonPopup == null) && isPortalStyle)
		{
			payShopMessage3ButtonPopup.Close2(immediately: true);
		}
	}
}
