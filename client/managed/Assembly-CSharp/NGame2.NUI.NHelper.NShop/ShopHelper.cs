using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper.NShop;

public static class ShopHelper
{
	public static int GetWindowGuideIndex(ShopData shopData)
	{
		return shopData?.MiniGuideIndex ?? 0;
	}

	public static StaminaType GetStaminaType(ShopData shopData)
	{
		if (shopData == null)
		{
			return StaminaType.Chicken;
		}
		if (__Extension_Method_ICollection__.COUNT(shopData.BuyCostTypeUI) <= 1)
		{
			return StaminaType.Chicken;
		}
		return StaminaType.Empty;
	}

	public static UserManager.UserStatType GetStatType(ShopData shopData)
	{
		if (shopData == null)
		{
			return UserManager.UserStatType.Gold;
		}
		return EnumLookup<UserManager.UserStatType>.GetByName(shopData.BuyCostType.ToString())?.EnumValue ?? UserManager.UserStatType.Gold;
	}

	public static bool IsGuildBootyItemGroup(ShopData shopData)
	{
		if (shopData == null)
		{
			return false;
		}
		if (shopData.BuyCostType == ShopCostType.GuildPoint)
		{
			return shopData.Code.Equals("GuildRaidShop");
		}
		return false;
	}

	public static bool IsNormalGuildShop(ShopData shopData)
	{
		if (shopData == null)
		{
			return false;
		}
		if (shopData.BuyCostType == ShopCostType.GuildPoint)
		{
			return shopData.Code.Equals("Guild");
		}
		return false;
	}

	public static bool Buyable(ShopItemComponent comp)
	{
		long price = comp.GetPrice();
		return GetPlayerAssetValue(comp.GetCurrentCostType()) >= price;
	}

	public static bool Buyable(GuildRaidBootyItemComponent comp, ShopData shopData)
	{
		int price = comp.GetPrice();
		return GetPlayerAssetValue(shopData.BuyCostType) >= price;
	}

	public static ShopCostType GetBuyCostType(UserManager.UserStatType userStatType)
	{
		switch (userStatType)
		{
		case UserManager.UserStatType.Gold:
			return ShopCostType.Gold;
		case UserManager.UserStatType.Gem:
		case UserManager.UserStatType.PayGem:
			return ShopCostType.Gem;
		case UserManager.UserStatType.GuildPoint:
			return ShopCostType.GuildPoint;
		case UserManager.UserStatType.PvpCoin:
			return ShopCostType.PvpCoin;
		case UserManager.UserStatType.RoyalPoint:
			return ShopCostType.RoyalPoint;
		case UserManager.UserStatType.FriendshipPoint:
			return ShopCostType.FriendshipPoint;
		case UserManager.UserStatType.RaidPoint:
			return ShopCostType.RaidPoint;
		case UserManager.UserStatType.WorldBossPoint:
			return ShopCostType.WorldBossPoint;
		case UserManager.UserStatType.EventDungeonPoint:
			return ShopCostType.EventDungeonPoint;
		case UserManager.UserStatType.EventDungeonPoint2:
			return ShopCostType.EventDungeonPoint2;
		case UserManager.UserStatType.EventGiftPoint:
			return ShopCostType.EventGiftPoint;
		case UserManager.UserStatType.GloryPoint:
			return ShopCostType.GloryPoint;
		case UserManager.UserStatType.RankingPoint:
			return ShopCostType.RankingPoint;
		case UserManager.UserStatType.EventDungeonPoint3:
			return ShopCostType.EventDungeonPoint3;
		case UserManager.UserStatType.GuildArenaPoint:
			return ShopCostType.GuildArenaPoint;
		case UserManager.UserStatType.ShopEventPoint:
			return ShopCostType.ShopEventPoint;
		case UserManager.UserStatType.OrdealArenaPoint:
			return ShopCostType.OrdealArenaPoint;
		case UserManager.UserStatType.EventOrvelPoint:
			return ShopCostType.EventOrvelPoint;
		case UserManager.UserStatType.ChallengeRaidPoint:
			return ShopCostType.ChallengeRaidPoint;
		case UserManager.UserStatType.CraftEventPoint:
			return ShopCostType.CraftEventPoint;
		default:
			return ShopCostType.Gold;
		}
	}

	public static long GetPlayerAssetValue(ShopCostType CostType)
	{
		long result = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(CostType.ToString());
		if (CostType == ShopCostType.Gem)
		{
			result = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		}
		if (CostType == ShopCostType.RankingPoint)
		{
			result = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(CurrencyType.RankingPoint.ToString());
		}
		if (CostType == ShopCostType.EventDungeonPoint3)
		{
			result = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(CurrencyType.EventDungeonPoint3.ToString());
		}
		return result;
	}

	public static string GetItemCodeFromBootyItem(long itemId, bool isBootyItemEquip, out int starCount, out long itemCount)
	{
		starCount = 0;
		int num = 0;
		itemCount = num;
		if (itemId == -1)
		{
			return string.Empty;
		}
		int index = 0;
		if (isBootyItemEquip)
		{
			GuildRaidBootyEquipItemInfo guildRaidBootyEquipItemInfo = GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.Get(itemId);
			if (guildRaidBootyEquipItemInfo != null)
			{
				index = guildRaidBootyEquipItemInfo.ItemIndex;
				starCount = guildRaidBootyEquipItemInfo.Star;
			}
		}
		else
		{
			GuildRaidBootyItemInfo guildRaidBootyItemInfo = GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.Get(itemId);
			if (guildRaidBootyItemInfo != null)
			{
				index = guildRaidBootyItemInfo.ItemIndex;
				int count = guildRaidBootyItemInfo.Count;
				itemCount = count;
			}
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
		if (data == null)
		{
			return string.Empty;
		}
		return data.Code;
	}

	public static int GetCurrentShopLevel(ShopData shopData)
	{
		int result = 1;
		if (IsNormalGuildShop(shopData))
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null)
			{
				return result;
			}
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			GuildBuildingInfo guildBuildingInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildBuildingInfo(dungeonIndex);
			if (guildBuildingInfo != null)
			{
				result = guildBuildingInfo.BuildingLevel;
			}
		}
		return result;
	}

	public static NPCFriendlyInfo GetCurrentSelectedNPCStep(int heroIndex)
	{
		NPCFriendlyInfo nPCFriendlyInfo = GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(heroIndex);
		if (nPCFriendlyInfo == null)
		{
			nPCFriendlyInfo = new NPCFriendlyInfo
			{
				FriendlyPoint = 0,
				Step = 1,
				HeroIndex = heroIndex
			};
		}
		return nPCFriendlyInfo;
	}

	public static int GetCurrentSelectedMaxDonatePoint(int heroIndex)
	{
		NPCFriendlyInfo currentSelectedNPCStep = GetCurrentSelectedNPCStep(heroIndex);
		return GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(currentSelectedNPCStep.HeroIndex, currentSelectedNPCStep.Step)?.ReqFriendlyPoint ?? 0;
	}

	public static int GetCurrentMyDonatePoint(int heroIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(heroIndex)?.FriendlyPoint ?? 0;
	}
}
