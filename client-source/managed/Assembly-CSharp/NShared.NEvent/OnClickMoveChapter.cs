using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OnClickMoveChapter : BaseNodeEvent
{
	public int ChapterIdx { get; set; }

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		if (!base.Check(argList, out tableChapterIndex, out tableDungeonIndex))
		{
			return false;
		}
		return true;
	}
}
