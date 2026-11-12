using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildRaidDungeonData : IDataKeySelector<GuildRaidDungeonDataKey>, IRaidData
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int Step { get; set; }

	[Key(3)]
	public int BonusDungeonRate { get; set; }

	[Key(4)]
	public int KillRewardIndex { get; set; }

	[Key(5)]
	public int KillActivityPoint { get; set; }

	[Key(6)]
	public int BuyableRewardIndex { get; set; }

	[Key(7)]
	public string Tactic { get; set; }

	[Key(8)]
	public int[] RecommendedHeroIndex { get; set; }

	[Key(9)]
	public string PortraitPath { get; set; }

	[Key(10)]
	public string PreviewBg { get; set; }

	[Key(11)]
	public long FinishModeRatio { get; set; }

	[Key(12)]
	public int[] ShowInfoSkill { get; set; }

	GuildRaidDungeonDataKey IDataKeySelector<GuildRaidDungeonDataKey>.Key => new GuildRaidDungeonDataKey(ChapterIndex, DungeonIndex, Step);

	int IRaidData.Difficulty => 1;

	int IRaidData.RaidMultiInfoGroupIndex => 0;

	[IgnoreMember]
	public DungeonDifficultyType Difficulty => DungeonDifficultyType.Normal;

	public override string ToString()
	{
		return "GuildRaidChapterIndex=" + ChapterIndex;
	}
}
