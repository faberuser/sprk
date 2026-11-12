using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OnClickStartBattle : BaseNodeEvent
{
	public DungeonDifficultyType DifficultyType { get; set; }

	public bool IsScenario { get; set; }

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		if (!base.Check(argList, out tableChapterIndex, out tableDungeonIndex))
		{
			return false;
		}
		if (ArgParser.HasValue(argList, 2) && ArgParser.GetValueEnum(argList, 2, DungeonDifficultyType.Easy) != DifficultyType)
		{
			return false;
		}
		if (ArgParser.HasValue(argList, 3) && ArgParser.GetValueBool(argList, 3) != IsScenario)
		{
			return false;
		}
		return true;
	}
}
