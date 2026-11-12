using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OpenTechnoEnchantRaid : IEvent
{
	public bool IsActive { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return true;
	}
}
