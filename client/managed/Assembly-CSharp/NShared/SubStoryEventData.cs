using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SubStoryEventData : IDataKeySelector<SubStoryEventKey>
{
	SubStoryEventKey IDataKeySelector<SubStoryEventKey>.Key => new SubStoryEventKey(Index, Sequence);

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Sequence { get; set; }

	[Key(2)]
	public int ReqIndex { get; set; }

	[Key(3)]
	public string Command { get; set; }

	[Key(4)]
	public string[] Argument { get; set; }

	public override string ToString()
	{
		return "[" + Index + "] = " + Command;
	}
}
