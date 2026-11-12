using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class MatchSeasonData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public string Begin { get; set; }

	[Key(3)]
	public string End { get; set; }

	[Key(4)]
	public string Description { get; set; }

	[Key(5)]
	public int SeasonIndex { get; set; }

	[Key(6)]
	public MatchArenaType ArenaType { get; set; }

	[Key(7)]
	public MatchSeasonType MatchSeasonType { get; set; }

	[Key(8)]
	public bool IsTakeReward { get; set; }

	[Key(9)]
	public string PlayTimeBegin { get; set; }

	[Key(10)]
	public string PlayTimeEnd { get; set; }

	[Key(11)]
	public int LimitedHeroStar { get; set; }

	[Key(12)]
	public int LimitedHeroLevel { get; set; }

	[Key(13)]
	public int LimitedHeroCount { get; set; }

	[Key(14)]
	public bool ActiveTier { get; set; }

	[Key(15)]
	public int ShowSeasonIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index + " (" + Name + ")";
	}
}
