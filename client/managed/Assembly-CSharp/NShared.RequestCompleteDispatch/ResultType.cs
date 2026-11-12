namespace NShared.RequestCompleteDispatch;

public enum ResultType
{
	Fail,
	Success,
	NotComplete,
	[Comment("파견단정보를 찾을 수 없습니다.")]
	DispatchBattleNotFound,
	[Comment("이미 완료된 상태입니다.")]
	AlreadyFinishDispatchBattle,
	[Comment("던전정보를 찾을 수 없습니다.")]
	DungeonNotFound,
	[Comment("재화가 충분하지 않습니다.")]
	NotEnoughCurrency
}
