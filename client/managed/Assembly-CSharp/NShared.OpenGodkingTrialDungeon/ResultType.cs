namespace NShared.OpenGodkingTrialDungeon;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 챕터 번호입니다.")]
	InvalidChapterIndex,
	[Comment("신왕의 시련 던전을 찾을 수 없습니다.")]
	GodkingTrialDungeonNotFound,
	[Comment("이미 던전이 열려 있습니다.")]
	AlreadyOpened,
	[Comment("신왕의 시련 입장 재화가 부족합니다.")]
	NotEnoughGodkingTrialKey,
	[Comment("아직 던전을 닫을 수 없습니다.")]
	NotWeekElapsed,
	ContentsDisabled
}
