using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class BattleSpeedChange : IEvent
{
	public float BattleGlobalTime { get; set; }

	public float SpeedRate { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		return false;
	}
}
