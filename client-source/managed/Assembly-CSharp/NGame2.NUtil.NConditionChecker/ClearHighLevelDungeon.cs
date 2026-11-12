using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearHighLevelDungeon : ConditionChecker.ConditionBase
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
		if (!instance.CheckCount(StaminaType.HideoutKey, count, checkType) || !CheckHighLevelDungeonProgressTime(minutes, checkType2))
		{
			return false;
		}
		return true;
	}

	private bool CheckHighLevelDungeonProgressTime(int minutes, CheckType minutesCheckType)
	{
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Hideout);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allDataOfBattleType))
		{
			return false;
		}
		int num = 0;
		int num2 = -1;
		long num3 = num2;
		foreach (CampaignDungeonData item in allDataOfBattleType)
		{
			if (item == null || num == item.ChapterIndex)
			{
				continue;
			}
			num = item.ChapterIndex;
			HideoutDungeonInfo hideoutDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.Get(num);
			if (hideoutDungeonInfo != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(hideoutDungeonInfo.CompletedTime, out var o_dateTime))
			{
				TimeSpan timeSpan = Common.GetUTCNow() - o_dateTime;
				long num4 = num3;
				int num5 = (int)timeSpan.TotalMinutes;
				if (num4 < num5)
				{
					num3 = (long)timeSpan.TotalMinutes;
				}
			}
		}
		long num6 = num3;
		int num7 = -1;
		if (num6 == num7)
		{
			return true;
		}
		return SingletonAuto<ConditionChecker>.instance.CheckCheckType(minutesCheckType, minutes, num3);
	}
}
