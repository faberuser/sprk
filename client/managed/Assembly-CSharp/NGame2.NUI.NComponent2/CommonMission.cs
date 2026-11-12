using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CommonMission
{
	public static void SetupSubQuestReward(SubQuestData subQuestData, CurrencyIconComponent.Listener listner, ref List<ItemIconComponent> itemIconList, ref List<CurrencyIconComponent> currencyIconList, IGrid grid)
	{
		RewardData rewardData = GetRewardData(subQuestData);
		if (rewardData == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = rewardData.GetRewardItemList();
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			if (rewardItemList[i] != null)
			{
				if (rewardItemList[i].GetCurrencyType() == CurrencyType.None)
				{
					AddItemIconComponent(rewardItemList[i], ref itemIconList, grid);
				}
				else
				{
					AddCurrencyIconComponent(rewardItemList[i].GetCurrencyType(), rewardItemList[i].ItemMin, listner, ref currencyIconList, grid);
				}
			}
		}
	}

	public static void SetupNodeIconSubQuestReward(SubQuestData subQuestData, CurrencyIconComponent.Listener listner, ref List<ItemIconComponent> itemIconList, ref List<CurrencyIconComponent> currencyIconList, IGrid grid)
	{
		RewardData rewardData = GetRewardData(subQuestData);
		if (rewardData == null)
		{
			return;
		}
		List<RewardElementData> nodeIconRewardItemList = rewardData.GetNodeIconRewardItemList();
		for (int i = 0; i < nodeIconRewardItemList.Count; i++)
		{
			if (nodeIconRewardItemList[i] != null)
			{
				if (nodeIconRewardItemList[i].GetCurrencyType() == CurrencyType.None)
				{
					AddItemIconComponent(nodeIconRewardItemList[i], ref itemIconList, grid);
				}
				else
				{
					AddCurrencyIconComponent(nodeIconRewardItemList[i].GetCurrencyType(), nodeIconRewardItemList[i].ItemMin, listner, ref currencyIconList, grid);
				}
				break;
			}
		}
	}

	public static RewardData GetRewardData(SubQuestData subQuestData)
	{
		if (subQuestData == null)
		{
			return null;
		}
		int num = ((subQuestData.VisualRewardIndex > 0) ? subQuestData.VisualRewardIndex : subQuestData.RewardIndex);
		if (num <= 0)
		{
			return null;
		}
		return GameRoot.Instance.RewardDataContainer.GetData(num);
	}

	private static void AddItemIconComponent(RewardElementData rewardData, ref List<ItemIconComponent> itemIconList, IGrid grid)
	{
		if (rewardData == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardData.ItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(grid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.Open(dataByCode);
				int itemMin = rewardData.ItemMin;
				Color? color = null;
				int num = itemMin;
				itemIconComponent.SetItemCount(num, color);
				itemIconList.Add(itemIconComponent);
			}
		}
	}

	private static void AddCurrencyIconComponent(CurrencyType type, int count, CurrencyIconComponent.Listener listner, ref List<CurrencyIconComponent> currencyIconList, IGrid grid)
	{
		CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
		if (!(currencyIconComponent == null))
		{
			currencyIconComponent.Open(type, count, listner);
			currencyIconList.Add(currencyIconComponent);
		}
	}

	public static void SetupAchievementReward(AchievementData achievementData, ref List<ItemIconComponent> itemIconList, ref List<CurrencyIconComponent> currencyIconList, IGrid grid, Color eventRewardColor)
	{
		if (achievementData == null)
		{
			return;
		}
		foreach (AchievementReward achievementReward in achievementData.GetAchievementRewardList())
		{
			IIconComponent iconComponent = CreateAchievementReward(achievementReward, achievementData, grid, eventRewardColor);
			if (iconComponent is ItemIconComponent)
			{
				itemIconList.Add(iconComponent as ItemIconComponent);
			}
			else if (iconComponent is CurrencyIconComponent)
			{
				currencyIconList.Add(iconComponent as CurrencyIconComponent);
			}
		}
		UIHelper.RepositionGrid(grid);
	}

	public static void SetupNodeIconAchievementReward(AchievementData achievementData, ref List<ItemIconComponent> itemIconList, ref List<CurrencyIconComponent> currencyIconList, IGrid grid, Color eventRewardColor)
	{
		if (achievementData == null)
		{
			return;
		}
		List<AchievementReward> achievementRewardList = achievementData.GetAchievementRewardList(isSorted: false);
		if (__Extension_Method_ICollection__.COUNT(achievementRewardList) > 0)
		{
			AchievementReward reward = achievementRewardList[0];
			if (__Extension_Method_ICollection__.COUNT(achievementRewardList) == 2 && AchievementRewardNodeIconCompare(achievementRewardList[0], achievementRewardList[1]) > 0)
			{
				reward = achievementRewardList[1];
			}
			IIconComponent iconComponent = CreateAchievementReward(reward, achievementData, grid, eventRewardColor);
			if (iconComponent is ItemIconComponent)
			{
				itemIconList.Add(iconComponent as ItemIconComponent);
			}
			else if (iconComponent is CurrencyIconComponent)
			{
				currencyIconList.Add(iconComponent as CurrencyIconComponent);
			}
		}
		UIHelper.RepositionGrid(grid);
	}

	public static IIconComponent CreateAchievementReward(AchievementReward reward, AchievementData achievementData, IGrid grid, Color eventRewardColor)
	{
		if (!AchievementData.ValidReward(reward))
		{
			return null;
		}
		Color color = Color.white;
		int num = 0;
		long num2 = num;
		int rewardValue = reward.RewardValue;
		long num3 = rewardValue;
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBooster(EventBonusType.BonusDailyMissionReward) != null && achievementData.Type == AchievementType.Daily)
		{
			int effectiveBoosterAddPercent = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBoosterAddPercent(EventBonusType.BonusDailyMissionReward);
			if (effectiveBoosterAddPercent > 0)
			{
				long num4 = num3;
				int rewardValue2 = reward.RewardValue;
				long num5 = (long)((float)effectiveBoosterAddPercent / 100f);
				int num6 = rewardValue2;
				num3 = checked(num4 + num6 * num5);
				color = eventRewardColor;
			}
		}
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.BonusDailyMissionReward, achievementData.Type.ToString());
		if (effectiveEvent != null && effectiveEvent.BonusPercentage > 0)
		{
			num2 = (long)((float)effectiveEvent.BonusPercentage / 100f);
			long num7 = num3;
			int rewardValue3 = reward.RewardValue;
			long num8 = num2;
			int num9 = rewardValue3;
			num3 = checked(num7 + num9 * num8);
			color = eventRewardColor;
		}
		if (reward.RewardType == AchievementRewardType.Item || reward.RewardType == AchievementRewardType.Booster)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(grid, ItemIconComponent.PrefabAuto);
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(reward.RewardCode);
			itemIconComponent.Open(dataByCode);
			long num10 = num3;
			int num11 = 1;
			if (num10 > num11)
			{
				itemIconComponent.SetItemCount(num3, color);
			}
			return itemIconComponent;
		}
		CurrencyType currencyType = CurrencyType.None;
		long currencyAmount = num3;
		AchievementRewardTypeAttribute achievementRewardTypeAttribute = EnumAttributeContainer<AchievementRewardType, AchievementRewardTypeAttribute>.Get(reward.RewardType);
		if (achievementRewardTypeAttribute != null)
		{
			currencyType = achievementRewardTypeAttribute.CurrencyType;
		}
		int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
		TeamLevelData data = GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel);
		checked
		{
			if (data != null)
			{
				if (reward.RewardType == AchievementRewardType.GoldTeamLevel)
				{
					int dailyGold = data.DailyGold;
					currencyAmount = dailyGold;
					long num12 = currencyAmount;
					int dailyGold2 = data.DailyGold;
					long num13 = num2;
					int num14 = dailyGold2;
					currencyAmount = num12 + num14 * num13;
				}
				else if (reward.RewardType == AchievementRewardType.StaminaTeamLevel)
				{
					int dailyBonusStamina = data.DailyBonusStamina;
					currencyAmount = dailyBonusStamina;
					long num15 = currencyAmount;
					int dailyBonusStamina2 = data.DailyBonusStamina;
					long num16 = num2;
					int num17 = dailyBonusStamina2;
					currencyAmount = num15 + num17 * num16;
				}
			}
			if (reward.RewardType == AchievementRewardType.GoldTeamLevel)
			{
				currencyType = CurrencyType.Gold;
			}
			else if (reward.RewardType == AchievementRewardType.StaminaTeamLevel)
			{
				currencyType = CurrencyType.Stamina;
			}
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
			currencyIconComponent.Open(currencyType, currencyAmount, null);
			currencyIconComponent.SetAmountColor(color);
			return currencyIconComponent;
		}
	}

	public static int AchievementRewardNodeIconCompare(AchievementReward a, AchievementReward b)
	{
		if (a.RewardType != AchievementRewardType.None && b.RewardType != AchievementRewardType.None)
		{
			if (a.RewardType == b.RewardType)
			{
				return 0;
			}
			if (a.RewardType == AchievementRewardType.Item && b.RewardType != AchievementRewardType.Item)
			{
				return -1;
			}
			if (a.RewardType != AchievementRewardType.Item && b.RewardType == AchievementRewardType.Item)
			{
				return 1;
			}
			if (a.RewardType == AchievementRewardType.Gold && a.RewardType != AchievementRewardType.Gold)
			{
				return 1;
			}
			if (a.RewardType != AchievementRewardType.Gold && a.RewardType == AchievementRewardType.Gold)
			{
				return -1;
			}
			if (a.RewardType == AchievementRewardType.Gem && a.RewardType != AchievementRewardType.Gem)
			{
				return 1;
			}
			if (a.RewardType != AchievementRewardType.Gem && a.RewardType == AchievementRewardType.Gem)
			{
				return -1;
			}
		}
		return 0;
	}

	public static HashSet<CurrencyType> GetCurrencyTypesFromAchievementData(AchievementData achievementData)
	{
		if (achievementData == null)
		{
			return null;
		}
		HashSet<CurrencyType> hashSet = null;
		List<AchievementReward> achievementRewardList = achievementData.GetAchievementRewardList(isSorted: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(achievementRewardList); i++)
		{
			AchievementRewardTypeAttribute achievementRewardTypeAttribute = EnumAttributeContainer<AchievementRewardType, AchievementRewardTypeAttribute>.Get(achievementRewardList[i].RewardType);
			if (achievementRewardTypeAttribute != null)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<CurrencyType>();
				}
				if (!hashSet.Contains(achievementRewardTypeAttribute.CurrencyType))
				{
					hashSet.Add(achievementRewardTypeAttribute.CurrencyType);
				}
			}
		}
		return hashSet;
	}

	public static HashSet<CurrencyType> GetCurrencyTypesFromSubQuestData(SubQuestData subQuestData)
	{
		if (subQuestData == null)
		{
			return null;
		}
		RewardData rewardData = GetRewardData(subQuestData);
		if (rewardData == null)
		{
			return null;
		}
		HashSet<CurrencyType> hashSet = null;
		List<RewardElementData> rewardItemList = rewardData.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData == null)
			{
				continue;
			}
			CurrencyType currencyType = rewardElementData.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<CurrencyType>();
				}
				if (!hashSet.Contains(currencyType))
				{
					hashSet.Add(currencyType);
				}
			}
		}
		return hashSet;
	}

	public static HashSet<CurrencyType> GetCurrencyTypesFromReward(MissionComponentData elementData)
	{
		if (elementData == null)
		{
			return null;
		}
		AchievementData data = elementData.GetData<AchievementData>();
		if (data != null)
		{
			return GetCurrencyTypesFromAchievementData(data);
		}
		SubQuestData data2 = elementData.GetData<SubQuestData>();
		if (data2 != null)
		{
			return GetCurrencyTypesFromSubQuestData(data2);
		}
		return null;
	}
}
