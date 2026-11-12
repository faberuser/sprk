using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopPriceCustomMessagePopup : BaseWindow
{
	public class DataClass
	{
		public string Title;

		public string Message;

		public string YesLabel;

		public string Currency;

		public long Price;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopPriceCustomMessagePopup";

	public UILabel Label_Title;

	public UILabel Label_Message;

	public UILabel Label_Yes;

	public UILabel Label_Price;

	public UIWidget IconProxyPos;

	private IconProxy _iconProxy;

	private Action<bool> _callback;

	public static void Open(DataClass data, Action<bool> callback = null)
	{
		WindowPrefab<PayShopPriceCustomMessagePopup> windowPrefab = new WindowPrefab<PayShopPriceCustomMessagePopup>("Resources/Prefab2/Window/PayShop/PayShopPriceCustomMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(data, callback);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(DataClass data, Action<bool> callback = null)
	{
		base.Open();
		_callback = callback;
		string text = data.Title;
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		}
		UIHelper.SetLabelText(Label_Title, text);
		UIHelper.SetLabelText(Label_Message, data.Message);
		string text2 = data.YesLabel;
		if (String_Extension_Method.IsNullOrEmpty(text2))
		{
			text2 = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
		}
		UIHelper.SetLabelText(Label_Yes, text2);
		if (_iconProxy == null)
		{
			_iconProxy = IconProxy.Create(IconProxyPos);
		}
		_iconProxy.SetupCurrencyNoBg(data.Currency);
		UIHelper.SetLabelText(Label_Price, Vespa_Long_Extension_Method.ToCommaString(data.Price));
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_callback, i_argA: true);
		Close();
	}

	public void OnClickClose()
	{
		Vespa_System_Action_Extension_Method.fire(_callback, i_argA: false);
		Close();
	}
}
