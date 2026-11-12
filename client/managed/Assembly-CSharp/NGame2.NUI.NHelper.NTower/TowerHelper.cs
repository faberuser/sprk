using System;
using System.Collections.Generic;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NShared.NUtil;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NHelper.NTower;

public static class TowerHelper
{
	public static string GetLocalizedFloorText(int floor, bool isUnderground = false)
	{
		string empty = string.Empty;
		empty = floor switch
		{
			1 => LocalizationManager.GetString(Tower.TOWER_FLOOR_NUMBER_1), 
			2 => LocalizationManager.GetString(Tower.TOWER_FLOOR_NUMBER_2), 
			3 => LocalizationManager.GetString(Tower.TOWER_FLOOR_NUMBER_3), 
			_ => LocalizationManager.GetString(Tower.TOWER_FLOOR_NUMBER, floor), 
		};
		if (isUnderground)
		{
			empty = LocalizationManager.GetStringDirect("MAZE_FLOOR", floor);
		}
		return empty;
	}

	public static int GetFloorDungeonSlot(TowerFloorData floorData)
	{
		if (floorData == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.COUNT(floorData.DungeonIndices) > 0)
		{
			if (floorData.DungeonSelectType == FloorDungeonSelectType.WeeklyRandom)
			{
				return RequestEnterLobbyWrapper.WeekOfYear % floorData.DungeonIndices.Length;
			}
			if (floorData.DungeonSelectType == FloorDungeonSelectType.MonthlyRandom)
			{
				_ = new int[12]
				{
					7, 8, 5, 4, 1, 2, 6, 9, 3, 12,
					11, 10
				};
				return RequestEnterLobbyWrapper.MonthOfYear % floorData.DungeonIndices.Length;
			}
		}
		return 0;
	}

	public static int GetFloorDungeonIndex(TowerFloorData floorData)
	{
		if (floorData == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.COUNT(floorData.DungeonIndices) == 0)
		{
			return floorData.DungeonIndex;
		}
		int floorDungeonSlot = GetFloorDungeonSlot(floorData);
		if (floorDungeonSlot >= floorData.DungeonIndices.Length)
		{
			return floorData.DungeonIndex;
		}
		return floorData.DungeonIndices[floorDungeonSlot];
	}

	public static void SetupReward(TowerFloorData floorData, int completedFloor, IGrid grid, List<Component> componentList, bool isShowEventDungeonPoint3 = false)
	{
		int[] array = ((floorData.Floor <= completedFloor && __Extension_Method_ICollection__.COUNT(floorData.RepeatRewardIndex) > 0) ? floorData.RepeatRewardIndex : floorData.RewardIndex);
		if (__Extension_Method_ICollection__.COUNT(array) == 0)
		{
			VespaDebug.LogError("reward not found : floor=" + floorData?.ToString() + " completedFloor=" + completedFloor);
			return;
		}
		int floorDungeonSlot = GetFloorDungeonSlot(floorData);
		int key = ((floorDungeonSlot < __Extension_Method_ICollection__.COUNT(array)) ? array[floorDungeonSlot] : array[__Extension_Method_ICollection__.COUNT(array) - 1]);
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(key);
		if (data == null)
		{
			VespaDebug.LogError("reward not found : dungeonSlot=" + floorDungeonSlot + " rewardIndex=" + key);
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (rewardItemList == null)
		{
			return;
		}
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			HashString itemCode = rewardElementData.ItemCode;
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			CurrencyTypeData currencyTypeData = null;
			if (dataByCode == null)
			{
				if (!isShowEventDungeonPoint3 || (!itemCode.IsNullOrEmpty() && itemCode.ToString() != CurrencyType.EventDungeonPoint3.ToString()))
				{
					continue;
				}
				currencyTypeData = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.EventDungeonPoint3);
				if (currencyTypeData == null)
				{
					continue;
				}
			}
			if (rewardElementData.ItemMin == 0 && rewardElementData.ItemMax == 0)
			{
				continue;
			}
			Component component = null;
			if (currencyTypeData != null)
			{
				CurrencyIconComponent currencyIconComponent = grid.AddGridItem(CurrencyIconComponent.PrefabAuto);
				CurrencyType currencyType = currencyTypeData.CurrencyType;
				int itemMax = rewardElementData.ItemMax;
				CurrencyIconComponent.Listener listener = null;
				int num = itemMax;
				currencyIconComponent.Open(currencyType, num, listener);
				if (rewardElementData.ItemMin != rewardElementData.ItemMax)
				{
					currencyIconComponent.SetMinMaxCount(rewardElementData.ItemMin, rewardElementData.ItemMax);
				}
				component = currencyIconComponent;
			}
			else
			{
				ItemIconComponent itemIconComponent = grid.AddGridItem(ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				if (rewardElementData.ItemMin != rewardElementData.ItemMax)
				{
					int itemMin = rewardElementData.ItemMin;
					int itemMax2 = rewardElementData.ItemMax;
					Color? color = null;
					int num2 = itemMax2;
					int num3 = itemMin;
					itemIconComponent.SetItemMinMax(num3, num2, color);
				}
				else
				{
					int itemMin2 = rewardElementData.ItemMin;
					Color? color2 = null;
					int num4 = itemMin2;
					itemIconComponent.SetItemCount(num4, color2);
				}
				component = itemIconComponent;
			}
			componentList.Add(component);
		}
		grid.RepositionGrid();
	}

