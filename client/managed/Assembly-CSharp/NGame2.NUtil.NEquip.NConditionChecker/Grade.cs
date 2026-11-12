using NShared;
using NShared.NUtil;

namespace NGame2.NUtil.NEquip.NConditionChecker;

public class Grade : ConditionChecker.ConditionBase
{
	public override bool Check(EquipItemInfo equipItemInfo, string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		if (equipItemInfo.ItemData == null)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result))
		{
			return false;
		}
		CheckType valueEnum = ArgParser.GetValueEnum(values, 2, CheckType.None);
		if (valueEnum == CheckType.None)
		{
			return false;
		}
		byte grade = equipItemInfo.ItemData.Grade;
		return Condition.CheckCompareResult(valueEnum, result, grade);
	}
}
