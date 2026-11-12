namespace NShared.SetEclipseDeck;

public enum ResultType
{
	Fail,
	Success,
	ChapterNotFound,
	DungeonNotFound,
	[Comment("영웅을 찾을수 없습니다.")]
	HeroNotFound
}
