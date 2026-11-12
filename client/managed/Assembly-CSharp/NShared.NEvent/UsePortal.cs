using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class UsePortal : IEvent
{
	public bool MoveToLast;

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return false;
	}
}
