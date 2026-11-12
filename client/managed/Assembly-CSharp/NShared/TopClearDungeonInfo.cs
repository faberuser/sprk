namespace NShared;

[JsonSerializable(Javascript = true)]
public class TopClearDungeonInfo : IAccountAttrKey<SweepDungeonType>
{
	SweepDungeonType IAccountAttrKey<SweepDungeonType>.Key => SweepDungeonType;

	public SweepDungeonType SweepDungeonType { get; set; }

	public int DungeonIndex { get; set; }
}
