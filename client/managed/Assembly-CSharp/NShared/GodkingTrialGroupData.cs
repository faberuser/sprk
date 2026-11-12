using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GodkingTrialGroupData : IDataKeySelector<int>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public GodkingTrialDungeonGateType GateType { get; set; }

	[Key(2)]
	public string DungeonNameKey { get; set; }

	[Key(3)]
	public string DungeonNamePrefixKey { get; set; }

	[Key(4)]
	public string BossTitleNamePrefixKey { get; set; }

	[Key(5)]
	public int BanRuleIndex { get; set; }

	[Key(6)]
	public string BgNodeTreeCode { get; set; }

	[Key(7)]
	public string BgNodeTreePath { get; set; }

	[Key(8)]
	public string PreviewShadowNodeTreePath { get; set; }

	[Key(9)]
	public string PreviewShadowNodeTreeCode { get; set; }

	[Key(10)]
	public string PreviewNodeTreePath { get; set; }

	[Key(11)]
	public string PreviewNodeTreeCode { get; set; }

	[Key(12)]
	public string BossDescription { get; set; }

	[Key(13)]
	public int[] RecommendedHero { get; set; }

	[Key(14)]
	public SkillAttrType[] GimicSkillAttrType { get; set; }

	[Key(15)]
	public string PreviewBg { get; set; }

	[Key(16)]
	public string Tactic { get; set; }

	[Key(17)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => ChapterIndex;
}
