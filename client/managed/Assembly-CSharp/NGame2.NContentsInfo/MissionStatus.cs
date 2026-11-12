using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NShared.NContentsDefine;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NContentsInfo;

public class MissionStatus : SingletonAuto<MissionStatus>
{
	public void OpenTimeoutPopup(Action onOk)
	{
		string title = LocalizationManager.GetString(Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("UI_EVENT_TIMEOUT");
		PayShopMessagePopup.Open(title, stringDirect, onOk);
	}

	public bool IsDisable_NotiPopup(int mainCategoryIndex, int subCategoryIndex)
	{
		return ContentsDefine.ReturnUserJumping.GetExtraWidget_in_MissionNodeWindowV3(mainCategoryIndex, subCategoryIndex)?.Disable_NotiPopup ?? false;
	}

	public bool IsMissionTimeout(MissionCategoryData data)
	{
		if (data == null)
		{
			return false;
		}
		TimeSpan remainTimeSpan;
		switch (data.MissionType)
		{
		case EMissionType.Special:
		case EMissionType.Special2:
			if (TryGetRemainTime_from_SpecialMission(data, out remainTimeSpan))
			{
				return remainTimeSpan <= TimeSpan.Zero;
			}
			break;
		case EMissionType.Special3:
			if (TryGetRemainTime_from_PlayerAnyMisc(data, out remainTimeSpan))
			{
				return remainTimeSpan <= TimeSpan.Zero;
			}
			break;
		}
		return false;
	}

	public string GetRemainTimeString_Long(TimeSpan remainTimeSpan)
	{
		if (remainTimeSpan > TimeSpan.Zero)
		{
			string remainTimeString_Short = GetRemainTimeString_Short(remainTimeSpan);
			return LocalizationManager.GetStringDirect("CHALLEGNERAID_LEFT_TIME", remainTimeString_Short);
		}
		return LocalizationManager.GetStringDirect("EVENT_MISSION_NOTICE_END");
	}

	public string GetRemainTimeString_Short(TimeSpan remainTimeSpan)
	{
		if (remainTimeSpan.Days > 0)
		{
			return LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_DAY_HOUR", remainTimeSpan.Days, remainTimeSpan.Hours);
		}
		if (remainTimeSpan.Hours > 0)
		{
			return LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_HOUR_MINUTE", remainTimeSpan.Hours, remainTimeSpan.Minutes);
		}
		if (remainTimeSpan > TimeSpan.Zero)
		{
			return LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_MINUTE_SECOND", remainTimeSpan.Minutes, remainTimeSpan.Seconds);
		}
		return LocalizationManager.GetStringDirect("ADVENTURE_NOTICE_STAMINA_END");
	}

	public string GetRemainTimeString_Long(MissionCategoryData missionCategoryData)
	{
		if (!TryGetRemainTime(missionCategoryData, out var remainTimeSpan))
		{
			return string.Empty;
		}
		return GetRemainTimeString_Long(remainTimeSpan);
	}

	public string GetRemainTimeString_Short(MissionCategoryData missionCategoryData)
	{
		if (!TryGetRemainTime(missionCategoryData, out var remainTimeSpan))
		{
			return string.Empty;
		}
		return GetRemainTimeString_Short(remainTimeSpan);
	}

	public bool TryGetRemainTime(MissionCategoryData missionCategoryData, out TimeSpan remainTimeSpan)
	{
		if (missionCategoryData == null)
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		if (TryGetRemainTime_from_PlayerAnyMisc(missionCategoryData, out remainTimeSpan))
		{
			return true;
		}
		if (TryGetRemainTime_from_SpecialMission(missionCategoryData, out remainTimeSpan))
		{
			return true;
		}
		remainTimeSpan = default(TimeSpan);
		return false;
	}

	private bool TryGetRemainTime_from_PlayerAnyMisc(MissionCategoryData missionCategoryData, out TimeSpan remainTimeSpan)
	{
		ReturnUserJumping.ExtraWidget_in_MissionNodeWindowV3 extraWidget_in_MissionNodeWindowV = ContentsDefine.ReturnUserJumping.GetExtraWidget_in_MissionNodeWindowV3(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex);
		if (extraWidget_in_MissionNodeWindowV == null)
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		string linked_PlayerAnyMisc = extraWidget_in_MissionNodeWindowV.Linked_PlayerAnyMisc;
		if (String_Extension_Method.IsNullOrEmpty(linked_PlayerAnyMisc))
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		PlayerAnyMisc playerAnyMisc = GlobalAutoBehaviour<AccountManager>.instance.UserAnyMiscManager.Get(linked_PlayerAnyMisc);
		if (playerAnyMisc == null)
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(playerAnyMisc.EndTime, out var o_dateTime))
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		remainTimeSpan = o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return true;
	}

	private bool TryGetRemainTime_from_SpecialMission(MissionCategoryData missionCategoryData, out TimeSpan remainTimeSpan)
	{
		if (missionCategoryData.MissionType != EMissionType.Special && missionCategoryData.MissionType != EMissionType.Special2)
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(missionCategoryData.OpenConditionValue))
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
		{
			remainTimeSpan = new TimeSpan(0, 0, -1);
			return true;
		}
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.None, missionCategoryData.OpenConditionValue[0]);
		if (effectiveEvent == null)
		{
			remainTimeSpan = new TimeSpan(0, 0, -1);
			return true;
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(effectiveEvent.EndTime, out var o_dateTime))
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			remainTimeSpan = o_dateTime - currentServerUTCTime;
			return true;
		}
		remainTimeSpan = new TimeSpan(0, 0, -1);
		return true;
	}
}
