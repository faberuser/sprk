namespace NShared.GiveUpEclipseDungeon;

public enum ResultType
{
	Fail,
	Success,
	[Comment("일치하는 던전정보가 없습니다.")]
	EclipseDungeonNotFound
}
