namespace NShared.OrdealInfo;

public enum ResultType
{
	Fail,
	Success,
	[Comment("시련의 전장 - 시즌정보를 찾을 수 없습니다.")]
	NotFoundSeasonData,
	MatchingDisabled,
	[Comment("진행중인 시련의 전장을 찾을 수 없습니다.")]
	NotFoundOrdealNodeInfo,
	InternalServerError,
	ContentsDisabled
}
