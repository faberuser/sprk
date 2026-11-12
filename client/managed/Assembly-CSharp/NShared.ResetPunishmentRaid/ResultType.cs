namespace NShared.ResetPunishmentRaid;

public enum ResultType
{
	Fail,
	Success,
	[Comment("이미 던전이 열려 있습니다.")]
	AlreadyOpened,
	[Comment("입장 재화가 부족합니다.")]
	NotEnoughKey,
	ContentsDisabled,
	CannotPlayDungeon,
	NotOpenedDungeon
}
