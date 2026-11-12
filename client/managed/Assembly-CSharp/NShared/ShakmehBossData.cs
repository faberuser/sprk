using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShakmehBossData : IDataKeySelector<ChapterDungeonKey2>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public ShakmehBossType BossType { get; set; }

	[Key(3)]
	public int[] SequenceSkillProjectileGroup { get; set; }

	[Key(4)]
	public string BossNameKey { get; set; }

	[Key(5)]
	public string DungeonNameKey { get; set; }

	[Key(6)]
	public string DungeonPrefixKey { get; set; }

	[Key(7)]
	public string TribeKey { get; set; }

	[Key(8)]
	public string TacticKey { get; set; }

	[Key(9)]
	public int ShakmehIndex { get; set; }

	[Key(10)]
	public int[] RecommendedHero { get; set; }

	[Key(11)]
	public string BgNodeTreeCode { get; set; }

	[Key(12)]
	public string BgNodeTreePath { get; set; }

	[Key(13)]
	public string PreviewNodeTreePath { get; set; }

	[Key(14)]
	public string PreviewNodeTreeCode { get; set; }

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);
}
