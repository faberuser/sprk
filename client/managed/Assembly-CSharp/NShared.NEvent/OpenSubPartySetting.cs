using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OpenSubPartySetting : IEvent
{
	public BattleType BattleType { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return ArgParser.GetValueEnum(argList, 0, BattleType.Campaign) == BattleType;
	}
}
