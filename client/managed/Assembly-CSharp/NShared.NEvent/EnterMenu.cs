using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class EnterMenu : IEvent
{
	public string Window { get; set; }

	public int SceneTriggerSequance { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		string valueString = ArgParser.GetValueString(argList, 0);
		int valueInt = ArgParser.GetValueInt(argList, 1);
		if (valueInt != 0 && valueInt != SceneTriggerSequance)
		{
			return false;
		}
		return string.Equals(Window, valueString);
	}
}