	public static void SetupFloorSkillIcons(TowerFloorData floorData, IGrid grid, List<SkillIconComponent> componentList)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(floorData.ChapterIndex, floorData.DungeonIndex);
		if (data == null)
		{
			return;
		}
		int randomByMonth = RandomTable.GetRandomByMonth(RequestEnterLobbyWrapper.MonthFromMillennium, floorData.DungeonIndex);
		int randomProjectileGroupIndex = data.GetRandomProjectileGroupIndex(randomByMonth);
		SkillProjectileGroupData data2 = GameRoot.Instance.SkillProjectileGroupDataContainer.GetData(randomProjectileGroupIndex);
		if (data2 == null)
		{
			return;
		}
		foreach (SkillProjectileGroupData.SkillProjectileInfo skillProjectileInfo in data2.GetSkillProjectileInfos())
		{
			SkillProjectileData data3 = GameRoot.Instance.SkillProjectileDataContainer.GetData(skillProjectileInfo.Index);
			if (data3 != null)
			{
				SkillData data4 = GameRoot.Instance.SkillDataContainer.GetData(data3.SkillIndex);
				if (data4 != null)
				{
					SkillIconComponent skillIconComponent = grid.AddGridItem(SkillIconComponent.PrefabAuto);
					skillIconComponent.Open(data4.Index);
					componentList.Add(skillIconComponent);
				}
			}
		}
		grid.RepositionGrid();
	}

	public static int GetRemainResetTime(int towerIndex)
	{
		TowerData data = GameRoot.Instance.TowerDataContainer.GetData(towerIndex);
		if (data == null)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.COUNT(data.MonthlyResetDate) > 0)
		{
			return RequestEnterLobbyWrapper.GetRemainMonthSecond(data.MonthlyResetDate);
		}
		if (data.WeeklyReset)
		{
			return RequestEnterLobbyWrapper.GetRemainWeekSecond();
		}
		if (data.DailyReset)
		{
			return RequestEnterLobbyWrapper.GetRemainDaySecond();
		}
		return 0;
	}

	public static void SetupRemainResetTimeLabel(UILabel label, int towerIndex)
	{
		UIHelper.SetLabelText(label, "");
		TowerData data = GameRoot.Instance.TowerDataContainer.GetData(towerIndex);
		if (data != null)
		{
			if (__Extension_Method_ICollection__.COUNT(data.MonthlyResetDate) > 0)
			{
				int remainMonthSecond = RequestEnterLobbyWrapper.GetRemainMonthSecond(data.MonthlyResetDate);
				UIHelper.SetRemainTimeLabelText(label, remainMonthSecond);
			}
			else if (data.WeeklyReset)
			{
				int remainWeekSecond = RequestEnterLobbyWrapper.GetRemainWeekSecond();
				UIHelper.SetRemainTimeLabelText(label, remainWeekSecond);
			}
			else if (data.DailyReset)
			{
				int remainDaySecond = RequestEnterLobbyWrapper.GetRemainDaySecond();
				UIHelper.SetRemainTimeLabelText(label, remainDaySecond);
			}
		}
	}

	public static bool SetupRemainResetTimeLabel(UILabel label, int towerIndex, float limitDay)
	{
		UIHelper.SetLabelText(label, "");
		TowerData data = GameRoot.Instance.TowerDataContainer.GetData(towerIndex);
		if (data == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(data.MonthlyResetDate) > 0)
		{
			int remainMonthSecond = RequestEnterLobbyWrapper.GetRemainMonthSecond(data.MonthlyResetDate);
			double totalDays = TimeSpan.FromSeconds(remainMonthSecond).TotalDays;
			if ((double)limitDay >= totalDays)
			{
				string remainTimeText = UIHelper.GetRemainTimeText(remainMonthSecond);
				string stringDirect = LocalizationManager.GetStringDirect("IMPROVE_RESETREMAININGDATE", remainTimeText);
				UIHelper.SetLabelText(label, stringDirect);
				return true;
			}
		}
		return false;
	}
}
