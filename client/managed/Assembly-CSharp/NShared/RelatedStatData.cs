using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class RelatedStatData : IDataKeySelector<StatType>
{
	[Key(0)]
	public StatType CurrentStatType { get; set; }

	[Key(1)]
	public StatType RelatedStatType { get; set; }

	[Key(2)]
	public OperationType ChangedStatType { get; set; }

	StatType IDataKeySelector<StatType>.Key => CurrentStatType;
}
