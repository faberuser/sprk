using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureStarStatData : IDataKeySelector<CreatureStarStatDataKey>
{
	CreatureStarStatDataKey IDataKeySelector<CreatureStarStatDataKey>.Key => new CreatureStarStatDataKey(Star, Transcended, StatType);

	[Key(0)]
	public int Star { get; set; }

	[Key(1)]
	public int Transcended { get; set; }

	[Key(2)]
	public StatType StatType { get; set; }

	[Key(3)]
	public int StatFactor { get; set; }

	public override string ToString()
	{
		return "Star=" + Star + " Transcended=" + Transcended + " StatType=" + StatType;
	}
}
