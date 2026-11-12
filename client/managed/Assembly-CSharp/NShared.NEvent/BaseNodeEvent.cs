using System.Collections.Generic;

namespace NShared.NEvent;

public abstract class BaseNodeEvent : IEvent
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public bool SkipTrigger { get; set; }

	public BaseNodeEvent()
	{
	}

	public void ParseNodePosition(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = ArgParser.GetValueInt(argList, 0);
		tableDungeonIndex = ArgParser.GetValueInt(argList, 1);
	}

	public virtual bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		ParseNodePosition(argList, out tableChapterIndex, out tableDungeonIndex);
		if (SkipTrigger)
		{
			return false;
		}
		if (tableChapterIndex != 0 && tableChapterIndex != ChapterIndex)
		{
			return false;
		}
		if (tableDungeonIndex != 0 && tableDungeonIndex != DungeonIndex)
		{
			return false;
		}
		return true;
	}
}
