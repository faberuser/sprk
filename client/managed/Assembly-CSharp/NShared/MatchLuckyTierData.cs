using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MatchLuckyTierData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public LuckyMatchTierType Tier { get; set; }

	[Key(2)]
	public int Level { get; set; }

	[Key(3)]
	public int MinScore { get; set; }

	[Key(4)]
	public int MaxScore { get; set; }

	[Key(5)]
	public float MinPercent { get; set; }

	[Key(6)]
	public float MaxPercent { get; set; }

	[Key(7)]
	public int MinRank { get; set; }

	[Key(8)]
	public int MaxRank { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index + " (" + Tier.ToString() + " " + Level + ")";
	}
}
