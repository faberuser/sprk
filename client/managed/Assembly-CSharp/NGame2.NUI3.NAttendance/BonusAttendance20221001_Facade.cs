using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetAttendanceReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NAttendance;

public class BonusAttendance20221001_Facade
{
	public enum ERewardState
	{
		None,
		Already_Received,
		CanReceive,
		Locked
	}

	public class InitWrapper_when_EnterLobby : NGame2.NScene.NLobbyScenePrerequisites.IRequestWrapper
	{
		public static bool ForceTimeout;

		public const float Timeout = 20f;

		private float _queryTime;

		public bool IsDoneOrTimeout
		{
			get
			{
				if (!IsDone)
				{
					return IsTimeout;
				}
				return true;
			}
		}

		public bool IsDone { get; private set; }

		public bool IsTimeout
		{
			get
			{
				if (IsDone)
				{
					return false;
				}
				return Time.realtimeSinceStartup - _queryTime > 20f;
			}
		}

		public void Init()
		{
			try
			{
				OpenInternal();
			}
			catch (Exception i_object)
			{
				Debug.LogError("TreasureHouseInitWrapper.instance.Open exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
				IsDone = true;
			}
		}

		private void OpenInternal()
		{
			_queryTime = Time.realtimeSinceStartup;
			if (ForceTimeout)
			{
				IsDone = true;
				return;
			}
			if (!DoesNeed_Request_for_NextAttendanceDay(isMain: true))
			{
				IsDone = true;
				return;
			}
			Enable_ReservePopup.Value = true;
			Request_for_NextAttendanceDay(delegate
			{
				IsDone = true;
			});
		}
	}

	private static AttendanceKeywordGroup _myGroup = new AttendanceKeywordGroup(EAttendanceKeyword.BonusAttendance20221001_1);

	public static LocalAccountBool Enable_ReservePopup = new LocalAccountBool(i_default: false, "BonusAttendance20221001_Facade_Enable_ReservePopup");

	public static DateTime _lastLocalTime_of_Request_for_NextAttendanceDay = DateTime.MinValue;

	public static bool CanDo_ReservePopup()
	{
		if (!Enable_ReservePopup.Value)
		{
			return false;
		}
		return CanReceiveReward_Any();
	}

	public static bool CanReceiveReward_Any()
	{
		if (IsUnlocked_by_LuaPoint(isMain: true) && CanReceiveReward_by_CompletedDay(isMain: true))
		{
			return true;
		}
		if (IsUnlocked_by_LuaPoint(isMain: false) && CanReceiveReward_by_CompletedDay(isMain: false))
		{
			return true;
		}
		return false;
	}

	public static bool IsInProgress()
	{
		return true;
	}

	public static bool IsUnlocked_by_LuaPoint(bool isMain)
	{
		return GetMonthlyAcquiredLuaPoint() >= GetRequire_LuaPoint_from_AttendanceInfoData(isMain);
	}

	public static ERewardState GetRewardState_by_Day(bool isMain, int day)
	{
		AttendanceInfo attendanceInfo = (isMain ? _myGroup.AttendanceInfo : _myGroup.SubAttendanceInfo);
		if (attendanceInfo == null)
		{
			return ERewardState.None;
		}
		if (day <= attendanceInfo.LastRewardedDay)
		{
			return ERewardState.Already_Received;
		}
		if (day <= attendanceInfo.CompletedDay)
		{
			return ERewardState.CanReceive;
		}
		return ERewardState.Locked;
	}

	public static bool CanReceiveReward_by_CompletedDay(bool isMain)
	{
		AttendanceInfo attendanceInfo = (isMain ? _myGroup.AttendanceInfo : _myGroup.SubAttendanceInfo);
		if (attendanceInfo != null && attendanceInfo.LastRewardedDay < attendanceInfo.CompletedDay)
		{
			return true;
		}
		return false;
	}

	public static void GetAttendancePeriod(out DateTime beginDate, out DateTime endDate)
	{
		beginDate = DateTime.MinValue;
		endDate = DateTime.MinValue;
		AttendanceInfo attendanceInfo = _myGroup.AttendanceInfo;
		if (attendanceInfo != null && attendanceInfo.TryGetCompletedTime_ServerLocal(out var completedTime_Local))
		{
			int day = DateTime.DaysInMonth(completedTime_Local.Year, completedTime_Local.Month);
			beginDate = new DateTime(completedTime_Local.Year, completedTime_Local.Month, 1);
			endDate = new DateTime(completedTime_Local.Year, completedTime_Local.Month, day);
		}
	}

	public static int GetAttendanceRewardEndDay()
	{
		return AttendanceKeywordGroup.Helper.GetAttendanceRewardEndDay(_myGroup.KeywordMain);
	}

