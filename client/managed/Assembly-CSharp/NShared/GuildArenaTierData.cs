using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GuildArenaTierData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public GuildArenaTierType Tier { get; set; }

	[Key(2)]
	public bool InTopTier { get; set; }

	[Key(3)]
	public int Level { get; set; }

	[Key(4)]
	public int MinScore { get; set; }

	[Key(5)]
	public int MaxScore { get; set; }

	[Key(6)]
	public int SoftResetScore { get; set; }

	[Key(7)]
	public int SoftResetTierIndex { get; set; }

	[Key(8)]
	public int MinRank { get; set; }

	[Key(9)]
	public int MaxRank { get; set; }

	[Key(10)]
	public int MatchScoreRate { get; set; }

	[Key(11)]
	public int DefenseDeckCount { get; set; }

	[Key(12)]
	public int AttackCount { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index + " (" + Tier.ToString() + " " + Level + ")";
	}
}
