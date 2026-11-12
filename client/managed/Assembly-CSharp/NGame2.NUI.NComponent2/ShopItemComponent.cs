using System;
using System.Collections.Generic;
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

public class ShopItemComponent : UIScrollItemBehaviour, ItemIconComponent.Listener, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class CostSpriteInfo
	{
		public ShopCostType ShopCostType;

		public UISprite Sprite_Cost;
	}

	[Serializable]
	public class ItemPriceInfo
	{
		public GameObject PriceGroup;

		public UILabel Label_Price;

		public UIWidget IconRoot;

		public IconProxy IconProxy;
	}

	public interface Listener
	{
		void OnClick(ShopItemComponent shopItem);
	}

	public interface IDataProvider
	{
		bool IsGreaterThanPrice(ShopItemComponent shopItem);

		int GetCurrentMyDonatePoint();

		int GetCurrentMaxDonatePoint();
	}

	public const string PrefabPath = "Resources/Prefab2/Component/ShopItem/ShopItemComponent";

	public const string PrefabPath2 = "Resources/Prefab2/Component/ShopItem/ChallengeRaidShopItemComponent";

	public static PrefabAuto<ShopItemComponent> PrefabAuto = new PrefabAuto<ShopItemComponent>("Resources/Prefab2/Component/ShopItem/ShopItemComponent");

	public static PrefabAuto<ShopItemComponent> PrefabAuto2 = new PrefabAuto<ShopItemComponent>("Resources/Prefab2/Component/ShopItem/ChallengeRaidShopItemComponent");

	public UISprite Sprite_Selected;

	public UIButton Button;

	public UILabel Label_PurchaseCount;

	public ItemPriceInfo PriceInfo;

	public UISimpleGrid ItemGrid;

	public UILabel Label_ItemName;

	public UISprite Sprite_SoldOut;

	public UILabel Label_SoldOut;

	public GaugeComponent GaugeComponent;

	private ItemIconComponent _itemIconComponent;

	private bool _selected;

	private List<string> _visibleGaugeList = new List<string>();

	private int _currentPoint;

	private Listener _listener;

	private IDataProvider _dataProvider;

	public ShopListItemInfo ShopListItemInfo { get; private set; }

	public ShopItemData ShopItemData { get; private set; }

	public int Purchased { get; set; }

	public ShopData ShopData { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (PriceInfo != null)
		{
			if (PriceInfo.IconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref PriceInfo.IconProxy);
			}
			PriceInfo.IconProxy = null;
		}
	}

	public bool Open(ShopListItemInfo info, Listener listener, IDataProvider dataProvider)
	{
		if (info == null)
		{
			return false;
		}
		ShopListItemInfo = info;
		OpenInternal(info.ShopIndex, listener, dataProvider);
		return true;
	}

	public bool Open(ShopItemData itemData, ShopItemInfo itemInfo, Listener listener, IDataProvider dataProvider)
	{
		if (itemData == null)
		{
			return false;
		}
		ShopItemData = itemData;
		Purchased = itemInfo?.Purchased ?? 0;
		OpenInternal(ShopItemData.ShopIndex, listener, dataProvider);
		return true;
	}

	private void OpenInternal(int ShopIndex, Listener listener, IDataProvider dataProvider)
	{
		ShopData = GameRoot.Instance.ShopDataContainer.GetData(ShopIndex);
		if (ShopData != null)
		{
			_listener = listener;
			_dataProvider = dataProvider;
			SetVisibleGaugeList();
			Setup();
		}
	}

	public void SetSelected(bool Select)
	{
		if (_selected != Select)
		{
			if (!IsSoldOut() || ShopItemData != null)
			{
				UIHelper.EnableGameObject(Sprite_Selected, Select);
			}
			_selected = Select;
		}
	}

	public void Close()
	{
		Reset();
	}

	private void Reset()
	{
		ShopListItemInfo = null;
		ShopItemData = null;
		FactoryBase.ReleaseComponent(_itemIconComponent);
		_itemIconComponent = null;
		_selected = false;
		EnableButton(enable: false);
	}

	public void SetSold()
	{
		if (IsSoldOut())
		{
			SetSelected(Select: false);
		}
		Setup();
	}

	public bool IsSoldOut()
	{
		if (ShopListItemInfo != null)
		{
			return ShopListItemInfo.Purchased >= ShopListItemInfo.PurchasableCount;
		}
		if (ShopItemData != null)
		{
			if (ShopItemData.PurchasableCount != 0)
			{
				return Purchased >= ShopItemData.PurchasableCount;
			}
			return false;
		}
		return false;
	}

	public void OnClickShopItem()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	private void Setup()
	{
		if (ShopListItemInfo != null || ShopItemData != null)
		{
			EnableButton(!IsSoldOut());
			SetupBackground();
			SetupItemIcon();
			SetupPrice();
			SetVisibleGauge();
			SetGauge();
		}
	}

	private void SetupBackground()
	{
		if (ShopListItemInfo != null)
		{
			if (!IsSoldOut())
			{
				UIHelper.EnableGameObject(Sprite_Selected, _selected);
			}
			else
			{
				UIHelper.EnableGameObject(Sprite_Selected, enable: false);
			}
			UIHelper.EnableGameObject(Label_PurchaseCount, !IsSoldOut() && ShopListItemInfo != null && ShopListItemInfo.PurchasableCount > 1);
			if (!IsSoldOut() && ShopListItemInfo != null && ShopListItemInfo.PurchasableCount > 1)
			{
				int num = ShopListItemInfo.PurchasableCount - ShopListItemInfo.Purchased;
				UIHelper.SetLabelText(Label_PurchaseCount, $"{num}/{ShopListItemInfo.PurchasableCount}");
			}
		}
		if (ShopItemData != null)
		{
			if (!IsSoldOut())
			{
				UIHelper.EnableGameObject(Sprite_Selected, _selected);
			}
			else
			{
				UIHelper.EnableGameObject(Sprite_Selected, enable: false);
			}
			UIHelper.EnableGameObject(Label_PurchaseCount, ShopItemData != null && ShopItemData.PurchasableCount > 0);
			if (ShopItemData != null && ShopItemData.PurchasableCount > 0)
			{
				int remainPurchasableCount = GetRemainPurchasableCount();
				UIHelper.SetLabelText(Label_PurchaseCount, $"{remainPurchasableCount}/{ShopItemData.PurchasableCount}");
			}
		}
	}

	public int GetRemainPurchasableCount()
	{
		if (ShopItemData == null)
		{
			return 0;
		}
		return ShopItemData.PurchasableCount - Purchased;
	}

	private void SetupItemIcon()
	{
		if (_itemIconComponent == null)
		{
			string ItemCode = string.Empty;
			int num = 0;
			long ItemCount = num;
			int ItemStar = 0;
			GetShopItemDetailInfo(out ItemCode, out ItemCount, out ItemStar);
			_itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(ItemCode);
			_itemIconComponent.Open(dataByCode, Shop.CreateFakeEquipInfo(dataByCode, ItemStar), this);
			_itemIconComponent.SetItemCount(ItemCount);
		}
		_itemIconComponent.SetIconGrayScale(IsSoldOut());
	}

	private void SetupPrice()
	{
		string code = string.Empty;
		if (ShopListItemInfo != null)
		{
			code = ShopListItemInfo.ItemCode;
		}
		if (ShopItemData != null)
		{
			code = ShopItemData.ItemCode;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
		if (dataByCode == null)
		{
			return;
		}
		UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(dataByCode));
		bool flag = IsSoldOut();
		UIHelper.EnableGameObject(Sprite_SoldOut, flag);
		UIHelper.EnableGameObject(Label_SoldOut, flag);
		UIHelper.EnableGameObject(PriceInfo.PriceGroup, !flag);
		if (ShopListItemInfo == null || !flag)
		{
			ShopCostType currentCostType = GetCurrentCostType();
			if (PriceInfo.IconProxy == null)
			{
				PriceInfo.IconProxy = IconProxy.Create(PriceInfo.IconRoot);
			}
			if (PriceInfo.IconProxy != null)
			{
				PriceInfo.IconProxy.SetupCurrencyNoBg(currentCostType.ToString());
			}
			long price = GetPrice();
			UIHelper.SetLabelText(PriceInfo.Label_Price, Vespa_Int_Extension_Method.ToCommaString(price));
			SetPriceLabelColor();
		}
	}

	public bool CheckIsGauge()
	{
		if (ShopItemData == null)
		{
			return false;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(ShopItemData.ItemCode);
		if (dataByCode == null)
		{
			return false;
		}
		return _visibleGaugeList.Contains(dataByCode.Code);
	}

	private void SetVisibleGaugeList()
	{
		_visibleGaugeList.Clear();
		_visibleGaugeList.Add("HOUSING_EXTEND_PAPER");
	}

	private void SetVisibleGauge()
	{
		if (!(GaugeComponent == null) && PriceInfo != null && !(PriceInfo.PriceGroup == null))
		{
			if (IsSoldOut())
			{
				UIHelper.EnableGameObject(GaugeComponent, enable: false);
				UIHelper.EnableGameObject(PriceInfo.PriceGroup, enable: false);
			}
			else
			{
				bool flag = CheckIsGauge();
				UIHelper.EnableGameObject(GaugeComponent, flag);
				UIHelper.EnableGameObject(PriceInfo.PriceGroup, !flag);
			}
		}
	}

	public void SetGauge()
	{
		if (!(GaugeComponent == null) && CheckIsGauge())
		{
			GaugeComponent.Init(_dataProvider.GetCurrentMyDonatePoint(), _dataProvider.GetCurrentMaxDonatePoint());
		}
	}

	public void SetAddValueGauge(int addValue)
	{
		GaugeComponent.SetAddValueGauge(addValue);
	}

	public ShopCostType GetCurrentCostType()
	{
		if (ShopItemData != null && ShopItemData.BuyCostTypeForItem != ShopCostType.None)
		{
			return ShopItemData.BuyCostTypeForItem;
		}
		return ShopData.BuyCostType;
	}

	public string GetSpriteNameByCostType(ShopCostType costType)
	{
		if (__Extension_Method_ICollection__.COUNT(ShopData.BuyCostTypeUI) > 0)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ShopData.BuyCostTypeUI); i++)
			{
				if (ShopData.BuyCostTypeUI[i] == costType && __Extension_Method_ICollection__.COUNT(ShopData.BuyCostTypeIconSprite) >= i)
				{
					return ShopData.BuyCostTypeIconSprite[i];
				}
			}
		}
		return string.Empty;
	}

	public long GetPrice()
	{
		string ItemCode = string.Empty;
		int num = 0;
		long ItemCount = num;
		int ItemStar = 0;
		if (!GetShopItemDetailInfo(out ItemCode, out ItemCount, out ItemStar))
		{
			int num2 = 0;
			return num2;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(ItemCode);
		if (dataByCode == null)
		{
			int num3 = 0;
			return num3;
		}
		int num4 = 0;
		long num5 = num4;
		if (ShopItemData != null && ShopItemData.TargetPrice > 0)
		{
			int targetPrice = ShopItemData.TargetPrice;
			num5 = targetPrice;
		}
		else
		{
			switch (GetCurrentCostType())
			{
			case ShopCostType.Gold:
			{
				int buyGold2 = dataByCode.BuyGold;
				num5 = buyGold2;
				break;
			}
			case ShopCostType.Gem:
			{
				int buyGem = dataByCode.BuyGem;
				num5 = buyGem;
				break;
			}
			case ShopCostType.PvpCoin:
			{
				int buyPvpCoin = dataByCode.BuyPvpCoin;
				num5 = buyPvpCoin;
				break;
			}
			case ShopCostType.GuildPoint:
			{
				int buyGuildPoint = dataByCode.BuyGuildPoint;
				num5 = buyGuildPoint;
				break;
			}
			case ShopCostType.RoyalPoint:
			{
				int buyRoyalPoint = dataByCode.BuyRoyalPoint;
				num5 = buyRoyalPoint;
				break;
			}
			case ShopCostType.FriendshipPoint:
			{
				int buyFriendshipPoint = dataByCode.BuyFriendshipPoint;
				num5 = buyFriendshipPoint;
				break;
			}
			case ShopCostType.RaidPoint:
			{
				int buyRaidPoint = dataByCode.BuyRaidPoint;
				num5 = buyRaidPoint;
				break;
			}
			case ShopCostType.WorldBossPoint:
			{
				int buyWorldBossPoint = dataByCode.BuyWorldBossPoint;
				num5 = buyWorldBossPoint;
				break;
			}
			case ShopCostType.EventDungeonPoint:
			{
				int buyEventDungeonPoint3 = dataByCode.BuyEventDungeonPoint;
				num5 = buyEventDungeonPoint3;
				break;
			}
			case ShopCostType.EventDungeonPoint2:
			{
				int buyEventDungeonPoint2 = dataByCode.BuyEventDungeonPoint2;
				num5 = buyEventDungeonPoint2;
				break;
			}
			case ShopCostType.EventDungeonPoint3:
			{
				int buyEventDungeonPoint = dataByCode.BuyEventDungeonPoint3;
				num5 = buyEventDungeonPoint;
				break;
			}
			case ShopCostType.GloryPoint:
			{
				int buyGloryPoint = dataByCode.BuyGloryPoint;
				num5 = buyGloryPoint;
				break;
			}
			case ShopCostType.EventGiftPoint:
			{
				int buyEventGiftPoint = dataByCode.BuyEventGiftPoint;
				num5 = buyEventGiftPoint;
				break;
			}
			case ShopCostType.RankingPoint:
			{
				int buyRankingPoint = dataByCode.BuyRankingPoint;
				num5 = buyRankingPoint;
				break;
			}
			case ShopCostType.GuildArenaPoint:
			{
				int buyGuildArenaPoint = dataByCode.BuyGuildArenaPoint;
				num5 = buyGuildArenaPoint;
				break;
			}
			case ShopCostType.ShopEventPoint:
			{
				int buyShopEventPoint = dataByCode.BuyShopEventPoint;
				num5 = buyShopEventPoint;
				break;
			}
			case ShopCostType.OrdealArenaPoint:
			{
				int buyOrdealArenaPoint = dataByCode.BuyOrdealArenaPoint;
				num5 = buyOrdealArenaPoint;
				break;
			}
			case ShopCostType.EventOrvelPoint:
			{
				int buyEventOrvelPoint = dataByCode.BuyEventOrvelPoint;
				num5 = buyEventOrvelPoint;
				break;
			}
			case ShopCostType.ChallengeRaidPoint:
			{
				int buyChallengeRaidPoint = dataByCode.BuyChallengeRaidPoint;
				num5 = buyChallengeRaidPoint;
				break;
			}
			default:
			{
				int buyGold = dataByCode.BuyGold;
				num5 = buyGold;
				break;
			}
			}
			if (dataByCode.Type == ItemType.Equip && ItemStar > 0)
			{
				EquipItemStarData data = GameRoot.Instance.EquipItemStarDataContainer.GetData(ItemStar);
				if (data != null)
				{
					num5 = (long)((float)num5 * ((float)data.BuyPriceFactor / 1000f));
				}
			}
			num5 = checked(num5 * ItemCount);
		}
		float discountRatio = GetDiscountRatio();
		return (long)Math.Ceiling(Math.Round((float)num5 * discountRatio, 2));
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
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroEventBonusData.BonusValue) && heroEventBonusData.BonusValue.Length == 2 && Vespa_String_Extension_Method.TOINT(heroEventBonusData.BonusValue[0], 0) == ShopData.Index)
			{
				int num2 = Vespa_String_Extension_Method.TOINT(heroEventBonusData.BonusValue[1], 0);
				num *= 1f - (float)num2 * 0.01f;
			}
		}
		return num;
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

	void ItemIconComponent.Listener.OnClick(ItemIconComponent comp)
	{
		if (!IsSoldOut() && _listener != null)
		{
			_listener.OnClick(this);
		}
	}

	private bool GetShopItemDetailInfo(out string ItemCode, out long ItemCount, out int ItemStar)
	{
		ItemCode = string.Empty;
		int num = 0;
		ItemCount = num;
		ItemStar = 0;
		if (ShopListItemInfo == null && ShopItemData == null)
		{
			return false;
		}
		if (ShopListItemInfo != null)
		{
			ItemCode = ShopListItemInfo.ItemCode;
			int count = ShopListItemInfo.Count;
			ItemCount = count;
			ItemStar = ShopListItemInfo.Star;
		}
		if (ShopItemData != null)
		{
			ItemCode = ShopItemData.ItemCode;
			int count2 = ShopItemData.Count;
			ItemCount = count2;
			ItemStar = ShopItemData.Star;
		}
		return true;
	}

	public void SetNotForSale()
	{
		SetSelected(Select: false);
		EnableButton(enable: false);
		UIHelper.EnableGameObject(Sprite_SoldOut, enable: true);
	}

	public void RefreshPurchasableCount(int purchasedCount)
	{
		Purchased = purchasedCount;
		Setup();
	}

	public void UpdateShopItemInfo(ShopItemInfo info)
	{
		Purchased = info?.Purchased ?? 0;
		if (IsSoldOut())
		{
			SetSelected(Select: false);
		}
		Setup();
	}
}
