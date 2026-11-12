using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class FocusingHeroParty : IEvent
{
	public int ChapterIndex;

	public int DungeonIndex;

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = ArgParser.GetValueInt(argList, 0);
		tableDungeonIndex = ArgParser.GetValueInt(argList, 1);
		if (ChapterIndex == tableChapterIndex && DungeonIndex == tableDungeonIndex)
		{
			return true;
		}
		return false;
	}
}
