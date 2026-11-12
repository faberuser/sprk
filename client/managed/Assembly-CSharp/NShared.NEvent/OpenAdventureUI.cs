using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OpenAdventureUI : BaseNodeEvent
{
	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return true;
	}
}
