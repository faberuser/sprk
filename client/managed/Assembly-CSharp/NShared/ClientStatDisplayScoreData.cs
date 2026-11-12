using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ClientStatDisplayScoreData : IDataKeySelector<StatType>
{
	[Key(0)]
	public StatType StatType { get; set; }

	[Key(1)]
	public CreatureTagType[] VisibleCreatureTag { get; set; }

	[Key(2)]
	public int EquipItemVisibleGroup { get; set; }

	[Key(3)]
	public bool ShowMultedStatOnly { get; set; }

	[Key(4)]
	public int ShowOptionIndex { get; set; }

	[Key(5)]
	public int DisplayScore { get; set; }

	StatType IDataKeySelector<StatType>.Key => StatType;
}
