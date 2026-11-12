using System.Collections.Generic;
using NShared.NUtil;

namespace NShared.NEvent;

[JsonSerializable]
public class BossHpPercentage : BaseNodeEvent
{
	public int Percentage { get; set; }

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		if (!base.Check(argList, out tableChapterIndex, out tableDungeonIndex))
		{
			return false;
		}
		CheckType valueEnum = ArgParser.GetValueEnum<string, CheckType>(argList, 2, CheckType.None);
		int valueInt = ArgParser.GetValueInt(argList, 3);
		return Condition.CheckCompareResult(valueEnum, valueInt, Percentage);
	}
}
