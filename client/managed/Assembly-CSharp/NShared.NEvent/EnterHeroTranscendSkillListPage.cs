using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class EnterHeroTranscendSkillListPage : IEvent
{
	public bool IsMine { get; set; }

	public bool HasPoint { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		if (ArgParser.GetValueBool(argList, 1) == HasPoint)
		{
			return IsMine;
		}
		return false;
	}
}
