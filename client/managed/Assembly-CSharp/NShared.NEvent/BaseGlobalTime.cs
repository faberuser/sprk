using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class BaseGlobalTime : IEvent
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int WaveIndex { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return false;
	}
}
