using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventWorldBossSeasonData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string PlayTimeBegin { get; set; }

	[Key(2)]
	public string PlayTimeEnd { get; set; }

	[Key(3)]
	public bool Immortal { get; set; }

	[Key(4)]
	public int DefaultWorldBossLevel { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
