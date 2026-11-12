using System;
using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class EnterHeroStatAwakenPage : IEvent
{
	public HeroAwakeType AwakeType { get; set; }

	bool IEvent.Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		tableChapterIndex = 0;
		tableDungeonIndex = 0;
		string valueString = ArgParser.GetValueString(argList, 0);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			return true;
		}
		return string.Equals(AwakeType.ToString(), valueString, StringComparison.OrdinalIgnoreCase);
	}
}
