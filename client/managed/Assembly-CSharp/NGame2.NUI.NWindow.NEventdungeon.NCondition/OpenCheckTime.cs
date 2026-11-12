using System;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEventdungeon.NCondition;

public class OpenCheckTime : IEventDungeonCondition
{
	bool IEventDungeonCondition.Check(EventDungeonData dungeonData, string[] values, out string labelText)
	{
		labelText = "";
		if (dungeonData == null)
		{
			return false;
		}
		return Check(values, out labelText);
	}

	private bool Check(string[] values, out string labelText)
	{
		labelText = "";
		if (values == null || values.Length != 3)
		{
			return false;
		}
		string valueString = ArgParser.GetValueString(values, 0);
		string valueString2 = ArgParser.GetValueString(values, 1);
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(valueString, out var o_dateTime) || !Vespa_String_Extension_Method2.TryParseNetworkTime2(valueString2, out var o_dateTime2))
		{
			return false;
		}
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		if (DateTime.Compare(currentServerLocalTime, o_dateTime2) > 0)
		{
			return false;
		}
		if (DateTime.Compare(currentServerLocalTime, o_dateTime) < 0)
		{
			string valueString3 = ArgParser.GetValueString(values, 2);
			if (!String_Extension_Method.IsNullOrEmpty(valueString3))
			{
				DateTime dateTime = new DateTime(o_dateTime.Year, o_dateTime.Month, o_dateTime.Day);
				DateTime dateTime2 = new DateTime(currentServerLocalTime.Year, currentServerLocalTime.Month, currentServerLocalTime.Day);
				int num = (int)(dateTime - dateTime2).TotalDays;
				_ = o_dateTime - currentServerLocalTime;
				labelText = LocalizationManager.GetStringDirect(valueString3, num);
			}
			return false;
		}
		return true;
	}
}
