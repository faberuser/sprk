namespace NShared.ResetEventDungeon;

public enum ResultType
{
	Fail,
	Success,
	PlayerDungeonNotFound,
	DungeonNotPlayed,
	DungeonPlayEnable,
	NotEnoughGem,
	NoEventDungeonData,
	NoResetCount,
	NoResetCurrencyType,
	NoResetAmount,
	NotEnoughEventDungeonPoint,
	NotEnoughEventDungeonPoint2,
	[Comment("입장 횟수 초기화가 불가능한 던전 입니다.")]
	CannotBeReset
}
