using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildRaidBootyItemComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate, ItemIconComponent.Listener
{
	public interface Listener
	{
		void OnClickComponent(GuildRaidBootyItemComponent This);
	}

	public interface IDataProvider
	{
		bool IsGreaterThanPrice(GuildRaidBootyItemComponent bootyItem);

		bool IsNew(GuildRaidBootyItemComponent bootyItem);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/GuildRaid/GuildRaidBootyItemComponent";

	public static PrefabAuto<GuildRaidBootyItemComponent> PrefabAuto = new PrefabAuto<GuildRaidBootyItemComponent>("Resources/Prefab2/Component/GuildRaid/GuildRaidBootyItemComponent");

	public UISprite Sprite_Selected;

	public UIButton Button;

	public ShopItemComponent.ItemPriceInfo PriceInfo;

	public UISimpleGrid ItemGrid;

	public UILabel Label_ItemName;

	public UILabel Label_RemainTimeUntilDestroy;

	public UIWidget WidgetNew;

	private ItemIconComponent _ItemIconComponent;

	private bool _selected;

	private Listener _listener;

	private IDataProvider _dataProvider;

	public ShopData ShopData { get; private set; }

	public GuildRaidBootyEquipItemInfo BootyEquipItemInfo { get; private set; }

	public GuildRaidBootyItemInfo BootyItemInfo { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Reset();
	}

	public bool Open(ShopData shopData, GuildRaidBootyEquipItemInfo info, Listener listener, IDataProvider dataProvider)
	{
		if (info == null)
		{
			return false;
		}
		_listener = listener;
		_dataProvider = dataProvider;
		BootyEquipItemInfo = info;
		BootyItemInfo = null;
		ShopData = shopData;
		Setup();
		return true;
	}

	public bool Open(ShopData shopData, GuildRaidBootyItemInfo info, Listener listener, IDataProvider dataProvider)
	{
		if (info == null)
		{
			return false;
		}
		_listener = listener;
		_dataProvider = dataProvider;
		BootyEquipItemInfo = null;
		BootyItemInfo = info;
		ShopData = shopData;
		Setup();
		return true;
	}

	private void Update()
	{
		SetupRemainTimeUntilDestroy();
	}

	private void Setup()
	{
		if (BootyEquipItemInfo != null || BootyItemInfo != null)
		{
			SetupBackground();
			SetupItemIcon();
			SetupPrice();
			SetupRemainTimeUntilDestroy();
			SetupNew();
		}
	}

	private void SetupBackground()
	{
		UIHelper.EnableGameObject(Sprite_Selected, enable: false);
	}

	private void SetupItemIcon()
	{
		if (_ItemIconComponent == null)
		{
			int index = 0;
			int num = 0;
			long count = num;
			int num2 = 0;
			if (BootyEquipItemInfo != null)
			{
				index = BootyEquipItemInfo.ItemIndex;
				num2 = BootyEquipItemInfo.Star;
			}
			if (BootyItemInfo != null)
			{
				index = BootyItemInfo.ItemIndex;
				int count2 = BootyItemInfo.Count;
				count = count2;
			}
			_ItemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
			_ItemIconComponent.Open(data, this);
			_ItemIconComponent.SetItemCount(count);
			_ItemIconComponent.SetStar((data != null && data.Type == ItemType.Equip) ? num2 : 0);
		}
	}

	private void SetupPrice()
	{
		int index = 0;
		if (BootyItemInfo != null)
		{
			index = BootyItemInfo.ItemIndex;
		}
		if (BootyEquipItemInfo != null)
		{
			index = BootyEquipItemInfo.ItemIndex;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
		if (data != null)
		{
			UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(data));
			if (PriceInfo.IconProxy == null)
			{
				PriceInfo.IconProxy = IconProxy.Create(PriceInfo.IconRoot);
			}
			if (PriceInfo.IconProxy != null)
			{
				PriceInfo.IconProxy.SetupCurrencyNoBg(ShopData.BuyCostType.ToString());
			}
			int price = GetPrice();
			long i_value = price;
			UIHelper.SetLabelText(PriceInfo.Label_Price, Vespa_Int_Extension_Method.ToCommaString(i_value));
			SetPriceLabelColor();
		}
	}

	private void SetupRemainTimeUntilDestroy()
	{
		string createdTimeUTC = string.Empty;
		if (BootyEquipItemInfo != null)
		{
			createdTimeUTC = BootyEquipItemInfo.CreatedTime;
		}
		if (BootyItemInfo != null)
		{
			createdTimeUTC = BootyItemInfo.CreatedTime;
		}
		string remainTimeUntilDestroy = RaidHelper.GetRemainTimeUntilDestroy(createdTimeUTC);
		UIHelper.SetLabelText(Label_RemainTimeUntilDestroy, remainTimeUntilDestroy);
	}

	public int GetPrice()
	{
		int index = 0;
		int num = 0;
		if (BootyEquipItemInfo != null)
		{
			index = BootyEquipItemInfo.ItemIndex;
			num = 1;
			_ = BootyEquipItemInfo.Star;
		}
		if (BootyItemInfo != null)
		{
			index = BootyItemInfo.ItemIndex;
			num = BootyItemInfo.Count;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
		if (data == null)
		{
			return 0;
		}
		int buyGuildPoint = data.BuyGuildPoint;
		float discountRatio = GetDiscountRatio();
		return (int)Mathf.Ceil((float)buyGuildPoint * discountRatio) * num;
	}

	private float GetDiscountRatio()
	{
		float num = 1f;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountShop, ShopData.Code);
		if (effectiveEvent != null)
		{
			int bonusPercentage = effectiveEvent.BonusPercentage;
			num *= 1f - (float)bonusPercentage * 0.01f;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(ShopData.HeroIndex);
		if (cachedHeroInfo == null)
		{
			return num;
		}
		HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
		if (data == null)
		{
			return num;
		}
		HeroEventBonusData[] heroBonusTypeDatas = data.GetHeroBonusTypeDatas(EventBonusType.DiscountShop);
		foreach (HeroEventBonusData heroEventBonusData in heroBonusTypeDatas)
		{
			if (__Extension_Method_ICollection__.COUNT(heroEventBonusData.BonusValue) == 2 && Vespa_String_Extension_Method.TOINT(heroEventBonusData.BonusValue[0], 0) == ShopData.Index)
			{
				int num2 = Vespa_String_Extension_Method.TOINT(heroEventBonusData.BonusValue[1], 0);
				num *= 1f - (float)num2 * 0.01f;
			}
		}
		return num;
	}

	public DateTime GetCreatedTime()
	{
		string i_str = string.Empty;
		if (BootyEquipItemInfo != null)
		{
			i_str = BootyEquipItemInfo.CreatedTime;
		}
		if (BootyItemInfo != null)
		{
			i_str = BootyItemInfo.CreatedTime;
		}
		DateTime o_dateTime = NGame2.NUI.NWindow.Common.GetUTCNow();
		Vespa_String_Extension_Method2.TryParseNetworkTime2(i_str, out o_dateTime);
		return o_dateTime;
	}

	public double GetRemainTime()
	{
		double num = 0.0;
		string createdTimeUTC = string.Empty;
		if (BootyEquipItemInfo != null)
		{
			createdTimeUTC = BootyEquipItemInfo.CreatedTime;
		}
		if (BootyItemInfo != null)
		{
			createdTimeUTC = BootyItemInfo.CreatedTime;
		}
		return RaidHelper.GetRemainTimeUntilDestroyTimeSpan(createdTimeUTC).Value.TotalSeconds;
	}

	public long GetItmeId()
	{
		long result = 0L;
		if (BootyEquipItemInfo != null)
		{
			result = BootyEquipItemInfo.Id;
		}
		if (BootyItemInfo != null)
		{
			result = BootyItemInfo.Id;
		}
		return result;
	}

	public int GetItmeIndex()
	{
		int result = 0;
		if (BootyEquipItemInfo != null)
		{
			result = BootyEquipItemInfo.ItemIndex;
		}
		if (BootyItemInfo != null)
		{
			result = BootyItemInfo.ItemIndex;
		}
		return result;
	}

	public void SetupNew()
	{
		bool enable = _dataProvider != null && _dataProvider.IsNew(this);
		UIHelper.EnableGameObject(WidgetNew, enable);
	}

	public void SetPriceLabelColor()
	{
		if (_dataProvider != null)
		{
			Color color = (_dataProvider.IsGreaterThanPrice(this) ? Color.white : Color.red);
			UIHelper.SetWidgetColor(PriceInfo.Label_Price, color);
		}
	}

	private void EnableButton(bool enable)
	{
		Button.enabled = enable;
	}

	public void OnClickShopItem()
	{
		OnClickComponent();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		OnClickComponent();
	}

	private void OnClickComponent()
	{
		if ((BootyEquipItemInfo != null || BootyItemInfo != null) && _listener != null)
		{
			_listener.OnClickComponent(this);
		}
	}

	public void SetSelected(bool Select)
	{
		if (_selected != Select)
		{
			UIHelper.EnableGameObject(Sprite_Selected, Select);
			_selected = Select;
		}
	}

	private void Reset()
	{
		BootyItemInfo = null;
		BootyEquipItemInfo = null;
		FactoryBase.SafeReleaseComponent(ref _ItemIconComponent);
		_ItemIconComponent = null;
		_selected = false;
		if (PriceInfo.IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref PriceInfo.IconProxy);
		}
		PriceInfo.IconProxy = null;
	}
}
