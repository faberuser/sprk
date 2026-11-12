using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NPayShopItemEvent;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper.NPayShop;

public static class PayShopHelper
{
	public static int GetPrice(CreatureData creatureData)
	{
		int result = -1;
		if (creatureData == null)
		{
			return result;
		}
		CreatureStarPriceData data = GameRoot.Instance.CreatureStarPriceDataContainer.GetData(creatureData.OpenStatus, creatureData.StartHeroStar);
		if (data == null)
		{
			return result;
		}
		int discountPercentage = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(creatureData);
		return data.BuyGem * (100 - discountPercentage) / 100;
	}

	public static int GetMilieage(CreatureData creatureData)
	{
		int result = 0;
		if (creatureData == null)
		{
			return result;
		}
		CreatureStarPriceData data = GameRoot.Instance.CreatureStarPriceDataContainer.GetData(creatureData.OpenStatus, creatureData.StartHeroStar);
		if (data == null)
		{
			return result;
		}
		int discountPercentage = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(creatureData);
		return data.Mileage * (100 - discountPercentage) / 100;
	}

	public static bool IsBuyable(NewPayShopProductInfo productInfo)
	{
		if (productInfo != null && SprkIsCashProduct(productInfo))
		{
			return false;
		}
		bool result = true;
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
		if (productInfo.DurationDay > 0)
		{
			double remainDayUntilMonthlyEnd = 0.0;
			result = NGame2.NAccount.UserPlayerProductPurchaseManager.IsPurchasable(productInfo, playerProductPurchaseInfo, out remainDayUntilMonthlyEnd);
		}
		else if (productInfo.PurchasableCount > 0)
		{
			result = playerProductPurchaseInfo == null || playerProductPurchaseInfo.PurchasedCount < productInfo.PurchasableCount;
		}
		else if (productInfo.ProductType == ProductType.Item)
		{
			result = PayShopManagement.CheckItemProductTypeException(productInfo);
		}
		else if (productInfo.ProductType == ProductType.Activation)
		{
			result = SetPriceForActivation(productInfo);
		}
		return result;
	}

