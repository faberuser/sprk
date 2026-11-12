using System;
using NGame2.NAccount;
using NGame2.NOrdeal;
using NShared;
using NShared.PetCancelExplore;
using NShared.PetEndExplore;
using NShared.PetStartExplore;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper.NPet;

public static class PetHelper
{
	public enum EPetExploreInfoStatus
	{
		Start,
		Cancel,
		End,
		Error
	}

	private static readonly float PetFoodCountUpdateDelayTime = 1f;

	private static DateTime? _lastPetFoodCountUpdatedTime = null;

	private const int DISABLE_INT_VALUE = -1;

	public static TimeSpan? GetPetGetUpRemainTime(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			return null;
		}
		if (String_Extension_Method.IsNullOrEmpty(petInfo.EndPenaltyTime))
		{
			return null;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return GetPetGetUpRemainTime(petInfo.EndPenaltyTime);
	}

	public static TimeSpan? GetPetGetUpRemainTime(string endPenaltyTimeStr)
	{
		if (String_Extension_Method.IsNullOrEmpty(endPenaltyTimeStr))
		{
			return null;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(endPenaltyTimeStr, out var o_dateTime))
		{
			return null;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		TimeSpan value = o_dateTime - currentServerUTCTime;
		if (value.Ticks < 0)
		{
			return null;
		}
		return value;
	}

	public static bool CheckIsSleep(PetInfo petInfo)
	{
		if (petInfo == null || String_Extension_Method.IsNullOrEmpty(petInfo.EndPenaltyTime))
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		TimeSpan? petGetUpRemainTime = GetPetGetUpRemainTime(petInfo.EndPenaltyTime);
		if (!petGetUpRemainTime.HasValue)
		{
			return false;
		}
		return (int)petGetUpRemainTime.Value.TotalSeconds > 0;
	}

	public static TimeSpan? GetFoodRechargeRemainTime()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager == null)
		{
			return null;
		}
		DateTime? nextRechargeTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetNextRechargeTime(StaminaType.PetFeedKey);
		if (!nextRechargeTime.HasValue)
		{
			return null;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return nextRechargeTime - currentServerUTCTime;
	}

	public static string GetCreatureGradeColorLocalizationKey(CreatureGrade grade)
	{
		return grade switch
		{
			CreatureGrade.Rare => "PET_GRADE_COLOR_RARE_TEXT", 
			CreatureGrade.Hero => "PET_GRADE_COLOR_HERO_TEXT", 
			CreatureGrade.Ancient => "PET_GRADE_COLOR_ANCIENT_TEXT", 
			CreatureGrade.Legend => "PET_GRADE_COLOR_LEGEND_TEXT", 
			_ => "", 
		};
	}

