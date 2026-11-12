using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class EnterChapter : BaseNodeEvent
{
	public bool IsEnabledAutoNext { get; set; }

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		if (ArgParser.GetValueInt(argList, 0) == base.ChapterIndex && !IsEnabledAutoNext)
		{
			return true;
		}
		return false;
	}
}
