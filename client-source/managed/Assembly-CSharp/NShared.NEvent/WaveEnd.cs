using System.Collections.Generic;

namespace NShared.NEvent;

[JsonSerializable]
public class WaveEnd : BaseNodeEvent
{
	public int WaveIndex { get; set; }

	public override bool Check(IList<string> argList, out int tableChapterIndex, out int tableDungeonIndex)
	{
		if (!base.Check(argList, out tableChapterIndex, out tableDungeonIndex))
		{
			return false;
		}
		return ArgParser.GetValueInt(argList, 2) == WaveIndex;
	}
}
