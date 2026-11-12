using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class PunishmentClearDungeonCount : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 4)
		{
			return true;
		}
		int valueInt = ArgParser.GetValueInt(values, 1);
		int valueInt2 = ArgParser.GetValueInt(values, 2);
		CheckType checkType = CheckType.None;
		if (valueInt <= 0 || valueInt2 <= 0 || !SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[3], ref checkType))
		{
			return true;
		}
		List<PunishmentRaidInfo> punishmentRaidInfoList = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfoList(valueInt);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(punishmentRaidInfoList))
		{
			return true;
		}
		int num = 0;
		for (int i = 0; i < punishmentRaidInfoList.Count; i++)
		{
			PunishmentRaidInfo punishmentRaidInfo = punishmentRaidInfoList[i];
			if (punishmentRaidInfo != null && punishmentRaidInfo.ClearCheckIndex != 0 && !String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime))
			{
				num++;
			}
		}
		return Condition.CheckCompareResult(checkType, valueInt2, num);
	}
}
