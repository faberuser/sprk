using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class OnSkillFire : IEvent
{
	public int SkillId { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		int valueInt = ArgParser.GetValueInt(argList, 0);
		return SkillId == valueInt;
	}
}
