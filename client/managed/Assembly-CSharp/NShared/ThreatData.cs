using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ThreatData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public CreatureTagType ClassType { get; set; }

	[Key(2)]
	public ThreatType ThreatType { get; set; }

	[Key(3)]
	public int Multiplier { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
