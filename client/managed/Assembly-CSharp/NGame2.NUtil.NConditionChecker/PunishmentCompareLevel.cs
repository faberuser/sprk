using NGame2.NAccount;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class PunishmentCompareLevel : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 4)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 1, -1);
		int valueInt2 = ArgParser.GetValueInt(values, 2, -1);
		CheckType checkType = CheckType.None;
		if (valueInt == -1 || valueInt2 == -1 || !SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[3], ref checkType))
		{
			return false;
		}
		int testValue = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(valueInt2, EPunishmentDungeonType.Boss)?.OpenLevel ?? 1;
		int valueInt3 = ArgParser.GetValueInt(values, 4, -1);
		if (valueInt3 != -1)
		{
			testValue = valueInt3;
		}
		return Condition.CheckCompareResult(checkType, valueInt, testValue);
	}
}
