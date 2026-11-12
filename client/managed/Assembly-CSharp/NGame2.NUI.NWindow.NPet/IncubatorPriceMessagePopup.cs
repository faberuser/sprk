using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPet;

public class IncubatorPriceMessagePopup : PayShopPriceMessagePopup
{
	public enum EIncubatorPriceType
	{
		None,
		Slot,
		Incubator
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Pet/IncubatorPriceMessagePopup";

	public UILabel Label_Message;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	private Color _colorDefault;

	public static void Open(EIncubatorPriceType incubatorPriceType, CurrencyType currencyType, long price, int mileage = 0, Action<bool> callback = null, int incubatorIndex = 0)
	{
		if (incubatorPriceType != EIncubatorPriceType.None)
		{
			WindowPrefab<IncubatorPriceMessagePopup> windowPrefab = new WindowPrefab<IncubatorPriceMessagePopup>("Resources/Prefab2/Window/Pet/IncubatorPriceMessagePopup", GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(currencyType, price, mileage, callback);
			windowPrefab.instance.Init(currencyType, price);
			windowPrefab.instance.SetMessage(incubatorPriceType, currencyType, mileage, incubatorIndex);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	private void Init(CurrencyType currencyType, long price)
	{
		bool flag = true;
		switch (currencyType)
		{
		case CurrencyType.Gold:
			flag = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= price;
			break;
		case CurrencyType.Gem:
			flag = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gem >= price;
			break;
		}
		Color color = (flag ? ColorGoldEnough : ColorGoldNotEnough);
		_colorDefault = Label_Price.color;
		UIHelper.SetLabelColor(Label_Price, color);
	}

	public override void Close()
	{
		UIHelper.SetLabelColor(Label_Price, _colorDefault);
		base.Close();
	}

	public void SetMessage(EIncubatorPriceType incubatorPriceType, CurrencyType currencyType, int mileage = 0, int incubatorIndex = 0)
	{
		string stringDirect = LocalizationManager.GetStringDirect("PAY.SHOP.GEM_MILEAGE_OBTAIN_MSG", mileage);
		UIHelper.SetLabelText(Label_Mileage, stringDirect);
		UIHelper.EnableGameObject(Label_Mileage, currencyType == CurrencyType.Gem);
		string empty = string.Empty;
		switch (incubatorPriceType)
		{
		case EIncubatorPriceType.Slot:
			empty = ((currencyType == CurrencyType.Gem) ? LocalizationManager.GetStringDirect("PETINCUBATOR_SLOT_PURCHASE_GEM_CONFIRM_NOTI") : LocalizationManager.GetStringDirect("PETINCUBATOR_SLOT_PURCHASE_GOLD_CONFIRM_NOTI"));
			UIHelper.SetLabelText(Label_Message, empty);
			break;
		case EIncubatorPriceType.Incubator:
		{
			PetIncubatorData data = GameRoot.Instance.PetIncubatorDataContainer.GetData(incubatorIndex);
			if (data == null)
			{
				VespaDebug.LogError($"Incubator Data is Null : index = {incubatorIndex}");
				break;
			}
			string stringDirect2 = LocalizationManager.GetStringDirect(data.NameLocalKey);
			empty = ((currencyType == CurrencyType.Gem) ? LocalizationManager.GetStringDirect("PETINCUBATOR_PURCHASE_GEM_CONFIRM_NOTI", stringDirect2) : LocalizationManager.GetStringDirect("PETINCUBATOR_PURCHASE_GOLD_CONFIRM_NOTI"));
			UIHelper.SetLabelText(Label_Message, empty);
			break;
		}
		}
	}
}
