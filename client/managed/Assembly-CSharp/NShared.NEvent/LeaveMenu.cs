using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class LeaveMenu : IEvent
{
	public string Window { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		string valueString = ArgParser.GetValueString(argList, 0);
		return string.Equals(Window, valueString);
	}
}
