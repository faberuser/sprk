using System.Collections.Generic;

namespace NShared.NEvent;

public class OpenOrdealArenaUI : IEvent
{
	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return true;
	}
}
