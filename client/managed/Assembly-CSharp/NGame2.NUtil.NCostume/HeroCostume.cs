using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCostume;

public class HeroCostume : BaseCostume
{
	public static CostumeState GetCostumeState(HeroInfo heroInfo, CostumeData costumeData)
	{
		return GetCostumeState((heroInfo == null) ? null : GameRoot.Instance.CostumeDataContainer.GetData(heroInfo.CostumeIndex), costumeData);
	}

	public static CostumeState GetCostumeState(CostumeData equippedCostumeData, CostumeData costumeData, bool checkHeroOwn = true)
	{
		if (costumeData == null)
		{
			return CostumeState.None;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(costumeData.HeroIndex);
		if (equippedCostumeData != null && cachedHeroInfo != null && equippedCostumeData.CostumeIndex == costumeData.CostumeIndex)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star), costumeData))
			{
				return CostumeState.UnEquipSelect;
			}
			return CostumeState.Equipped;
		}
		if (!costumeData.IsDefault)
		{
			if (checkHeroOwn && cachedHeroInfo == null)
			{
				return CostumeState.NeedHero;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) == null)
			{
				if (costumeData.CostumeIndex != 180 && costumeData.CostumeIndex != 280 && costumeData.CostumeIndex != 380 && costumeData.CostumeIndex != 480 && costumeData.CostumeIndex != 580 && costumeData.CostumeIndex != 680 && costumeData.CostumeIndex != 780 && costumeData.CostumeIndex != 880 && costumeData.CostumeIndex != 980 && costumeData.CostumeIndex != 1080 && costumeData.CostumeIndex != 1180 && costumeData.CostumeIndex != 1280 && costumeData.CostumeIndex != 1380 && costumeData.CostumeIndex != 1580 && costumeData.CostumeIndex != 1680 && costumeData.CostumeIndex != 1780 && costumeData.CostumeIndex != 1880 && costumeData.CostumeIndex != 1980 && costumeData.CostumeIndex != 2080 && costumeData.CostumeIndex != 2180 && costumeData.CostumeIndex != 2280 && costumeData.CostumeIndex != 2380 && costumeData.CostumeIndex != 2480 && costumeData.CostumeIndex != 2580 && costumeData.CostumeIndex != 2680 && costumeData.CostumeIndex != 2780 && costumeData.CostumeIndex != 2880 && costumeData.CostumeIndex != 2980 && costumeData.CostumeIndex != 3018 && costumeData.CostumeIndex != 3180 && costumeData.CostumeIndex != 3280 && costumeData.CostumeIndex != 3380 && costumeData.CostumeIndex != 3417 && costumeData.CostumeIndex != 3580 && costumeData.CostumeIndex != 3680 && costumeData.CostumeIndex != 3780 && costumeData.CostumeIndex != 3880 && costumeData.CostumeIndex != 3980 && costumeData.CostumeIndex != 4080 && costumeData.CostumeIndex != 4180 && costumeData.CostumeIndex != 4280 && costumeData.CostumeIndex != 4380 && costumeData.CostumeIndex != 4480 && costumeData.CostumeIndex != 4514 && costumeData.CostumeIndex != 4680 && costumeData.CostumeIndex != 4780 && costumeData.CostumeIndex != 4880 && costumeData.CostumeIndex != 4980 && costumeData.CostumeIndex != 5080 && costumeData.CostumeIndex != 5112 && costumeData.CostumeIndex != 5280 && costumeData.CostumeIndex != 5380 && costumeData.CostumeIndex != 5414 && costumeData.CostumeIndex != 5580 && costumeData.CostumeIndex != 5680 && costumeData.CostumeIndex != 5780 && costumeData.CostumeIndex != 5880 && costumeData.CostumeIndex != 5980 && costumeData.CostumeIndex != 6080 && costumeData.CostumeIndex != 6180 && costumeData.CostumeIndex != 6280 && costumeData.CostumeIndex != 6380 && costumeData.CostumeIndex != 6480 && costumeData.CostumeIndex != 6580 && costumeData.CostumeIndex != 6680 && costumeData.CostumeIndex != 6780 && costumeData.CostumeIndex != 6880 && costumeData.CostumeIndex != 6980 && costumeData.CostumeIndex != 7080 && costumeData.CostumeIndex != 7180 && costumeData.CostumeIndex != 7280 && costumeData.CostumeIndex != 7380 && costumeData.CostumeIndex != 7480 && costumeData.CostumeIndex != 7580 && costumeData.CostumeIndex != 7680 && costumeData.CostumeIndex != 7780 && costumeData.CostumeIndex != 7880 && costumeData.CostumeIndex != 7980 && costumeData.CostumeIndex != 8080 && costumeData.CostumeIndex != 8180 && costumeData.CostumeIndex != 8280 && costumeData.CostumeIndex != 8390 && costumeData.CostumeIndex != 8490 && costumeData.CostumeIndex != 8590 && costumeData.CostumeIndex != 8680 && costumeData.CostumeIndex != 8780 && costumeData.CostumeIndex != 8880 && costumeData.CostumeIndex != 8980 && costumeData.CostumeIndex != 9080 && costumeData.CostumeIndex != 9180 && costumeData.CostumeIndex != 9280 && costumeData.CostumeIndex != 9380 && costumeData.CostumeIndex != 9480 && costumeData.CostumeIndex != 9580 && costumeData.CostumeIndex != 9610 && costumeData.CostumeIndex != 9780 && costumeData.CostumeIndex != 9880 && costumeData.CostumeIndex != 9980 && costumeData.CostumeIndex != 10080 && costumeData.CostumeIndex != 10180 && costumeData.CostumeIndex != 10280 && costumeData.CostumeIndex != 11106 && costumeData.CostumeIndex != 190 && costumeData.CostumeIndex != 290 && costumeData.CostumeIndex != 390 && costumeData.CostumeIndex != 490 && costumeData.CostumeIndex != 590 && costumeData.CostumeIndex != 690 && costumeData.CostumeIndex != 790 && costumeData.CostumeIndex != 890 && costumeData.CostumeIndex != 990 && costumeData.CostumeIndex != 1090 && costumeData.CostumeIndex != 1190 && costumeData.CostumeIndex != 1290 && costumeData.CostumeIndex != 1390 && costumeData.CostumeIndex != 1590 && costumeData.CostumeIndex != 1690 && costumeData.CostumeIndex != 1790 && costumeData.CostumeIndex != 1890 && costumeData.CostumeIndex != 1990 && costumeData.CostumeIndex != 2090 && costumeData.CostumeIndex != 2190 && costumeData.CostumeIndex != 2290 && costumeData.CostumeIndex != 2390 && costumeData.CostumeIndex != 2490 && costumeData.CostumeIndex != 2590 && costumeData.CostumeIndex != 2690 && costumeData.CostumeIndex != 2790 && costumeData.CostumeIndex != 2890 && costumeData.CostumeIndex != 2990 && costumeData.CostumeIndex != 3090 && costumeData.CostumeIndex != 3190 && costumeData.CostumeIndex != 3290 && costumeData.CostumeIndex != 3390 && costumeData.CostumeIndex != 3490 && costumeData.CostumeIndex != 3590 && costumeData.CostumeIndex != 3690 && costumeData.CostumeIndex != 3790 && costumeData.CostumeIndex != 3890 && costumeData.CostumeIndex != 3990 && costumeData.CostumeIndex != 4090 && costumeData.CostumeIndex != 4190 && costumeData.CostumeIndex != 4290 && costumeData.CostumeIndex != 4390 && costumeData.CostumeIndex != 4490 && costumeData.CostumeIndex != 4590 && costumeData.CostumeIndex != 4690 && costumeData.CostumeIndex != 4790 && costumeData.CostumeIndex != 4890 && costumeData.CostumeIndex != 4990 && costumeData.CostumeIndex != 5090 && costumeData.CostumeIndex != 5190 && costumeData.CostumeIndex != 5290 && costumeData.CostumeIndex != 5390 && costumeData.CostumeIndex != 5490 && costumeData.CostumeIndex != 5590 && costumeData.CostumeIndex != 5690 && costumeData.CostumeIndex != 5790 && costumeData.CostumeIndex != 5890 && costumeData.CostumeIndex != 5990 && costumeData.CostumeIndex != 6090 && costumeData.CostumeIndex != 6190 && costumeData.CostumeIndex != 6290 && costumeData.CostumeIndex != 6390 && costumeData.CostumeIndex != 6490 && costumeData.CostumeIndex != 6590 && costumeData.CostumeIndex != 6690 && costumeData.CostumeIndex != 6790 && costumeData.CostumeIndex != 6890 && costumeData.CostumeIndex != 6990 && costumeData.CostumeIndex != 7090 && costumeData.CostumeIndex != 7190 && costumeData.CostumeIndex != 7290 && costumeData.CostumeIndex != 7390 && costumeData.CostumeIndex != 7490 && costumeData.CostumeIndex != 7590 && costumeData.CostumeIndex != 7690 && costumeData.CostumeIndex != 7790 && costumeData.CostumeIndex != 7890 && costumeData.CostumeIndex != 7990 && costumeData.CostumeIndex != 8090 && costumeData.CostumeIndex != 8190 && costumeData.CostumeIndex != 8290 && costumeData.CostumeIndex != 8690 && costumeData.CostumeIndex != 8790 && costumeData.CostumeIndex != 8890 && costumeData.CostumeIndex != 8990 && costumeData.CostumeIndex != 9090 && costumeData.CostumeIndex != 9190 && costumeData.CostumeIndex != 9290 && costumeData.CostumeIndex != 9390 && costumeData.CostumeIndex != 9490 && costumeData.CostumeIndex != 9590 && costumeData.CostumeIndex != 9690 && costumeData.CostumeIndex != 9790 && costumeData.CostumeIndex != 9890 && costumeData.CostumeIndex != 9990 && costumeData.CostumeIndex != 10090 && costumeData.CostumeIndex != 10190 && costumeData.CostumeIndex != 10290 && costumeData.CostumeIndex != 11190)
				{
					return CostumeState.Buyable;
				}
				return CostumeState.CannotBuy;
			}
			return CostumeState.Owned;
		}
		if (costumeData.CostumeType != EHeroCostumeType.Legend)
		{
			if (checkHeroOwn && cachedHeroInfo == null)
			{
				return CostumeState.NeedHero;
			}
			if (cachedHeroInfo == null)
			{
				return CostumeState.NeedAwaken;
			}
			if (costumeData.EquipCondition == EEquipCondition.NeedAwaken)
			{
				if (costumeData.HeroStar > 0 && cachedHeroInfo.Star < costumeData.HeroStar)
				{
					return CostumeState.NeedAwaken;
				}
			}
			else if (costumeData.EquipCondition == EEquipCondition.NeedSubStory && !CanBuySubStoryCostume(costumeData))
			{
				return CostumeState.NeedSubStory;
			}
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) != null)
		{
			return CostumeState.Owned;
		}
		if (cachedHeroInfo != null && equippedCostumeData == null)
		{
			if (!costumeData.IsCoupleCostume())
			{
				if (Vespa_Common_Class_Extension_Method.EQUALS(GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star), costumeData))
				{
					return CostumeState.UnEquipSelect;
				}
			}
			else
			{
				List<CostumeData> costumeDataListByHeroStar = GameRoot.Instance.CostumeDataContainer.GetCostumeDataListByHeroStar(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, isFreeCostume: true);
				if (__Extension_Method_ICollection__.COUNT(costumeDataListByHeroStar) > 0 && !costumeData.IsBonusCostume())
				{
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeDataListByHeroStar); i++)
					{
						if (Vespa_Common_Class_Extension_Method.EQUALS(costumeDataListByHeroStar[i], costumeData))
						{
							return CostumeState.UnEquipSelect;
						}
					}
				}
			}
		}
		if (costumeData.IsFreeCostume())
		{
			return CostumeState.Owned;
		}
		if (costumeData.CostumeType == EHeroCostumeType.Legend)
		{
			if (costumeData.IsBonusCostume())
			{
				CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeData.MainIndex);
				if (!data.IsBonusCostume())
				{
					return GetCostumeState(equippedCostumeData, data, checkHeroOwn);
				}
			}
			EventBonusType eventBonusType = EventBonusType.PayShopDiscount;
			if (!GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(eventBonusType))
			{
				return CostumeState.LegendNotBuyable;
			}
			if (costumeData.ProductIndex == 0)
			{
				return CostumeState.LegendNotBuyable;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(costumeData.ProductIndex) == null)
			{
				return CostumeState.LegendNotBuyable;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(costumeData.ProductIndex) == null)
			{
				return CostumeState.LegendNotBuyable;
			}
			if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(costumeData.ProductIndex) == null)
			{
				return CostumeState.LegendNotBuyable;
			}
			return CostumeState.LegendBuyable;
		}
		if (!costumeData.IsBuy)
		{
			return CostumeState.CannotBuy;
		}
		return CostumeState.Buyable;
	}

	private static bool CanBuySubStoryCostume(CostumeData costumeData)
	{
		if (costumeData == null)
		{
			return false;
		}
		if (costumeData.CostumeIndex == 0)
		{
			return false;
		}
		IList<SubStoryData> allData = GameRoot.Instance.SubStoryDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			if (allData[i].UnlockCostumeIndex == costumeData.CostumeIndex && allData[i].CreatureIndex == costumeData.HeroIndex)
			{
				return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(allData[i].ChapterIndex, allData[i].DungeonIndex);
			}
		}
		return false;
	}

	private static bool IsShowCostume(CostumeData costumeData, HeroInfo heroInfo)
	{
		if (!costumeData.IsCoupleCostume())
		{
			return true;
		}
		CostumeInfo costumeInfo = GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex);
		int costumeIndex = heroInfo.CostumeIndex;
		bool flag = costumeIndex == costumeData.CostumeIndex;
		bool flag2 = false;
		if (costumeInfo != null)
		{
			flag2 = true;
		}
		else
		{
			List<CostumeData> costumeDataListByHeroStar = GameRoot.Instance.CostumeDataContainer.GetCostumeDataListByHeroStar(heroInfo.HeroIndex, heroInfo.Star, isFreeCostume: true);
			if (__Extension_Method_ICollection__.COUNT(costumeDataListByHeroStar) > 0)
			{
				flag2 = costumeDataListByHeroStar.Exists((CostumeData basicCostumeData) => basicCostumeData.CostumeIndex == costumeData.CostumeIndex);
			}
		}
		if (costumeData.IsBonusCostume())
		{
			if (flag2 && flag)
			{
				return true;
			}
			return false;
		}
		int[] bonusCostumeIndices = costumeData.BonusCostumeIndices;
		foreach (int num2 in bonusCostumeIndices)
		{
			if (costumeIndex == num2)
			{
				return false;
			}
		}
		return true;
	}

	public static int GetPriceGem(CostumeData costumeData, bool checkDiscount = true)
	{
		if (costumeData == null)
		{
			return -1;
		}
		int num = costumeData.ReqBuyGem;
		if (checkDiscount)
		{
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(costumeData);
			num = BaseCostume.CalculatePrice(num, priceDiscountPercentage);
		}
		return num;
	}

	public static int GetObtainMileage(CostumeData costumeData)
	{
		if (costumeData == null)
		{
			return -1;
		}
		int mileage = costumeData.Mileage;
		int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(costumeData);
		return BaseCostume.GetMileage(mileage, priceDiscountPercentage);
	}

	public static int CheckReplaceBasicCostume(int costumeIndex, int heroIndex, int heroStar)
	{
		if (costumeIndex == 0)
		{
			return -1;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data == null)
		{
			return -1;
		}
		if (data != null && data.CostumeType != EHeroCostumeType.Legend)
		{
			return -1;
		}
		return GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(heroIndex, heroStar)?.CostumeIndex ?? (-1);
	}
}
