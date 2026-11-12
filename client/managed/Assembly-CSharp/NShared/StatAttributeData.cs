using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StatAttributeData : IDataKeySelector<StatType>
{
	[Key(0)]
	public StatType StatType { get; set; }

	[Key(1)]
	public bool Positive { get; set; }

	[Key(2)]
	public bool Negative { get; set; }

	[Key(3)]
	public bool Percent { get; set; }

	[Key(4)]
	public bool AllowNegative { get; set; }

	StatType IDataKeySelector<StatType>.Key => StatType;
}
