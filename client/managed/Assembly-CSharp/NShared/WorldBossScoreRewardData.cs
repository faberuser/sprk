using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class WorldBossScoreRewardData : IDataKeySelector<WorldBossScoreRewardDataKey>
{
	[Key(0)]
	public int WorldBossIndex { get; set; }

	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public long MinScore { get; set; }

	[Key(3)]
	public long MaxScore { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public int WorldBossPoint { get; set; }

	WorldBossScoreRewardDataKey IDataKeySelector<WorldBossScoreRewardDataKey>.Key => new WorldBossScoreRewardDataKey(WorldBossIndex, Index);

	public override string ToString()
	{
		return "[Index=" + Index + "]";
	}
}
