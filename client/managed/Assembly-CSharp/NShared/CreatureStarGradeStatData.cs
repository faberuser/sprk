using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public sealed class CreatureStarGradeStatData : IDataKeySelector<CreatureStarGradeStatKey>
{
	[Key(0)]
	public int CreatureIndex { get; set; }

	[Key(1)]
	public byte Star { get; set; }

	[Key(2)]
	public byte Transcended { get; set; }

	[Key(3)]
	public int LevelStatFactor { get; set; }

	[Key(4)]
	public int LookupTableIndex { get; set; }

	CreatureStarGradeStatKey IDataKeySelector<CreatureStarGradeStatKey>.Key => new CreatureStarGradeStatKey(CreatureIndex, Star, Transcended);

	public override string ToString()
	{
		return "CreatureIndex=" + CreatureIndex + " Star=" + Star + " Enhanced=" + Transcended;
	}

	public long GetStat(StatType statType)
	{
		if (GameRoot.Instance.CreatureStarGradeStatDataContainer.GetStatLookup(LookupTableIndex).TryGetValue(statType, out var value))
		{
			return value;
		}
		return 0L;
	}
}
