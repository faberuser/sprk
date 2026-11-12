namespace NShared.RewardEventRoulette;

public enum ResultType
{
	Fail,
	Success,
	ItemNotOwned,
	ItemDataNotFound,
	InvalidItemCount,
	[Comment("마녀의 교환소 정보를 찾을 수 없습니다.")]
	RouletteDataNotFound,
	[Comment("마녀의 교환소 교환 횟수 - 잘못된 입력값입니다.")]
	NotCorrectRouletteCount,
	[Comment("이벤트 진행 조건이 없습니다.")]
	MissingBonusEventCondition,
	[Comment("재화가 충분하지 않습니다.")]
	NotEnoughCurrency
}
