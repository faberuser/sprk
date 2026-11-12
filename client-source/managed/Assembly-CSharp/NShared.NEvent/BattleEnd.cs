using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class BattleEnd : BaseNodeEvent
{
	public TeamType Winner { get; set; }

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		if (!base.Check(argList, out tableChapterIndex, out tableDungeonIndex))
		{
			return false;
		}
		return ArgParser.GetValueEnum(argList, 2, TeamType.Left) == Winner;
	}
}
