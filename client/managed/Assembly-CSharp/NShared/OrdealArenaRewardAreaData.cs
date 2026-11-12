using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class OrdealArenaRewardAreaData : IDataKeySelector<OrdealArenaRewardAreaDataKey>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public MatchTierType TierType { get; set; }

	[Key(2)]
	public int RewardIndex { get; set; }

	OrdealArenaRewardAreaDataKey IDataKeySelector<OrdealArenaRewardAreaDataKey>.Key => new OrdealArenaRewardAreaDataKey(Index, TierType);
}
