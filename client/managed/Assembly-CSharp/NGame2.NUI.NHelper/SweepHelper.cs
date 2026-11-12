using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NShared.SweepDungeon;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper;

public static class SweepHelper
{
	public static void OpenSweepPopup(CampaignDungeonData dungeonData, ISweepDungeonWindow sweepWindow)
	{
		if (dungeonData == null)
		{
			VespaDebug.LogError("dungeonData == null");
		}
		else if (!IsRemainTicket(dungeonData))
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("SWEEP_ERROR_ITEM_DESC");
			MessagePopup3Button.Open(title, stringDirect, null, delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Stamina.ToString());
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			SweepPopup.Open(dungeonData, sweepWindow);
		}
	}

	public static void RequestSweep(CampaignDungeonData dungeonData, long sweepCount, string[] selectedRewards, Action onSuccess = null)
	{
		NShared.SweepDungeon.Request request = new NShared.SweepDungeon.Request
		{
			ChapterIndex = dungeonData.ChapterIndex,
			DungeonIndex = dungeonData.DungeonIndex,
			DungeonDifficulty = 0,
			SweepCount = sweepCount,
			SelectedRewardItemCodes = selectedRewards
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SweepDungeon.Response response)
		{
			if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Apply(response.CampaignResults);
				GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Apply(response.TowerInfoResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
				popupOpenData.SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemInfos).SetCurrencyResultInfos3(response.CurrencyResults);
				BattleType battleType = BattleType.Eclipse;
				long count = sweepCount;
				int start = response.StartDungeonIndex;
				int end = response.EndDungeonIndex - 1;
				if (dungeonData.SweepDungeonType != SweepDungeonType.Eclipse)
				{
					battleType = BattleType.None;
					CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(dungeonData.ChapterIndex, response.StartDungeonIndex);
					CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(dungeonData.ChapterIndex, response.EndDungeonIndex);
					start = GetDungeonStep(data);
					end = GetDungeonStep(data2);
				}
				SweepResultPopup.Open(popupOpenData, battleType, delegate
				{
				}).SetText(dungeonData.SweepDungeonType, start, end, count);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public static bool IsPreSelectRewardDungeon(SweepDungeonType sweepDungeonType)
	{
		if (sweepDungeonType == SweepDungeonType.Shakmeh || sweepDungeonType == SweepDungeonType.MiddleShakmeh)
		{
			return true;
		}
		return false;
	}

	public static bool IsRemainTicket(CampaignDungeonData dungeonData)
	{
		int sweepTicketCount = dungeonData.SweepTicketCount;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SweepTicketItemIndex");
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(valueInt) >= sweepTicketCount;
	}

	public static bool IsRemainStamina(CampaignDungeonData dungeonData)
	{
		StaminaType staminaType = dungeonData.ReqStaminaType;
		int num = dungeonData.ReqStamina + dungeonData.AdvancedReqStamina;
		if (dungeonData.SweepDungeonType == SweepDungeonType.Eclipse)
		{
			staminaType = StaminaType.EclipseKey;
			num = 1;
		}
		long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(staminaType);
		int num2 = num;
		return stamina >= num2;
	}

	public static bool IsPossibleSweepStep(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		if (dungeonData.SweepDungeonType == SweepDungeonType.None)
		{
			return false;
		}
		TopClearDungeonInfo topClearInfo = GetTopClearInfo(dungeonData);
		if (topClearInfo == null || topClearInfo.DungeonIndex <= 0)
		{
			return false;
		}
		bool flag = false;
		if (dungeonData.SweepDungeonType == SweepDungeonType.Eclipse)
		{
			int key = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo()?.MaxWaveIndex ?? 0;
			int num = 1;
			EclipseStartData data = GameRoot.Instance.EclipseStartDataContainer.GetData(key);
			if (data != null)
			{
				num = ((data.StartWave <= 0) ? 1 : data.StartWave);
			}
			return topClearInfo.DungeonIndex > num;
		}
		return topClearInfo.DungeonIndex >= dungeonData.DungeonIndex;
	}

	public static bool IsExistClearRecord(CampaignDungeonData dungeonData)
	{
		TopClearDungeonInfo topClearInfo = GetTopClearInfo(dungeonData);
		if (topClearInfo != null)
		{
			return topClearInfo.DungeonIndex > 0;
		}
		return false;
	}

	public static void GetSweepReqItemInfo(CampaignDungeonData dungeonData, ref StaminaType staminaType, ref int staminaCount, ref int ticketCount)
	{
		if (dungeonData != null)
		{
			ticketCount = dungeonData.SweepTicketCount;
			if (dungeonData.SweepDungeonType == SweepDungeonType.Eclipse)
			{
				staminaType = StaminaType.EclipseKey;
				staminaCount = 1;
			}
			else
			{
				staminaType = dungeonData.ReqStaminaType;
				staminaCount = dungeonData.ReqStamina + dungeonData.AdvancedReqStamina;
			}
		}
	}

	public static CurrencyType GetCurrencyTypeByStaminaType(StaminaType staminaType)
	{
		return EnumAttributeContainer<StaminaType, StaminaTypeAttribute>.Get(staminaType)?.CurrencyType ?? CurrencyType.None;
	}

	public static long GetPossibleSweepMaxCount(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			int num = 0;
			return num;
		}
		if (dungeonData.SweepDungeonType == SweepDungeonType.Shakmeh)
		{
			int num2 = 1;
			return num2;
		}
		int sweepTicketCount = dungeonData.SweepTicketCount;
		int sweepTicketMaxCount = dungeonData.SweepTicketMaxCount;
		int num3 = dungeonData.ReqStamina + dungeonData.AdvancedReqStamina;
		StaminaType staminaType = dungeonData.ReqStaminaType;
		if (dungeonData.SweepDungeonType == SweepDungeonType.Eclipse)
		{
			staminaType = StaminaType.EclipseKey;
			num3 = 1;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SweepTicketItemIndex");
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(valueInt);
		long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(staminaType);
		int num4 = ((sweepTicketCount == 0) ? int.MaxValue : (itemCount / sweepTicketCount));
		long num6;
		if (num3 != 0)
		{
			int num5 = num3;
			num6 = stamina / num5;
		}
		else
		{
			int num7 = 1;
			num6 = num7;
		}
		long num8 = num6;
		long val = num8;
		int num9 = num4;
		long num10 = System.Math.Min(num9, val);
		long num11 = num10;
		int num12 = 0;
		if (num11 > num12 && dungeonData.SweepDungeonType == SweepDungeonType.MiddleShakmeh && !SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.IsEnoughShakmehBossGauge)
		{
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(CurrencyType.ShakmehMiddleBossPoint);
			long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.ShakmehMiddleBossPoint);
			long num13 = checked(data.MaxValue - userStat);
			int expectedBossPointByDungeon = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.GetExpectedBossPointByDungeon(dungeonData.DungeonIndex);
			int num14 = (int)System.Math.Ceiling((float)num13 / (float)expectedBossPointByDungeon);
			long val2 = num10;
			int num15 = num14;
			num10 = System.Math.Min(val2, num15);
		}
		long val3 = num10;
		int num16 = sweepTicketMaxCount;
		return System.Math.Min(val3, num16);
	}

	public static TopClearDungeonInfo GetTopClearInfo(CampaignDungeonData dungeonData)
	{
		TopClearDungeonInfo[] topClearDungeonInfos = GlobalAutoBehaviour<AccountManager>.instance.TopClearDungeonInfos;
		TopClearDungeonInfo result = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(topClearDungeonInfos); i++)
		{
			TopClearDungeonInfo topClearDungeonInfo = topClearDungeonInfos[i];
			if (topClearDungeonInfo.SweepDungeonType == dungeonData.SweepDungeonType)
			{
				result = topClearDungeonInfo;
				break;
			}
		}
		return result;
	}

	public static CampaignDungeonData GetTopClearDungeonData(CampaignDungeonData dungeonData)
	{
		TopClearDungeonInfo topClearInfo = GetTopClearInfo(dungeonData);
		if (topClearInfo == null)
		{
			return null;
		}
		int chapterIndex = dungeonData.ChapterIndex;
		int dungeonIndex = ((topClearInfo.SweepDungeonType != SweepDungeonType.Eclipse) ? topClearInfo.DungeonIndex : dungeonData.DungeonIndex);
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
	}

	public static int GetTopClearDungeonStep(CampaignDungeonData dungeonData)
	{
		TopClearDungeonInfo topClearInfo = GetTopClearInfo(dungeonData);
		if (topClearInfo == null || topClearInfo.DungeonIndex <= 0)
		{
			return 0;
		}
		int result = 1;
		switch (topClearInfo.SweepDungeonType)
		{
		case SweepDungeonType.ChallengeTower:
		case SweepDungeonType.MazeKnight:
		case SweepDungeonType.MazeWarrior:
		case SweepDungeonType.MazeAssassin:
		case SweepDungeonType.MazeArcher:
		case SweepDungeonType.MazeMechanic:
		case SweepDungeonType.MazeWizard:
		case SweepDungeonType.MazePriest:
		{
			TowerFloorData towerDataByDungeon = GameRoot.Instance.TowerFloorDataContainer.GetTowerDataByDungeon(dungeonData.ChapterIndex, topClearInfo.DungeonIndex);
			if (towerDataByDungeon != null)
			{
				result = towerDataByDungeon.Floor;
			}
			break;
		}
		case SweepDungeonType.MiddleShakmeh:
		case SweepDungeonType.Shakmeh:
			result = GameRoot.Instance.ShakmehDungeonDataContainer.GetDataByDungeonIndex(topClearInfo.DungeonIndex)?.Level ?? topClearInfo.DungeonIndex;
			break;
		case SweepDungeonType.Eclipse:
			result = topClearInfo.DungeonIndex;
			break;
		}
		return result;
	}

	public static int GetDungeonStep(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return 0;
		}
		if (dungeonData.SweepDungeonType == SweepDungeonType.Eclipse)
		{
			return 0;
		}
		int result = 1;
		switch (dungeonData.SweepDungeonType)
		{
		case SweepDungeonType.ChallengeTower:
		case SweepDungeonType.MazeKnight:
		case SweepDungeonType.MazeWarrior:
		case SweepDungeonType.MazeAssassin:
		case SweepDungeonType.MazeArcher:
		case SweepDungeonType.MazeMechanic:
		case SweepDungeonType.MazeWizard:
		case SweepDungeonType.MazePriest:
		{
			TowerFloorData towerDataByDungeon = GameRoot.Instance.TowerFloorDataContainer.GetTowerDataByDungeon(dungeonData.ChapterIndex, dungeonData.DungeonIndex);
			if (towerDataByDungeon != null)
			{
				result = towerDataByDungeon.Floor;
			}
			break;
		}
		case SweepDungeonType.MiddleShakmeh:
		case SweepDungeonType.Shakmeh:
			result = GameRoot.Instance.ShakmehDungeonDataContainer.GetDataByDungeonIndex(dungeonData.DungeonIndex)?.Level ?? dungeonData.DungeonIndex;
			break;
		}
		return result;
	}

	public static string GetStepLocalText(SweepDungeonType dungeonType, long floor)
	{
		int num = 0;
		if (floor <= num)
		{
			return "-";
		}
		string key = string.Empty;
		switch (dungeonType)
		{
		case SweepDungeonType.MazeKnight:
		case SweepDungeonType.MazeWarrior:
		case SweepDungeonType.MazeAssassin:
		case SweepDungeonType.MazeArcher:
		case SweepDungeonType.MazeMechanic:
		case SweepDungeonType.MazeWizard:
		case SweepDungeonType.MazePriest:
			key = "SWEEP_RESULT_START_DESC_MAZE";
			break;
		case SweepDungeonType.ChallengeTower:
			key = "SWEEP_RESULT_START_DESC_CHALLENGETOWER";
			break;
		case SweepDungeonType.MiddleShakmeh:
		case SweepDungeonType.Shakmeh:
			key = "SWEEP_RESULT_DIFFICULTY_DESC";
			break;
		case SweepDungeonType.Eclipse:
			key = "SWEEP_RESULT_START_DESC_ECLIPSE";
			break;
		}
		return LocalizationManager.GetStringDirect(key, floor);
	}
}
