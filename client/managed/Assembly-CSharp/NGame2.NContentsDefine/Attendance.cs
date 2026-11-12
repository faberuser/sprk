using System;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NContentsDefine;

public class Attendance
{
	public static bool TryGetRemainTime(AttendanceInfo attendanceInfo, out TimeSpan remainTimeSpan)
	{
		if (attendanceInfo == null)
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		if (!attendanceInfo.TryGetEndTime_ServerLocal(out var endTime_Local))
		{
			remainTimeSpan = default(TimeSpan);
			return false;
		}
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		remainTimeSpan = endTime_Local - currentServerLocalTime;
		return true;
	}

	public static string GetRemainTimeString(AttendanceInfo attendanceInfo)
	{
		if (!TryGetRemainTime(attendanceInfo, out var remainTimeSpan))
		{
			return string.Empty;
		}
		if (remainTimeSpan > TimeSpan.Zero)
		{
			return SingletonAuto<MissionStatus>.instance.GetRemainTimeString_Long(remainTimeSpan);
		}
		return LocalizationManager.GetStringDirect("UI_EVENT_TIMEOUT");
	}
}