	public static bool IsNewbieTabProductPurchaseable(NewPayShopProductInfo productInfo)
	{
		if (productInfo.IsPurchaseMarketing)
		{
			return !PurchaseMarketingManager.IsPurchaseMarketingByProduct(productInfo.Index);
		}
		string[] newbieProductOpenCondition = GetNewbieProductOpenCondition(productInfo.Index);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(newbieProductOpenCondition))
		{
			return false;
		}
		return !SingletonAuto<ConditionChecker>.instance.Check(newbieProductOpenCondition);
	}

	public static bool IsSoldOut(NewPayShopProductInfo info)
	{
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(info.Index);
		if (info.PurchasableCount > 0 && info.PurchasableCount - (playerProductPurchaseInfo?.PurchasedCount ?? 0) <= 0)
		{
			return true;
		}
		return false;
	}

	public static string[] GetNewbieProductOpenCondition(int payShopProductIndex)
	{
		NewbieRewardProductData dataByPayShopProductIndex = GameRoot.Instance.NewbieRewardProductDataContainer.GetDataByPayShopProductIndex(payShopProductIndex);
		if (dataByPayShopProductIndex != null)
		{
			NewbieRewardTapOpenData dataByRewardType = GameRoot.Instance.NewbieTapOpenDataContainer.GetDataByRewardType(dataByPayShopProductIndex.RewardType);
			if (dataByRewardType != null)
			{
				return dataByRewardType.Condition;
			}
		}
		return GameRoot.Instance.ClearMissionProductDataContainer.GetDataByPayShopProductIndex(payShopProductIndex)?.MissionOpenCondition;
	}

	public static bool SetPriceForActivation(NewPayShopProductInfo productInfo)
	{
		bool flag = true;
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.ItemInfos); i++)
			{
				PayShopProductItemInfo payShopProductItemInfo = productInfo.ItemInfos[i];
				if (payShopProductItemInfo == null)
				{
					continue;
				}
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(payShopProductItemInfo.ItemIndex);
				if (data != null)
				{
					flag = ActivationPurchasable(data);
					if (!flag)
					{
						break;
					}
				}
			}
		}
		else
		{
			string[] useItemCode = productInfo.UseItemCode;
			foreach (string code in useItemCode)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
				if (dataByCode != null)
				{
					flag = ActivationPurchasable(dataByCode);
					if (!flag)
					{
						break;
					}
				}
			}
		}
		return flag;
	}

	public static bool ActivationPurchasable(ItemData itemData)
	{
		if (itemData.Type == ItemType.Potion && itemData.GetSubData<PotionItemData>() != null && itemData.GetSubData<PotionItemData>().ActionType == ItemActionType.ExtendEquip)
		{
			int maxEquipItemCount = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount();
			int inventoryExtendSize = Helper.GetInventoryExtendSize(GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount());
			if (maxEquipItemCount >= inventoryExtendSize)
			{
				return false;
			}
		}
		if (itemData.Type == ItemType.Potion && itemData.GetSubData<PotionItemData>() != null && itemData.GetSubData<PotionItemData>().ActionType == ItemActionType.ExtendChest)
		{
			int maxChestItemCount = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount();
			int chestExtendSize = Helper.GetChestExtendSize(GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount(InventoryType.Chest));
			if (maxChestItemCount >= chestExtendSize)
			{
				return false;
			}
		}
		return true;
	}

	public static CostumeData[] GetBuyableCostumeByHeroClass(CreatureTagType heroClass, EHeroCostumeType heroCostumeType = EHeroCostumeType.None)
	{
		if (heroCostumeType == EHeroCostumeType.Legend)
		{
			EventBonusType eventBonusType = EventBonusType.PayShopDiscount;
			if (!GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(eventBonusType))
			{
				return null;
			}
		}
		return GameRoot.Instance.CostumeDataContainer.GetAllData().Where(delegate(CostumeData data)
		{
			if (string.IsNullOrEmpty(data.Group))
			{
				return false;
			}
			if (!data.IsBuy || data.IsBonusCostume() || data.IsFreeCostume())
			{
				return false;
			}
			if (heroCostumeType != EHeroCostumeType.None && data.CostumeType != heroCostumeType)
			{
				return false;
			}
			if (data.CostumeType == EHeroCostumeType.Legend)
			{
				if (data.ProductIndex == 0)
				{
					return false;
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(data.ProductIndex) == null)
				{
					return false;
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(data.ProductIndex) == null)
				{
					return false;
				}
				if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(data.ProductIndex) == null)
				{
					return false;
				}
			}
			CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(data.HeroIndex);
			return (heroClass == CreatureTagType.None || (data2 != null && data2.TagType == heroClass)) ? true : false;
		}).ToArray();
	}

	public static CostumeData[] GetBuyableCostumeByHero(int creatureIndex, EHeroCostumeType heroCostumeType = EHeroCostumeType.None)
	{
		if (heroCostumeType == EHeroCostumeType.Legend)
		{
			EventBonusType eventBonusType = EventBonusType.PayShopDiscount;
			if (!GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(eventBonusType))
			{
				return null;
			}
		}
		return GameRoot.Instance.CostumeDataContainer.GetAllData().Where(delegate(CostumeData data)
		{
			if (string.IsNullOrEmpty(data.Group))
			{
				return false;
			}
			if (!data.IsBuy || data.IsBonusCostume() || data.IsFreeCostume())
			{
				return false;
			}
			if (heroCostumeType != EHeroCostumeType.None && data.CostumeType != heroCostumeType)
			{
				return false;
			}
			if (data.CostumeType == EHeroCostumeType.Legend)
			{
				if (data.ProductIndex == 0)
				{
					return false;
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(data.ProductIndex) == null)
				{
					return false;
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(data.ProductIndex) == null)
				{
					return false;
				}
				if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(data.ProductIndex) == null)
				{
					return false;
				}
			}
			return (creatureIndex == data.HeroIndex) ? true : false;
		}).ToArray();
	}

	public static bool ExistBuyableLegendCostume()
	{
		EventBonusType eventBonusType = EventBonusType.PayShopDiscount;
		if (!GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(eventBonusType))
		{
			return false;
		}
		foreach (CostumeData item in from data in GameRoot.Instance.CostumeDataContainer.GetAllData()
			where data.CostumeType == EHeroCostumeType.Legend
			select data)
		{
			if (item.IsBuy && !item.IsBonusCostume() && !item.IsFreeCostume() && item.ProductIndex != 0 && GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(item.ProductIndex) != null && GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(item.ProductIndex) != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(item.ProductIndex) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool NewCostumeExists(int heroIndex, EHeroCostumeType heroCostumeType = EHeroCostumeType.None)
	{
		foreach (CostumeData allDatum in GameRoot.Instance.CostumeDataContainer.GetAllData())
		{
			if ((heroCostumeType == EHeroCostumeType.None || allDatum.CostumeType == heroCostumeType) && allDatum.HeroIndex == heroIndex && allDatum.New != 0)
			{
				if (allDatum.CostumeType != EHeroCostumeType.Legend)
				{
					return true;
				}
				if (allDatum.ProductIndex != 0 && GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(allDatum.ProductIndex) != null && GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(allDatum.ProductIndex) != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(allDatum.ProductIndex) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static CostumeGroupData GetLastCostumeData(int creaturIndex, EHeroCostumeType heroCostumeType = EHeroCostumeType.None)
	{
		CostumeData[] buyableCostumeByHero = GetBuyableCostumeByHero(creaturIndex, heroCostumeType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(buyableCostumeByHero))
		{
			return null;
		}
		CostumeGroupData costumeGroupData = null;
		CostumeData[] array = buyableCostumeByHero;
		foreach (CostumeData costumeData in array)
		{
			CostumeGroupData dataByGroupName = GameRoot.Instance.CostumeGroupDataContainer.GetDataByGroupName(costumeData.Group);
			if (dataByGroupName != null && (costumeGroupData == null || costumeGroupData.CostumeGroupIndex < dataByGroupName.CostumeGroupIndex))
			{
				costumeGroupData = dataByGroupName;
			}
		}
		return costumeGroupData;
	}

	public static bool SprkIsCashProduct(NewPayShopProductInfo product)
	{
		if (string.IsNullOrEmpty(product.GoogleId) && string.IsNullOrEmpty(product.AppleId) && string.IsNullOrEmpty(product.OneStoreId) && string.IsNullOrEmpty(product.UDPStoreId))
		{
			return false;
		}
		return true;
	}
}
