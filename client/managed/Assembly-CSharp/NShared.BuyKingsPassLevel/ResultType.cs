namespace NShared.BuyKingsPassLevel;

public enum ResultType
{
	Fail,
	Success,
	[Comment("진행중인 시즌이 없습니다.")]
	NoCurrentKingsPassSeason,
	[Comment("진행중인 시즌이 아닙니다.")]
	NotInProgressKingsPassSeason,
	InvalidReqLevel,
	NotFoundKingsPassSeasonData,
	NotFoundKingsPassLevelData,
	AlreadyHigherLevel,
	InvalidLevelUpPrice,
	NotEnoughGem
}
