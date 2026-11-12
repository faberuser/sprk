using System.Collections.Generic;

namespace NShared.NEvent;

public interface IEvent
{
	bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex);
}
