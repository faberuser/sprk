using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ChallengeRaidData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int RaidIndex { get; set; }

	[Key(2)]
	public int ShopIndex { get; set; }

	[Key(3)]
	public RaidRewardType RewardType { get; set; }

	[Key(4)]
	public string RaidSelectImage { get; set; }

	[Key(5)]
	public string RankingBossImage { get; set; }

	[Key(6)]
	public string GuideBattle { get; set; }

	[Key(7)]
	public string GuideRanking { get; set; }

	[Key(8)]
	public string RoomTitle { get; set; }

	[Key(9)]
	public bool ShowEnterButtonEventImage { get; set; }

	[Key(10)]
	public string PortalImagePath { get; set; }
}
