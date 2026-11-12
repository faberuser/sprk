namespace NShared.SelectBuff;

public enum ResultType
{
	Fail,
	Success,
	CannotSelectableBuff,
	[Comment("시련의 전장 - 시즌정보를 찾을 수 없습니다.")]
	NotFoundSeasonData,
	[Comment("시련의 전장 - 오픈되지 않은 시즌입니다.")]
	MatchingDisabled,
	[Comment("선택할 수 있는 버프를 찾을 수 없습니다.")]
	NotFoundSelectBuff,
	[Comment("선택할 수 있는 버프를 찾을 수 없습니다.")]
	NotAvailableSelectBuff,
	[Comment("선택할수 있는 버프의 최대 개수를 초과하였습니다.")]
	OverValueSelectBuffMaxCount,
	InternalServerError,
	ContentsDisabled
}
