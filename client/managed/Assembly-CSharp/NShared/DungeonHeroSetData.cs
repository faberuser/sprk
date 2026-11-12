using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class DungeonHeroSetData : IDataKeySelector<DungeonHeroSetKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public DungeonDifficultyType Difficulty { get; set; }

	[Key(3)]
	public int HeroListSetDataIndex { get; set; }

	[Key(4)]
	public bool CommonUniqueWeapon { get; set; }

	[Key(5)]
	public int CommonHeroStar { get; set; }

	[Key(6)]
	public int CommonHeroLevel { get; set; }

	[Key(7)]
	public int CommonHeroTranscended { get; set; }

	[Key(8)]
	public int CommonHeroTier { get; set; }

	[Key(9)]
	public int[] CommonHeroAddExtraSkill { get; set; }

	[Key(10)]
	public bool IsScenario { get; set; }

	DungeonHeroSetKey IDataKeySelector<DungeonHeroSetKey>.Key => new DungeonHeroSetKey(ChapterIndex, DungeonIndex, Difficulty);

	public override string ToString()
	{
		return "DungeonSetHero ChapterIndex=" + ChapterIndex + ", DungeonIndex=" + DungeonIndex + ", Diffculty=" + Difficulty;
	}
}
