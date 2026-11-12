using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUtil.NPayShop;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopPriceMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopPriceMessagePopup";

	public UILabel Label_Price;

	public UIWidget IconProxyPos;

	public UILabel Label_Mileage;

	private IconProxy _iconProxy;

	private Action<bool> _callback;

	public static void Open(NewPayShopProductInfo info, Action<bool> callback = null)
	{
		CurrencyType priceType = info.PriceType;
		long price = info.Price;
		int mileage = info.Mileage;
		Open(priceType, price, mileage, callback);
	}

	public static void Open(CreatureData data, Action<bool> callback = null)
	{
		int price = PayShopHeroComponent.GetPrice(data);
		int milieage = PayShopHeroComponent.GetMilieage(data);
		long mileage = milieage;
		Open(CurrencyType.Gem, price, mileage, callback);
	}

	public static void Open(NewEquipGachaData data, Action<bool> callback = null)
	{
		int gemApplyValidDiscounts = Calculator.GetGemApplyValidDiscounts(data);
		gemApplyValidDiscounts = Calculator.GetStepUpDiscount(data, gemApplyValidDiscounts);
		int mileageApplyValidMileage = Calculator.GetMileageApplyValidMileage(data);
		long mileage = mileageApplyValidMileage;
		mileage = Calculator.GetStepUpMileage(data, mileage);
		Open(CurrencyType.Gem, gemApplyValidDiscounts, mileage, callback);
	}

	public static void Open(CurrencyType currencyType, long price, long mileage = 0, Action<bool> callback = null)
	{
		long currencyCount = price;
		UIHelper.BuyConfirmPopupOpen(currencyType, currencyCount, delegate
		{
			WindowPrefab<PayShopPriceMessagePopup> windowPrefab = new WindowPrefab<PayShopPriceMessagePopup>("Resources/Prefab2/Window/PayShop/PayShopPriceMessagePopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(currencyType, price, mileage, callback);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}, delegate
		{
			Vespa_System_Action_Extension_Method.fire(callback, i_argA: true);
		});
	}

	public void OpenInternal(CurrencyType currencyType, long price, long mileage = 0, Action<bool> callback = null)
	{
		base.Open();
		_callback = callback;
		if (_iconProxy == null)
		{
			_iconProxy = IconProxy.Create(IconProxyPos);
		}
		_iconProxy.SetupCurrencyNoBg(currencyType);
		UIHelper.SetLabelText(Label_Price, Vespa_Long_Extension_Method.ToCommaString(price));
		string stringDirect = LocalizationManager.GetStringDirect("PAY.SHOP.GEM_MILEAGE_OBTAIN_MSG", mileage);
		UIHelper.SetLabelText(Label_Mileage, stringDirect);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
	}

	public void OnClickBuy()
	{
		Action<bool> callback = _callback;
		_callback = null;
		Vespa_System_Action_Extension_Method.fire(callback, i_argA: true);
		Close();
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void Close()
	{
		if (_callback != null)
		{
			Action<bool> callback = _callback;
			_callback = null;
			Vespa_System_Action_Extension_Method.fire(callback, i_argA: false);
		}
		base.Close();
	}
}
