using System.Collections.Generic;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopProductSimpleComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopProductSimpleComponent";

	public static PrefabAuto<PayShopProductSimpleComponent> PrefabAuto = new PrefabAuto<PayShopProductSimpleComponent>("Prefab2/Component/PayShop2/PayShopProductSimpleComponent");

	public Transform ItemIconPos;

	public UILabel Label_Name;

	private ItemIconComponent _itemIconComponent;

	private CurrencyIconComponent _currencyIconComponent;

	private static readonly Dictionary<CurrencyType, NShared.NLocalization.Common> CurrencyKeyDics = new Dictionary<CurrencyType, NShared.NLocalization.Common>
	{
		{
			CurrencyType.Gem,
			NShared.NLocalization.Common.RUBY_COUNT
		},
		{
			CurrencyType.Gold,
			NShared.NLocalization.Common.GOLD_COUNT
		},
		{
			CurrencyType.Stamina,
			NShared.NLocalization.Common.STAMINA_COUNT
		},
		{
			CurrencyType.Mileage,
			NShared.NLocalization.Common.MILEAGE_COUNT
		},
		{
			CurrencyType.LuaPoint,
			NShared.NLocalization.Common.LUA_POINT_COUNT
		},
		{
			CurrencyType.ShopEventPoint,
			NShared.NLocalization.Common.SHOP_EVENT_POINT_COUNT
		},
		{
			CurrencyType.LimitedShopEventPoint,
			NShared.NLocalization.Common.SHOP_EVENT_POINT_LIMITED_COUNT
		}
	};

	public ObtainItemType ObtainType { get; private set; }

	public int SortIndex { get; set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		FactoryBase.SafeReleaseComponent(ref _currencyIconComponent);
	}

	public void Open(ItemData itemData, long Count, ObtainItemType obtainType = ObtainItemType.BaseItem)
	{
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconPos);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		_itemIconComponent.Open(itemData);
		ObtainType = obtainType;
		SetName(itemData, Count);
	}

	public void Open(CurrencyType currencyType, long Count, ObtainItemType obtainType)
	{
		_currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(ItemIconPos);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_currencyIconComponent.transform);
		CurrencyIconComponent currencyIconComponent = _currencyIconComponent;
		CurrencyIconComponent.Listener listener = null;
		int num = 0;
		currencyIconComponent.Open(currencyType, num, listener);
		ObtainType = obtainType;
		SetName(currencyType, Count);
	}

	private void SetName(ItemData itemData, long Count)
	{
		string text = ItemNameBuilder.GetItemName(itemData) + "X" + Count;
		SetName(text);
	}

	public void SetName(string name)
	{
		UIHelper.SetLabelText(Label_Name, name);
	}

	private void SetName(CurrencyType currencyType, long Count)
	{
		string empty = string.Empty;
		string text = Vespa_Int_Extension_Method.ToCommaString(Count);
		if (!CurrencyKeyDics.ContainsKey(currencyType))
		{
			empty = "Not Applied CurrencyType=" + currencyType;
			VespaDebug.LogError(empty);
		}
		else
		{
			empty = LocalizationManager.GetString(CurrencyKeyDics[currencyType], text);
		}
		SetName(empty);
	}

	public void OpenError(int itemIndex)
	{
		SetName("[FF0000]<Error>[-]" + itemIndex);
	}

	public void OpenError(string itemCode)
	{
		SetName("[FF0000]<Error>[-]" + itemCode);
	}
}
