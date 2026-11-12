namespace NShared.SelectTier;

public enum ResultType
{
	Fail,
	Success,
	InvalidTier,
	[Comment("시련의 전장 - 시즌정보를 찾을 수 없습니다.")]
	NotFoundSeasonData,
	[Comment("진행중인 시즌정보가 없어서 시련의 전장을 입장할 수 없습니다.")]
	CannotOpenOrdeal,
	[Comment("진행중인 시련의 전장을 찾을 수 없습니다.")]
	NotFoundOrdealNodeInfo,
	InternalServerError,
	ContentsDisabled
}
