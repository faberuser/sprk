using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearChallengeRaid : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		int count = 0;
		int minutes = 0;
		ConditionChecker instance = SingletonAuto<ConditionChecker>.instance;
		if (instance == null)
		{
			return false;
		}
		if (!instance.CheckGetValues(values, ref count, ref minutes))
		{
			return false;
		}
		CheckType checkType = CheckType.None;
		CheckType checkType2 = CheckType.None;
		if (!SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[2], ref checkType) || !SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[4], ref checkType2))
		{
			return false;
		}
		if (!instance.CheckCount(StaminaType.None, count, checkType) || !CheckChallengeRaidProgressTime(minutes, checkType2))
		{
			return false;
		}
		return true;
	}

	private bool CheckChallengeRaidProgressTime(int minutes, CheckType minutesCheckType)
	{
		int num = -1;
		long num2 = num;
		foreach (ChallengeRaidData allDatum in GameRoot.Instance.ChallengeRaidDataContainer.GetAllData())
		{
			if (allDatum != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(GlobalAutoBehaviour<AccountManager>.instance.RaidManager.Get(allDatum.Index).CompletedTime, out var o_dateTime))
			{
				TimeSpan timeSpan = Common.GetUTCNow() - o_dateTime;
				long num3 = num2;
				int num4 = (int)timeSpan.TotalMinutes;
				if (num3 < num4)
				{
					num2 = (long)timeSpan.TotalMinutes;
				}
			}
		}
		return SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, num2);
	}
}
