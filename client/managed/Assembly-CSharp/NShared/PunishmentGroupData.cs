using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentGroupData : IDataKeySelector<PunishmentGroupDataKey>
{
	PunishmentGroupDataKey IDataKeySelector<PunishmentGroupDataKey>.Key => new PunishmentGroupDataKey(GroupIndex, PunishmentDungeonType);

	[Key(0)]
	public int GroupIndex { get; set; }

	[Key(1)]
	public EPunishmentDungeonType PunishmentDungeonType { get; set; }

	[Key(2)]
	public int ChapterIndex { get; set; }

	[Key(3)]
	public int KarmaRaidIndex { get; set; }

	[Key(4)]
	public int OpenStaminaCount { get; set; }

	[Key(5)]
	public int[] StaminaIncreaseCount { get; set; }

	[Key(6)]
	public int StaminaIncreaseRate { get; set; }

	[Key(7)]
	public string NameKey { get; set; }

	[Key(8)]
	public string DescKey { get; set; }

	[Key(9)]
	public string ImagePath { get; set; }

	[Key(10)]
	public string[] BackgroundPaths { get; set; }

	[Key(11)]
	public string NoticeInfoKey { get; set; }

	[Key(12)]
	public int AffixGroupIndex { get; set; }

	[Key(13)]
	public int InfinityRewardGroupIndex { get; set; }

	[Key(14)]
	public int CreatureIndex { get; set; }

	[Key(15)]
	public int[] CreaturePosition { get; set; }

	[Key(16)]
	public int[] CreatureRotation { get; set; }

	[Key(17)]
	public int CreatureScale { get; set; }

	[Key(18)]
	public int KarmaFixRewardIndex { get; set; }

	[Key(19)]
	public int KarmaBonusRewardIndex { get; set; }

	[Key(20)]
	public string BossGuidanceKey { get; set; }

	[Key(21)]
	public string RewardDescKey { get; set; }

	[Key(22)]
	public int InitialShardPoint { get; set; }

	[Key(23)]
	public int KarmaCardPickInterval { get; set; }

	[Key(24)]
	public string BossRaidBgm { get; set; }
}