	public static bool TryGetPetOptionLabelText(int optionIndex, int petStar, ref string optionName, ref string optionValue)
	{
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			return false;
		}
		return TryGetPetOptionLabelText(data, petStar, ref optionName, ref optionValue);
	}

	public static bool TryGetPetOptionLabelText(PetOptionData petOptionData, int petStar, ref string optionName, ref string optionValue)
	{
		if (petOptionData == null)
		{
			return false;
		}
		bool num = TryGetPetOptionNameLabelText(petOptionData, ref optionName);
		bool flag = TryGetPetOptionValueLabelText(petOptionData, petStar, ref optionValue);
		return num && flag;
	}

	public static bool TryGetPetOptionNameLabelText(int optionIndex, ref string optionName)
	{
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			return false;
		}
		return TryGetPetOptionNameLabelText(data, ref optionName);
	}

	public static bool TryGetPetOptionNameLabelText(PetOptionData petOptionData, ref string optionName)
	{
		if (petOptionData == null)
		{
			return false;
		}
		if (petOptionData.ConditionType == PetOptionConditionType.None)
		{
			optionName = LocalizationManager.GetString(petOptionData.OptionType);
		}
		else
		{
			string text = LocalizationManager.GetString(petOptionData.OptionType);
			optionName = LocalizationManager.GetStringDirect(petOptionData.ContentsOptionDescKey, text);
		}
		return true;
	}

	public static bool TryGetPetOptionValueLabelText(int optionIndex, int petStar, ref string optionValue)
	{
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			return false;
		}
		return TryGetPetOptionValueLabelText(data, petStar, ref optionValue);
	}

	public static bool TryGetPetOptionValueLabelText(PetOptionData petOptionData, int petStar, ref string optionValue)
	{
		if (petOptionData == null)
		{
			return false;
		}
		optionValue = StatData.GetStatDisplayValue(petOptionData.OptionType, petOptionData.GetOptionValue(petStar), showSign: true, isCommaString: true);
		return true;
	}

	public static bool TryGetPetBonusOptionLabelText(int bonusOptionIndex, int petStar, ref string optionName, ref string optionValue)
	{
		PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(bonusOptionIndex);
		if (data == null)
		{
			return false;
		}
		return TryGetPetBonusOptionLabelText(data, petStar, ref optionName, ref optionValue);
	}

	public static bool TryGetPetBonusOptionLabelText(PetBonusData petBonusData, int petStar, ref string optionName, ref string optionValue)
	{
		if (petBonusData == null)
		{
			return false;
		}
		bool num = TryGetPetBonusOptionNameLabelText(petBonusData, ref optionName);
		bool flag = TryGetPetBonusOptionValueLabelText(petBonusData, petStar, ref optionValue);
		return num && flag;
	}

	public static bool TryGetPetBonusOptionNameLabelText(int bonusOptionIndex, ref string optionName)
	{
		PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(bonusOptionIndex);
		if (data == null)
		{
			return false;
		}
		return TryGetPetBonusOptionNameLabelText(data, ref optionName);
	}

	public static bool TryGetPetBonusOptionNameLabelText(PetBonusData petBonusData, ref string optionName)
	{
		if (petBonusData == null)
		{
			return false;
		}
		optionName = LocalizationManager.GetStringDirect(petBonusData.BonusNameKey);
		return true;
	}

	public static bool TryGetPetBonusOptionValueLabelText(int bonusOptionIndex, int petStar, ref string optionValue)
	{
		PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(bonusOptionIndex);
		if (data == null)
		{
			return false;
		}
		return TryGetPetBonusOptionValueLabelText(data, petStar, ref optionValue);
	}

	public static bool TryGetPetBonusOptionValueLabelText(PetBonusData petBonusData, int petStar, ref string optionValue)
	{
		if (petBonusData == null)
		{
			return false;
		}
		optionValue = LocalizationManager.GetStringDirect(petBonusData.BonusDescKey, petBonusData.GetBonusValue(petStar));
		return true;
	}

	public static float GetIncubatorRemainTimePercent(PetIncubatorSlotInfo slotInfo)
	{
		if (slotInfo == null || slotInfo.MountingIndex == 0 || slotInfo.SetItemIndex == 0 || String_Extension_Method.IsNullOrEmpty(slotInfo.CompletedTime))
		{
			return -1f;
		}
		int eggTotalReqTimeSec = GetEggTotalReqTimeSec(slotInfo, slotInfo.SetItemIndex);
		if (eggTotalReqTimeSec == -1)
		{
			return -1f;
		}
		TimeSpan? incubatorRemainTime = GetIncubatorRemainTime(slotInfo);
		if (!incubatorRemainTime.HasValue)
		{
			return -1f;
		}
		double totalSeconds = incubatorRemainTime.Value.TotalSeconds;
		if (totalSeconds <= 0.0)
		{
			return 1f;
		}
		float num = (float)(eggTotalReqTimeSec - (int)totalSeconds) / (float)eggTotalReqTimeSec;
		if (num > 1f)
		{
			return 1f;
		}
		return num;
	}

	public static int GetEggBasicReqTimeSec(int eggIndex)
	{
		return GameRoot.Instance.PetEggDataContainer.GetData(eggIndex)?.ReqTime ?? (-1);
	}

	public static int GetEggTotalReqTimeSec(PetIncubatorSlotInfo slotInfo, int eggIndex)
	{
		if (GameRoot.Instance.PetEggDataContainer.GetData(eggIndex) == null)
		{
			return -1;
		}
		int eggBasicReqTimeSec = GetEggBasicReqTimeSec(eggIndex);
		if (eggBasicReqTimeSec <= 0)
		{
			return -1;
		}
		PetIncubatorData data = GameRoot.Instance.PetIncubatorDataContainer.GetData(slotInfo.MountingIndex);
		if (data == null)
		{
			return -1;
		}
		int timePer = data.TimePer;
		return (int)((float)eggBasicReqTimeSec * (1f - (float)timePer * 0.01f));
	}

	public static TimeSpan? GetIncubatorRemainTime(PetIncubatorSlotInfo petIncubatorSlotInfo)
	{
		if (petIncubatorSlotInfo == null || String_Extension_Method.IsNullOrEmpty(petIncubatorSlotInfo.CompletedTime))
		{
			return null;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		Vespa_String_Extension_Method2.TryParseNetworkTime2(petIncubatorSlotInfo.CompletedTime, out var o_dateTime);
		return o_dateTime - currentServerUTCTime;
	}

	public static TimeSpan? UpdateRemainTimeEggSupply(UILabel eggTimeLabel)
	{
		TimeSpan? remainPetEggSupplyTimeSpan = GlobalAutoBehaviour<AccountManager>.instance.PetManager.GetRemainPetEggSupplyTimeSpan();
		if (!remainPetEggSupplyTimeSpan.HasValue || !remainPetEggSupplyTimeSpan.HasValue)
		{
			UIHelper.SetLabelText(eggTimeLabel, LocalizationManager.GetStringDirect("RAIDER_EGG_RECEIVABLE"));
			return remainPetEggSupplyTimeSpan;
		}
		UIHelper.SetLabelText(eggTimeLabel, OrdealHelper.GetRemainTimeText(remainPetEggSupplyTimeSpan.Value));
		return remainPetEggSupplyTimeSpan;
	}

	public static void UpdatePetFoodState(UILabel petFoodStateLabel, long remainPetFoodCount, long maxPetFoodCount)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.PetManager.NotHavePet)
		{
			UIHelper.EnableGameObject(petFoodStateLabel, enable: false);
			return;
		}
		UIHelper.EnableGameObject(petFoodStateLabel, enable: true);
		string remainPetFoodCountStr = GetRemainPetFoodCountStr(remainPetFoodCount, maxPetFoodCount);
		if (remainPetFoodCount > 0)
		{
			UIHelper.SetLabelText(petFoodStateLabel, remainPetFoodCountStr);
			return;
		}
		UpdatePetFoodCount(ref remainPetFoodCount);
		TimeSpan? foodRechargeRemainTime = GetFoodRechargeRemainTime();
		if (!foodRechargeRemainTime.HasValue)
		{
			UIHelper.SetLabelText(petFoodStateLabel, remainPetFoodCountStr);
		}
		else if ((int)foodRechargeRemainTime.Value.TotalSeconds > 0)
		{
			UIHelper.SetLabelText(petFoodStateLabel, OrdealHelper.GetRemainTimeText(foodRechargeRemainTime.Value));
		}
		else
		{
			UIHelper.SetLabelText(petFoodStateLabel, OrdealHelper.GetRemainTimeText(TimeSpan.Zero));
		}
	}

	private static void UpdatePetFoodCount(ref long remainPetFoodcount)
	{
		if (remainPetFoodcount <= 0)
		{
			DateTime utcNow = DateTime.UtcNow;
			if (!_lastPetFoodCountUpdatedTime.HasValue)
			{
				_lastPetFoodCountUpdatedTime = utcNow;
			}
			if (!((utcNow - _lastPetFoodCountUpdatedTime.Value).TotalSeconds < (double)PetFoodCountUpdateDelayTime))
			{
				_lastPetFoodCountUpdatedTime = utcNow;
				remainPetFoodcount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
			}
		}
	}

	public static string GetRemainPetFoodCountStr(long remainPetFoodCount, long maxPetFoodCount)
	{
		return remainPetFoodCount + " / " + maxPetFoodCount;
	}

	public static bool CheckPossiblePetAwaken(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			return false;
		}
		bool flag = petInfo.CanAwaken();
		if (!flag)
		{
			return false;
		}
		PetAwakenData data = GameRoot.Instance.PetAwakenDataContainer.GetData(petInfo.PetIndex, petInfo.Star + 1);
		if (data == null)
		{
			return false;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode1);
		if (dataByCode == null)
		{
			return false;
		}
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(dataByCode.Index);
		bool flag2 = data.ItemCount1 <= itemCount;
		return flag && flag2;
	}

	public static int GetMaxExploreCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxExploreCnt");
	}

	public static int GetCurExploreCount(int exploreIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return 0;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.GetPetExploreInfoCountByExploreIndex(exploreIndex);
	}

	public static bool IsEnoughExploreReqFood(PetInfo petinfo, int exploreIndex)
	{
		if (petinfo == null)
		{
			return false;
		}
		int exploreReqFood = GetExploreReqFood(exploreIndex);
		int fullPoint = petinfo.FullPoint;
		if (exploreReqFood == -1 || exploreReqFood > fullPoint)
		{
			return false;
		}
		return true;
	}

	public static int GetExploreReqFood(int exploreIndex)
	{
		return GameRoot.Instance.PetAdventureDataContainer.GetData(exploreIndex)?.ReqFood ?? (-1);
	}

	public static bool CanExplorePetStatus(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			return false;
		}
		if (petInfo.Status != PetStatusType.None)
		{
			return false;
		}
		if (!CheckExplorePenaltyTime(petInfo.EndPenaltyTime))
		{
			return false;
		}
		return true;
	}

	public static bool CheckExplorePenaltyTime(string timeStr)
	{
		if (String_Extension_Method.IsNullOrEmpty(timeStr))
		{
			return true;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(timeStr, out var o_dateTime))
		{
			return true;
		}
		if (GetCurServerTimeUTCTime().CompareTo(o_dateTime) >= 0)
		{
			return true;
		}
		return false;
	}

	public static DateTime GetCurServerTimeUTCTime()
	{
		if (!AccountManager.IsLogin())
		{
			return DateTime.MinValue;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
	}

	public static bool CanExplorePetType(PetInfo petInfo)
	{
		if (petInfo == null)
		{
			return false;
		}
		CreatureData creatureData = petInfo.CreatureData;
		if (creatureData == null)
		{
			return false;
		}
		if (creatureData.IsLinePet)
		{
			return false;
		}
		return true;
	}

	public static bool CanExplorePetCount(int petCount, int exploreIndex)
	{
		int maxExplorePetCount = GetMaxExplorePetCount(exploreIndex);
		if (maxExplorePetCount == -1 || maxExplorePetCount <= petCount)
		{
			return false;
		}
		return true;
	}

	public static int GetMaxExplorePetCount(int exploreIndex)
	{
		return GameRoot.Instance.PetAdventureDataContainer.GetData(exploreIndex)?.MaxPetCount ?? (-1);
	}

	public static bool EnoughExplorePetCount(int curPetCount, int exploreIndex)
	{
		int maxExplorePetCount = GetMaxExplorePetCount(exploreIndex);
		if (maxExplorePetCount == -1 || maxExplorePetCount != curPetCount)
		{
			return false;
		}
		return true;
	}

	public static EPetExploreInfoStatus GetPetExploreInfoStatus(int exploreIndex, int deckIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return EPetExploreInfoStatus.Error;
		}
		return GetPetExploreInfoStatus(GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.Get(exploreIndex, deckIndex));
	}

	public static EPetExploreInfoStatus GetPetExploreInfoStatus(PetExploreInfo exploreInfo)
	{
		if (exploreInfo == null)
		{
			return EPetExploreInfoStatus.Start;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(exploreInfo.EndTime, out var o_dateTime))
		{
			return EPetExploreInfoStatus.Cancel;
		}
		if (GetCurServerTimeUTCTime().CompareTo(o_dateTime) < 0)
		{
			return EPetExploreInfoStatus.Cancel;
		}
		return EPetExploreInfoStatus.End;
	}

	public static int GetPetExploreRemainSec(int petIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return 0;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.TryGetPetExploreEndTime(petIndex, out var dateTime))
		{
			return 0;
		}
		DateTime curServerTimeUTCTime = GetCurServerTimeUTCTime();
		int num = (int)(dateTime - curServerTimeUTCTime).TotalSeconds;
		if (num <= 0)
		{
			return 0;
		}
		return num;
	}

	public static bool IsExploring(int petIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.AlreadyExploreMemer(petIndex);
	}

	public static bool AnyExploreEnd()
	{
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		DateTime cachedExploreMinEndTime = GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.GetCachedExploreMinEndTime();
		if (GetCurServerTimeUTCTime() >= cachedExploreMinEndTime)
		{
			return true;
		}
		return false;
	}

	public static TimeSpan? GetExploreEndTimeSpan(int exploreIndex, int deckIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		PetExploreInfo petExploreInfo = GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.Get(exploreIndex, deckIndex);
		if (petExploreInfo == null)
		{
			return null;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(petExploreInfo.EndTime, out var o_dateTime))
		{
			return null;
		}
		DateTime curServerTimeUTCTime = GetCurServerTimeUTCTime();
		TimeSpan value = o_dateTime - curServerTimeUTCTime;
		if (value.TotalSeconds < 0.0)
		{
			return null;
		}
		return value;
	}

	public static DateTime GetExploreEndTime(int exploreIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return DateTime.MaxValue;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PetExploreManager.GetExploreMinEndTime(exploreIndex);
	}

	public static void RequestPetExploreStart(int exploreIndex, int deckIndex, int[] petIndices, Action<NShared.PetStartExplore.Response> responseCallback)
	{
		NShared.PetStartExplore.Request request = new NShared.PetStartExplore.Request
		{
			ExploreIndex = exploreIndex,
			DeckIndex = deckIndex,
			PetIndices = petIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, responseCallback);
	}

	public static void RequestPetExploreCancel(int exploreIndex, int deckIndex, Action<NShared.PetCancelExplore.Response> response)
	{
		NShared.PetCancelExplore.Request request = new NShared.PetCancelExplore.Request
		{
			ExploreIndex = exploreIndex,
			DeckIndex = deckIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, response);
	}

	public static void RequestPetExploreEnd(int exploreIndex, int deckIndex, Action<NShared.PetEndExplore.Response> response)
	{
		NShared.PetEndExplore.Request request = new NShared.PetEndExplore.Request
		{
			ExploreIndex = exploreIndex,
			DeckIndex = deckIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, response);
	}
}
