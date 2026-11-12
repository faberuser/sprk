using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GuildBuildingData : IDataKeySelector<GuildBuildingDataKey>
{
	GuildBuildingDataKey IDataKeySelector<GuildBuildingDataKey>.Key => new GuildBuildingDataKey(Index, Level);

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Level { get; set; }

	[Key(2)]
	public string ModelName { get; set; }

	[Key(3)]
	public bool CanUpgrade { get; set; }

	[Key(4)]
	public string IdleAnimationKey { get; set; }

	[Key(5)]
	public string UpgradeAnimationKey { get; set; }

	[Key(6)]
	public int ReqGuildLevel { get; set; }

	[Key(7)]
	public int ReqWood { get; set; }

	[Key(8)]
	public int ReqStone { get; set; }

	[Key(9)]
	public int ReqMetal { get; set; }

	[Key(10)]
	public int ShopIndex { get; set; }

	[Key(11)]
	public int GuildRaidRewardIndex { get; set; }

	[Key(12)]
	public int[] SkillIndex { get; set; }

	[Key(13)]
	public string NameKey { get; set; }

	[Key(14)]
	public string DescKey { get; set; }

	[Key(15)]
	public string IconPath { get; set; }

	[Key(16)]
	public string PortraitPath { get; set; }

	[Key(17)]
	public bool ControlHeadQuarter { get; set; }
}
