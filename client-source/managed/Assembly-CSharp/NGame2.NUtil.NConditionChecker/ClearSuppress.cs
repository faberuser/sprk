using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearSuppress : ConditionChecker.ConditionBase
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
		if (!instance.CheckCount(StaminaType.GuildSuppressKey, count, checkType) || !CheckSuppressProgressTime(minutes, checkType2))
		{
			return false;
		}
		return true;
	}

	private bool CheckSuppressProgressTime(int minutes, CheckType minutesCheckType)
	{
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Conquest);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allDataOfBattleType))
		{
			return false;
		}
		int num = -1;
		long num2 = num;
		foreach (CampaignDungeonData item in allDataOfBattleType)
		{
			ChapterDungeonInfo chapterDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(item.ChapterIndex, item.DungeonIndex);
			if (chapterDungeonInfo != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(chapterDungeonInfo.CompletedTime, out var o_dateTime))
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
		long num5 = num2;
		int num6 = -1;
		if (num5 == num6)
		{
			return true;
		}
		return SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, num2);
	}
}
