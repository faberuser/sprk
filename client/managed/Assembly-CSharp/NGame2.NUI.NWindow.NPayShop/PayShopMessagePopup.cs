using System;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

[UIEvent(AllowDuringPatchDownloading = true)]
public class PayShopMessagePopup : MessagePopup
{
	public new const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopMessagePopup";

	public new static PayShopMessagePopup Open(string title, string message)
	{
		return Open(title, message, null);
	}

	public new static PayShopMessagePopup Open(string title, string message, Action onOk)
	{
		WindowPrefab<PayShopMessagePopup> windowPrefab = new WindowPrefab<PayShopMessagePopup>("Resources/Prefab2/Window/PayShop/PayShopMessagePopup", MessagePopup.GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onOk);
		windowPrefab.instance.SetOKButtonLabel(LocalizationManager.GetStringDirect("COMMON_CONFIRM"));
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public new static PayShopMessagePopup OpenYesOnly(string title, string message, Action onYes, bool ignoreOnClickClose = false)
	{
		WindowPrefab<PayShopMessagePopup> windowPrefab = new WindowPrefab<PayShopMessagePopup>("Resources/Prefab2/Window/PayShop/PayShopMessagePopup", MessagePopup.GetCurrentLayerType());
		windowPrefab.instance.OpenInternalYesOnly(title, message, onYes, ignoreOnClickClose);
		windowPrefab.instance.SetYesButtonLabel(LocalizationManager.GetStringDirect("COMMON_CONFIRM"));
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public new static PayShopMessagePopup OpenError(Enum error, Action onOk = null)
	{
		if (error.GetType() == typeof(BaseResultType) && (BaseResultType)(object)error == BaseResultType.Disabled)
		{
			return null;
		}
		string text = LocalizationManager.GetString(error);
		string text2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		if (string.IsNullOrEmpty(text))
		{
			text = text2;
		}
		VespaDebug.LogError(text + "\n" + StackTraceUtility.ExtractStackTrace());
		return Open(text2, text, onOk);
	}

	public new static void OpenEnabled(string title, string message, Action onYes, Action onNo, string YesLabelKey, bool bYesEnabled)
	{
		WindowPrefab<PayShopMessagePopup> windowPrefab = new WindowPrefab<PayShopMessagePopup>("Resources/Prefab2/Window/PayShop/PayShopMessagePopup", MessagePopup.GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(title, message, onYes, onNo, YesLabelKey, bYesEnabled);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}
}
