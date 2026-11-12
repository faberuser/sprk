namespace NShared.HeroLimitBreakLevelUp;

public enum ResultType
{
	Fail,
	Success,
	[Comment("영웅이 존재하지 않습니다.")]
	NoHero,
	[Comment("최대레벨에 도달하지 않았습니다.")]
	NotMaxLevel,
	[Comment("최대각성에 도달하지 않았습니다.")]
	NotMaxStar,
	[Comment("최대초월에 도달하지 않았습니다.")]
	NotMaxTranscended,
	[Comment("한계돌파 데이터가 존재하지 않습니다.")]
	NoHeroLimitBreakData,
	[Comment("한계돌파 재료아이템 데이터가 존재하지 않습니다.")]
	InvalidMaterialItemData,
	[Comment("한계돌파 재료아이템이 충분하지 않습니다.")]
	NotEnoughMaterialItem
}
