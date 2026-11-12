using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI3.NAttendance;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NContentsInfo;

public class AttendanceFacade
{
	public static bool CanReceiveReward_Any()
	{
		if (BonusAttendance20221001_Facade.CanReceiveReward_Any())
		{
			return true;
		}
		return false;
	}

	public static bool CanReceiveReward_Any(AttendanceKeywordGroup attendanceGroup)
	{
		if (attendanceGroup.KeywordMain == EAttendanceKeyword.BonusAttendance20221001_1)
		{
			return BonusAttendance20221001_Facade.CanReceiveReward_Any();
		}
		return false;
	}

	public static List<AttendanceKeywordGroup> GetGroupList_ReservePopup_on_EnterLobby()
	{
		List<AttendanceKeywordGroup> list = new List<AttendanceKeywordGroup>();
		foreach (AttendanceKeywordGroup item in SingletonAuto<AttendanceDataContainer>.instance.GetGroupList_SortedByShowIndex())
		{
			if (item.KeywordMain == EAttendanceKeyword.BonusAttendance20221001_1)
			{
				if (!BonusAttendance20221001_Facade.CanDo_ReservePopup())
				{
					continue;
				}
			}
			else if (!item.CanDo_ReservePopup())
			{
				continue;
			}
			list.Add(item);
		}
		return list;
	}

	public static List<AttendanceKeywordGroup> GetGroupList_InProgress()
	{
		List<AttendanceKeywordGroup> list = new List<AttendanceKeywordGroup>();
		foreach (AttendanceKeywordGroup item in SingletonAuto<AttendanceDataContainer>.instance.GetGroupList_SortedByShowIndex())
		{
			if (item.KeywordMain == EAttendanceKeyword.BonusAttendance20221001_1)
			{
				if (!BonusAttendance20221001_Facade.IsInProgress())
				{
					continue;
				}
			}
			else if (!item.IsInProgress_v1())
			{
				continue;
			}
			list.Add(item);
		}
		return list;
	}

	public static AttendanceKeywordGroup GetAttendance_DefaultDailyAttendance()
	{
		AttendanceKeywordGroup result = new AttendanceKeywordGroup(EAttendanceKeyword.ReturnUser);
		if (result.IsValid_AttendanceInfo())
		{
			return result;
		}
		result = new AttendanceKeywordGroup(EAttendanceKeyword.BeginUser201110);
		if (result.IsValid_AttendanceInfo())
		{
			return result;
		}
		result = new AttendanceKeywordGroup(EAttendanceKeyword.Common);
		if (result.IsValid_AttendanceData())
		{
			return result;
		}
		return default(AttendanceKeywordGroup);
	}

	public static string GetLocalizeText_DefaultDailyAttendance(EAttendanceKeyword keyword)
	{
		return keyword switch
		{
			EAttendanceKeyword.BeginUser201110 => LocalizationManager.GetStringDirect("MENU_ALL_ATTENDANCE_NEW"), 
			EAttendanceKeyword.ReturnUser => LocalizationManager.GetStringDirect("MENU_ALL_ATTENDANCE_RETURN"), 
			_ => LocalizationManager.GetStringDirect("MENU_ALL_ATTENDANCE_NORMAL"), 
		};
	}

	public static bool AlreadyReceived_Reward_Attendance_BeginUser()
	{
		if (!AccountManager.IsLogined())
		{
			return true;
		}
		EAttendanceKeyword[] array = new EAttendanceKeyword[1] { EAttendanceKeyword.BeginUser201110 };
		for (int i = 0; i < array.Length; i++)
		{
			AttendanceKeywordGroup attendanceKeywordGroup = AttendanceKeywordGroup.Helper.CreateByAnyKeyword(array[i]);
			if (attendanceKeywordGroup.IsValid_AttendanceInfo())
			{
				int rewardableDays = attendanceKeywordGroup.RewardableDays;
				int completedDay = attendanceKeywordGroup.CompletedDay;
				if (rewardableDays <= 0 || rewardableDays != completedDay)
				{
					return false;
				}
			}
		}
		return true;
	}
}