	public static int GetRequire_LuaPoint_from_AttendanceInfoData(bool isMain)
	{
		return GetLuaPoint_from_AttendanceInfoData(isMain ? _myGroup.AttendanceInfoData : _myGroup.SubAttendanceInfoData);
	}

	private static int GetLuaPoint_from_AttendanceInfoData(AttendanceInfoData attendanceInfoData)
	{
		if (attendanceInfoData == null)
		{
			return 0;
		}
		if (attendanceInfoData.ConditionType1 != QuestOpenConditionType.OverKey)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.COUNT(attendanceInfoData.ConditionValue1) < 2)
		{
			return 0;
		}
		_ = attendanceInfoData.ConditionValue1[0];
		return Vespa_String_Extension_Method.TOINT(attendanceInfoData.ConditionValue1[1], 0);
	}

	public static int GetMonthlyAcquiredLuaPoint()
	{
		if (!AccountManager.IsLogined())
		{
			return 0;
		}
		PlayerAnyMisc playerAnyMisc = GlobalAutoBehaviour<AccountManager>.instance.UserAnyMiscManager.Get("MonthlyAcquiredLuaPoint");
		if (playerAnyMisc == null)
		{
			return 0;
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(playerAnyMisc.EndTime, out var o_dateTime) && GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() >= o_dateTime)
		{
			return 0;
		}
		return Vespa_String_Extension_Method.TOINT(playerAnyMisc.MiscValue, 0);
	}

	public static AttendanceReward GetAttendanceReward(bool isMain, int day)
	{
		IList<AttendanceReward> attendanceRewards = AttendanceKeywordGroup.Helper.GetAttendanceRewards(isMain ? _myGroup.KeywordMain : _myGroup.KeywordSub, day);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(attendanceRewards))
		{
			return null;
		}
		return attendanceRewards[0];
	}

	public static bool DoesNeed_Request_for_NextAttendanceDay(bool isMain)
	{
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if ((GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() - _lastLocalTime_of_Request_for_NextAttendanceDay).TotalMinutes < 5.0)
		{
			return false;
		}
		AttendanceData attendanceData = (isMain ? _myGroup.AttendanceData : _myGroup.SubAttendanceData);
		if (attendanceData == null)
		{
			return false;
		}
		DateTime completedTime_Local = DateTime.MinValue;
		AttendanceInfo attendanceInfo = (isMain ? _myGroup.AttendanceInfo : _myGroup.SubAttendanceInfo);
		if (attendanceInfo != null && !attendanceInfo.TryGetCompletedTime_ServerLocal(out completedTime_Local))
		{
			completedTime_Local = DateTime.MinValue;
		}
		DateTime date = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime().Date;
		DateTime date2 = completedTime_Local.Date;
		if (date == date2)
		{
			return false;
		}
		if (date.Month != date2.Month)
		{
			return true;
		}
		int attendanceRewardEndDay = AttendanceKeywordGroup.Helper.GetAttendanceRewardEndDay(attendanceData.Keyword);
		if (attendanceInfo != null && attendanceInfo.CompletedDay >= attendanceRewardEndDay)
		{
			return false;
		}
		return true;
	}

	public static void Request_for_NextAttendanceDay(Action onComplete)
	{
		_lastLocalTime_of_Request_for_NextAttendanceDay = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		Request_GetAttendanceReward(_myGroup.IndexMain, delegate
		{
			Request_GetAttendanceReward(_myGroup.IndexSub, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
			});
		});
	}

	private static void Request_GetAttendanceReward(int attendanceIndex, Action onComplete)
	{
		NShared.GetAttendanceReward.Request request = new NShared.GetAttendanceReward.Request
		{
			AttendanceIndex = attendanceIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetAttendanceReward.Response response)
		{
			OnResponse_GetAttendanceReward(request, response, onComplete);
		});
	}

	private static void OnResponse_GetAttendanceReward(NShared.GetAttendanceReward.Request request, NShared.GetAttendanceReward.Response response, Action onComplete)
	{
		if (response == null)
		{
			MessagePopup.OpenError(BaseResultType.Fail);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else if (response.Result != ResultType.Success)
		{
			MessagePopup.OpenError(response.Result);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(response.AttendanceInfos);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	private static bool IsEmptyDbReward(DbRewardResult dbReward)
	{
		if (dbReward == null)
		{
			return true;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dbReward.CurrencyResults))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dbReward.ItemResults))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dbReward.EquipItemResults))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dbReward.HeroInfos))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dbReward.ExpResultInfos))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(dbReward.StaminaResultInfos))
		{
			return false;
		}
		return true;
	}
}
