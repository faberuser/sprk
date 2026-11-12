using System.Collections.Generic;
using NShared.NEvent;

public static class IEvent_Extension_Method
{
	public static bool Check(this IEvent This, IList<string> argList)
	{
		int tableChapterIndex = 0;
		int tableDungeonIndex = 0;
		return This.Check(argList, out tableChapterIndex, out tableDungeonIndex);
	}
}
