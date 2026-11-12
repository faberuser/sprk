using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class EnterPunishScenarioList : IEvent
{
	public int GroupIndex { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return ArgParser.GetValueInt(argList, 0) == GroupIndex;
	}
}
