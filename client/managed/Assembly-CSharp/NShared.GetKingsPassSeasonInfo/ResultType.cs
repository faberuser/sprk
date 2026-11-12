namespace NShared.GetKingsPassSeasonInfo;

public enum ResultType
{
	Fail,
	Success,
	[Comment("진행중인 시즌이 없습니다.")]
	NoCurrentKingsPassSeason
}
