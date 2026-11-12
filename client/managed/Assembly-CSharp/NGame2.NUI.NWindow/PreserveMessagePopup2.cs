using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NPayShopItemEvent;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PreserveMessagePopup2 : BaseWindow, IUserObserver<PayShopItemEventManager.EventChangedInfo>, IUserObserver
{
	public enum EPriceType
	{
		None,
		Gold,
		Gem,
		Mileage
	}

	[Serializable]
	public class PriceRoot
	{
		public EPriceType PriceType;

		public Transform Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2";

	public const string PrefabPath2 = "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup3";

	public const string PrefabPath3 = "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup4";

	public const string PrefabPath4 = "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup5";

	public UILabel Label_Title;

	public UILabel Label_Content;

	public UILabel Label_Yes;

	public UIButton Button_Yes;

	public PriceRoot[] PriceRoots;

	public UILabel Label_Price;

	public UILabel Label_MilieageObtain;

	private Action _onYes;

	private Action _onCancel;

	private Action _onSystemBackButton;

	private List<IPayShopEventSearchInfo> _list;

	public static CurrencyType GetCurrencyType(EPriceType priceType)
	{
		return priceType switch
		{
			EPriceType.Gem => CurrencyType.Gem, 
			EPriceType.Gold => CurrencyType.Gold, 
			EPriceType.Mileage => CurrencyType.Mileage, 
			_ => CurrencyType.None, 
		};
	}

	public static void ForcedOpen(string title, string content, string yes, EPriceType priceType, int priceValue, Action onYes, int obtainMileage = 0, Action cancel = null, bool useUnderLabel = false, bool isEnabled = true, bool isEnoghPrice = true)
	{
		GetCurrencyType(priceType);
		WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>(useUnderLabel ? "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup3" : "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, cancel, null, obtainMileage, isEnabled, isEnoghPrice);
		windowPrefab.instance.SetList(null);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(string title, string content, EPriceType priceType, int priceValue, Action onYes, Action onCancel = null)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_ASK_EQUIP_PRESERVE");
			windowPrefab.instance.OpenInternal(title, content, stringDirect, priceType, priceValue, onYes, onCancel, null);
			windowPrefab.instance.SetList(null);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void Open(string title, string content, EPriceType priceType, int priceValue, string yes, Action onYes, Action onCancel = null)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, onCancel, null);
			windowPrefab.instance.SetList(null);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void Open(string title, string content, string yes, EPriceType priceType, int priceValue, Action onYes, int obtainMileage = 0, Action cancel = null, bool useUnderLabel = false, bool isEnabled = true, bool isEnoghPrice = true)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>(useUnderLabel ? "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup3" : "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, cancel, null, obtainMileage, isEnabled, isEnoghPrice);
			windowPrefab.instance.SetList(null);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void OpenPopup4(string title, string content, string yes, EPriceType priceType, int priceValue, int obtainMileage, List<IPayShopEventSearchInfo> list, Action onYes)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup4", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, null, null, obtainMileage);
			windowPrefab.instance.SetList(list);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void OpenPopup5(string title, string content, string yes, EPriceType priceType, int priceValue, int obtainMileage, List<IPayShopEventSearchInfo> list, Action onYes)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup5", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, null, null, obtainMileage);
			windowPrefab.instance.SetList(list);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void OpenPopupBuyHero(string title, string content, string yes, EPriceType priceType, int priceValue, int obtainMileage, List<IPayShopEventSearchInfo> list, Action onYes)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, null, null, obtainMileage);
			windowPrefab.instance.SetList(list);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void OpenPriceConfirm(string title, string content, string yes, EPriceType priceType, int priceValue, Action onYes, bool isEnoghPrice = true)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, null, null, 0, isEnabled: true, isEnoghPrice);
			windowPrefab.instance.SetList(null);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void OpenEnabled(string title, string content, string yes, EPriceType priceType, int priceValue, Action onYes, bool isEnabled)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(title, content, yes, priceType, priceValue, onYes, null, null, 0, isEnabled);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	public static void OpenByHeroPresetSlot(EPriceType priceType, int priceValue, Action onYes, Action onCancel = null)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		int num = priceValue;
		Action obj = delegate
		{
			WindowPrefab<PreserveMessagePopup2> windowPrefab = new WindowPrefab<PreserveMessagePopup2>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup2", GameUIRoot.ELayerType.UI);
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CONFIRM);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_PRESET_PRUCHASE_CONFIRM");
			string yes = LocalizationManager.GetString(Inventory.EQUIP_EXTEND_ACTION);
			long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
			int num3 = priceValue;
			bool isEnoghPrice = totalGem >= num3;
			windowPrefab.instance.OpenInternal(text, stringDirect, yes, priceType, priceValue, onYes, onCancel, onCancel, 0, isEnabled: true, isEnoghPrice);
			windowPrefab.instance.SetList(null);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(currencyType, num2, openAction, skipAction);
	}

	protected void OpenInternal(string title, string content, string yes, EPriceType priceType, int priceValue, Action onYes, Action onCancel, Action onBackButton, int obtainMileage = 0, bool isEnabled = true, bool isEnoghPrice = true)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		_onSystemBackButton = onBackButton;
		Color color = (isEnoghPrice ? Color.white : Color.red);
		UIHelper.SetLabelText(Label_Title, title);
		UIHelper.SetLabelText(Label_Content, content);
		UIHelper.SetLabelText(Label_Yes, yes);
		UIHelper.SetLabelText(Label_Price, Vespa_Int_Extension_Method.ToCommaString(priceValue));
		UIHelper.SetLabelColor(Label_Price, color);
		UIHelper.EnableButton(Button_Yes, isEnabled);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PriceRoots); i++)
		{
			UIHelper.EnableGameObject(PriceRoots[i].Root, PriceRoots[i].PriceType == priceType);
		}
		if ((bool)Label_MilieageObtain)
		{
			if (obtainMileage > 0)
			{
				Label_MilieageObtain.gameObject.SetActive(value: true);
				string stringDirect = LocalizationManager.GetStringDirect("PAY.SHOP.GEM_MILEAGE_OBTAIN_MSG", obtainMileage);
				UIHelper.SetLabelText(Label_MilieageObtain, stringDirect);
			}
			else
			{
				Label_MilieageObtain.gameObject.SetActive(value: false);
			}
		}
	}

	private void SetList(List<IPayShopEventSearchInfo> list)
	{
		_list = list;
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		Vespa_System_Action_Extension_Method.fire(_onSystemBackButton);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void OnClickYes()
	{
		Action onYes = _onYes;
		Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
	}

	public void OnClickCancel()
	{
		Action onCancel = _onCancel;
		Close();
		Vespa_System_Action_Extension_Method.fire(onCancel);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<PayShopItemEventManager.EventChangedInfo>.OnChanged(PayShopItemEventManager.EventChangedInfo changedInfo)
	{
		int num = 0;
		long num2 = num;
		int num3 = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			int price = 0;
			int mileage = 0;
			CalChangeValues(_list[i], out price, out mileage);
			long num4 = num2;
			int num5 = price;
			num2 = checked(num4 + num5);
			num3 += mileage;
		}
		UIHelper.SetLabelText(Label_Price, Vespa_Int_Extension_Method.ToCommaString(num2));
		if ((bool)Label_MilieageObtain)
		{
			if (num3 > 0)
			{
				Label_MilieageObtain.gameObject.SetActive(value: true);
				string stringDirect = LocalizationManager.GetStringDirect("PAY.SHOP.GEM_MILEAGE_OBTAIN_MSG", num3);
				UIHelper.SetLabelText(Label_MilieageObtain, stringDirect);
			}
			else
			{
				Label_MilieageObtain.gameObject.SetActive(value: false);
			}
		}
	}

	private void CalChangeValues(IPayShopEventSearchInfo info, out int price, out int mileage)
	{
		price = 0;
		mileage = 0;
		if (info is CreatureData)
		{
			CreatureData creatureData = info as CreatureData;
			price = PayShopHelper.GetPrice(creatureData);
			mileage = PayShopHelper.GetMilieage(creatureData);
		}
		else if (info is CostumeData)
		{
			CostumeData costumeData = info as CostumeData;
			price = HeroCostume.GetPriceGem(costumeData);
			mileage = HeroCostume.GetObtainMileage(costumeData);
		}
		else if (info is AccessoryCostumeData)
		{
			AccessoryCostumeData costumeData2 = info as AccessoryCostumeData;
			price = AccessoryCostume.GetPriceGem(costumeData2);
			mileage = AccessoryCostume.GetObtainMileage(costumeData2);
		}
		else if (info is HairCostumeData)
		{
			HairCostumeData costumeData3 = info as HairCostumeData;
			price = HairCostume.GetPriceGem(costumeData3);
			mileage = HairCostume.GetObtainMileage(costumeData3);
		}
		else if (info is WeaponCostumeData)
		{
			WeaponCostumeData costumeData4 = info as WeaponCostumeData;
			price = WeaponCostume.GetPriceGem(costumeData4);
			mileage = WeaponCostume.GetObtainMileage(costumeData4);
		}
	}
}
