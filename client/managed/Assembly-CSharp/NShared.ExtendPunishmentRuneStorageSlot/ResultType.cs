namespace NShared.ExtendPunishmentRuneStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	[Comment("ReqGem 필드가 비었거나 음수일 때")]
	ReqGemError,
	[Comment("룬 확장을 위한 루비가 부족할 때")]
	NotEnoughGem,
	[Comment("확장 데이터가 테이블에 없을 때.")]
	ExtendDataNotFound,
	[Comment("가방 확장 최대 횟수를 초과 했을 때.")]
	MaxExtendCountExceeded,
	[Comment("정확한 양의 루비가 아닐 때.")]
	InvalidAmountOfGem
}
