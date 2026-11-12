using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

public static class __Extension_Method_NShared_AttendanceInfo__
{
	public static bool TryGetCompletedTime_ServerLocal(this AttendanceInfo This, out DateTime completedTime_Local)
	{
		if (This == null)
		{
			completedTime_Local = DateTime.Now;
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(This.CompletedTime, out var o_dateTime))
		{
			completedTime_Local = DateTime.Now;
			return false;
		}
		if (!AccountManager.IsLogined())
		{
			completedTime_Local = DateTime.Now;
			return false;
		}
		completedTime_Local = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
		return true;
	}

	public static bool TryGetEndTime_ServerLocal(this AttendanceInfo This, out DateTime endTime_Local)
	{
		if (This == null)
		{
			endTime_Local = DateTime.Now;
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(This.EndTime, out var o_dateTime))
		{
			endTime_Local = DateTime.Now;
			return false;
		}
		if (!AccountManager.IsLogined())
		{
			endTime_Local = DateTime.Now;
			return false;
		}
		endTime_Local = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(o_dateTime);
		return true;
	}
}
