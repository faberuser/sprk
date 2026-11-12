using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.CancelDispatch;
using NShared.GetDispatchList;
using NShared.NLocalization;
using NShared.RequestCompleteDispatch;
using NShared.StartDispatch;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NDispatchBattle;

public class DispatchHelper
{
	public enum ContentMode
	{
		None,
		Chapter,
		Raid
	}

	public const float VALUE_PERCENTAGE_RED = 0.4f;

	public static ContentMode GetContentMode(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return ContentMode.None;
		}
		if (GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(info.ChapterIndex, info.DungeonIndex) != null)
		{
			return ContentMode.Raid;
		}
		return ContentMode.Chapter;
	}

	public static DispatchType GetDispatchType(WCL_DispatchBattleInfo info)
	{
		DispatchType result = DispatchType.None;
		if (info == null)
		{
			return result;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(info.ChapterIndex, info.DungeonIndex);
		if (data == null)
		{
			return result;
		}
		switch (GetContentMode(info))
		{
		case ContentMode.Chapter:
			if (info.Difficulty == 0)
			{
				result = data.DispatchType_Easy;
			}
			else if (info.Difficulty == 1)
			{
				result = data.DispatchType_Normal;
			}
			else if (info.Difficulty == 2)
			{
				result = data.DispatchType_Hard;
			}
			else if (info.Difficulty == 3)
			{
				result = data.DispatchType_Hell;
			}
			break;
		case ContentMode.Raid:
			result = data.DispatchType_Easy;
			break;
		}
		return result;
	}

	public static float GetWinRate(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return 0f;
		}
		return GetWinRate(info.LastWinCount, info.LastLoseCount);
	}

	public static float GetWinRate(int winCount, int loseCount)
	{
		float num = winCount + loseCount;
		if (num == 0f)
		{
			return 0f;
		}
		if (winCount == 0)
		{
			return 0f;
		}
		return (float)winCount / num;
	}

	public static string GetDispatchStateString(DispatchState state)
	{
		switch (state)
		{
		case DispatchState.Prepare:
		case DispatchState.Simulation:
			return LocalizationManager.GetString(DispatchBattle.DISPATCH_BATTLE_PREPAREING);
		case DispatchState.Battle:
			return LocalizationManager.GetString(DispatchBattle.DISPATCH_BATTLE_PROGRESSING);
		case DispatchState.ReadyToComplete:
			return LocalizationManager.GetString(DispatchBattle.DISPATCH_BATTLE_COMPLETE);
		default:
			return "";
		}
	}

	public static string GetPrepareRandomDescription(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return "";
		}
		DispatchState dispatchState = GetDispatchState(info);
		DispatchStateData randomDataInState = GameRoot.Instance.DispatchStateDataContainer.GetRandomDataInState(dispatchState);
		if (randomDataInState == null)
		{
			return "";
		}
		return LocalizationManager.GetStringDirect(randomDataInState.StateDescKey);
	}

	public static string GetRemainTimeFormattedStr(WCL_DispatchBattleInfo info, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS)
	{
		if (info == null)
		{
			return "";
		}
		return GetRemainTimeFormattedStr(info.RemainTime, formattedStr_HH_MM_SS, formattedStr_MM_SS);
	}

	public static string GetTotalTimeFormattedStr(WCL_DispatchBattleInfo info, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS)
	{
		if (info == null)
		{
			return "";
		}
		return GetTotalTimeFormattedStr(info.TotalTime, formattedStr_HH_MM_SS, formattedStr_MM_SS);
	}

	public static string GetTotalTimeFormattedStr(TimeSpan totalTime, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS)
	{
		return GetRemainTimeFormattedStr(totalTime, formattedStr_HH_MM_SS, formattedStr_MM_SS);
	}

	public static int GetDifficultyOnChapter(EnterDungeonData data)
	{
		return (int)(data?.Difficulty ?? NShared.DungeonDifficultyType.Easy);
	}

	public static int GetDifficultyOnRaid(RaidData data)
	{
		return data?.Level ?? 0;
	}

	public static int GetRaidMaxCompleteLv(int multiRaidIndex)
	{
		int raidMaxLevel = RaidHelper.GetRaidMaxLevel(multiRaidIndex);
		int raidMaxLevel2 = GameRoot.Instance.RaidDataContainer.GetRaidMaxLevel(multiRaidIndex);
		if (raidMaxLevel < raidMaxLevel2)
		{
			return raidMaxLevel - 1;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(multiRaidIndex, raidMaxLevel);
		if (data == null)
		{
			return 0;
		}
		RaidData raidDataSingleVersion = RaidHelper.GetRaidDataSingleVersion(data);
		if (raidDataSingleVersion == null)
		{
			return 0;
		}
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(data.ChapterIndex, data.DungeonIndex);
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(raidDataSingleVersion.ChapterIndex, raidDataSingleVersion.DungeonIndex) || flag)
		{
			return raidMaxLevel;
		}
		return raidMaxLevel - 1;
	}

	public static int GetRaidMaxCompleteLv(RaidData multiRaidData)
	{
		if (multiRaidData == null)
		{
			return 0;
		}
		return GetRaidMaxCompleteLv(multiRaidData.Index);
	}

	public static DispatchState GetDispatchState(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return DispatchState.Prepare;
		}
		if (info.State == DispatchState.Simulation)
		{
			return DispatchState.Prepare;
		}
		return info.State;
	}

	public static BaseRewardPopup.PopupOpenData GetPopupOpenData(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return new BaseRewardPopup.PopupOpenData();
		}
		return new BaseRewardPopup.PopupOpenData().SetCurrencyResultInfos3(info.CurrencyResults).SetItemResultInfos(info.ItemResults).SetStaminaResultInfos(info.StaminaResult);
	}

	public static bool IsDispatchedHero(int heroIndex)
	{
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager == null)
		{
			return false;
		}
		return IsDispatchedHero(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex));
	}

	public static bool IsDispatchedHero(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.GetByHeroIndex(heroInfo.HeroIndex) == null)
		{
			return false;
		}
		return true;
	}

	public static bool IsDispatchable(int chapterIndex, int dungeonIndex)
	{
		return IsDispatchable(GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex));
	}

	public static bool IsDispatchable(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		if (dungeonData.DispatchType_Easy != DispatchType.None)
		{
			return true;
		}
		if (dungeonData.DispatchType_Normal != DispatchType.None)
		{
			return true;
		}
		if (dungeonData.DispatchType_Hard != DispatchType.None)
		{
			return true;
		}
		if (dungeonData.DispatchType_Hell != DispatchType.None)
		{
			return true;
		}
		return false;
	}

	public static bool IsSameInfo(WCL_DispatchBattleInfo info1, WCL_DispatchBattleInfo info2)
	{
		if (info1 == null || info2 == null)
		{
			return false;
		}
		if (info1.SlotIndex != info2.SlotIndex)
		{
			return false;
		}
		return true;
	}

	public static bool IsExistCompleteDispatch(RaidData raidData)
	{
		if (raidData == null)
		{
			return false;
		}
		RaidData raidData2 = raidData;
		if (raidData.SingleRaidIndex != 0)
		{
			raidData2 = RaidHelper.GetRaidDataSingleVersion(raidData);
		}
		if (raidData2 == null)
		{
			return false;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(raidData2.ChapterIndex, raidData2.DungeonIndex);
		if (data == null)
		{
			return false;
		}
		DispatchType dispatchType_Easy = data.DispatchType_Easy;
		List<WCL_DispatchBattleInfo> listByChapterIndex = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.GetListByChapterIndex(dispatchType_Easy);
		int i = 0;
		for (int count = listByChapterIndex.Count; i < count; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = listByChapterIndex[i];
			if (wCL_DispatchBattleInfo != null && wCL_DispatchBattleInfo.State == DispatchState.ReadyToComplete)
			{
				return true;
			}
		}
		return false;
	}

	public static void SendRQGetDispatchList(Action onComplete)
	{
		NShared.GetDispatchList.Request request = new NShared.GetDispatchList.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.GetDispatchList.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.GetDispatchList.ResultType.Success)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
				{
					if (response.DispatchBattleInfos == null)
					{
						return;
					}
					List<WCL_DispatchBattleInfo> list = new List<WCL_DispatchBattleInfo>(__Extension_Method_ICollection__.COUNT(response.DispatchBattleInfos));
					int i = 0;
					for (int num = response.DispatchBattleInfos.Length; i < num; i++)
					{
						DispatchBattleInfo dispatchBattleInfo = response.DispatchBattleInfos[i];
						if (dispatchBattleInfo != null)
						{
							list.Add(new WCL_DispatchBattleInfo().Copy(dispatchBattleInfo));
						}
					}
					GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Set(list.ToArray());
				}
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				VespaDebug.LogError($"LobbyManagement::UpdateDispatchBattleInfo - Failed to Get DispatchBattleList {response.Result.ToString()}");
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public static void SendRQStartDispatch(int chapterIndex, int dungeonIndex, int difficulty, int deckIndex, int[] heroIndices, int repeatCount, Action<WCL_DispatchBattleInfo> onComplete)
	{
		NShared.StartDispatch.Request request = new NShared.StartDispatch.Request();
		request.ChapterIndex = chapterIndex;
		request.DungeonIndex = dungeonIndex;
		request.DeckIndex = deckIndex;
		request.Difficulty = difficulty;
		request.RepeatCount = repeatCount;
		request.HeroIndices = heroIndices;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.ShowLoadingGauge, delegate(NShared.StartDispatch.Response response)
		{
			SprkDispatch.Started(response);
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.StartDispatch.ResultType.Success)
			{
				WCL_DispatchBattleInfo wCL_DispatchBattleInfo = new WCL_DispatchBattleInfo().Copy(response.DispatchBattleInfo);
				if (AccountManager.IsLogined())
				{
					if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
					{
						GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Add(wCL_DispatchBattleInfo);
					}
					if (GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					}
				}
				Vespa_System_Action_Extension_Method.fire(onComplete, wCL_DispatchBattleInfo);
			}
			else
			{
				VespaDebug.LogError($"ContentUIDispatchBattle::SendDispatchBattle - Failed to StartDispatch (BaseResult: {response.BaseResult.ToString()}, Result: {response.Result.ToString()})");
				SprkDispatch.StartRejected(response);
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
		});
	}

	public static void SendRQCancelDispatch(int slotIndex, Action<WCL_DispatchBattleInfo> onComplete)
	{
		NShared.CancelDispatch.Request request = new NShared.CancelDispatch.Request();
		request.SlotIndex = slotIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.ShowLoadingGauge, delegate(NShared.CancelDispatch.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.CancelDispatch.ResultType.Success)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Remove(response.DispatchBattleInfo.SlotIndex);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.EclipseStaminaResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.TeamExpResult);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.FlaskResults);
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroExp(response.HeroExpResults);
				}
				List<StaminaResultInfo> list = new List<StaminaResultInfo> { response.StaminaResult, response.EclipseStaminaResult };
				WCL_DispatchBattleInfo wCL_DispatchBattleInfo = new WCL_DispatchBattleInfo().Copy(response.DispatchBattleInfo);
				wCL_DispatchBattleInfo.SetRewardInfo(response.CurrencyResults, response.ItemResults, list.ToArray());
				Vespa_System_Action_Extension_Method.fire(onComplete, wCL_DispatchBattleInfo);
			}
			else
			{
				VespaDebug.LogError($"ContentUIDispatchBattle::SendRQCancelDispatch - Failed to cancel dispatch (BaseResult: {response.BaseResult.ToString()}, Result: {response.Result.ToString()})");
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
		});
	}

	public static void SendRQRequestCompleteDispatch(int slotIndex, Action<WCL_DispatchBattleInfo> onComplete)
	{
		NShared.RequestCompleteDispatch.Request request = new NShared.RequestCompleteDispatch.Request();
		request.SlotIndex = slotIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.ShowLoadingGauge, delegate(NShared.RequestCompleteDispatch.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.RequestCompleteDispatch.ResultType.Success)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Remove(response.DispatchBattleInfo.SlotIndex);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.EclipseStaminaResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.TeamExpResult);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.FlaskResults);
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroExp(response.HeroExpResults);
				}
				List<StaminaResultInfo> list = new List<StaminaResultInfo> { response.EclipseStaminaResult };
				WCL_DispatchBattleInfo wCL_DispatchBattleInfo = new WCL_DispatchBattleInfo().Copy(response.DispatchBattleInfo);
				wCL_DispatchBattleInfo.SetRewardInfo(response.CurrencyResults, response.ItemResults, list.ToArray());
				Vespa_System_Action_Extension_Method.fire(onComplete, wCL_DispatchBattleInfo);
			}
			else
			{
				VespaDebug.LogError($"ContentUIDispatchBattle::SendRQRequestCompleteDispatch - Failed to requestCompleteDispatch (BaseResult: {response.BaseResult.ToString()}, Result: {response.Result.ToString()})");
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
		});
	}

	private static string GetRemainTimeFormattedStr(TimeSpan Time, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS)
	{
		TimeSpan timeSpan = Time;
		if (timeSpan.Ticks < 0)
		{
			timeSpan = new TimeSpan(0L);
		}
		string text = "";
		if (timeSpan.Hours != 0)
		{
			return LocalizationManager.GetString(formattedStr_HH_MM_SS, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}
		return LocalizationManager.GetString(formattedStr_MM_SS, timeSpan.Minutes, timeSpan.Seconds);
	}
}
