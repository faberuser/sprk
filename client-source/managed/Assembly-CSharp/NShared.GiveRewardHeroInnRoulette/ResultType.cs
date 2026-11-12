namespace NShared.GiveRewardHeroInnRoulette;

public enum ResultType
{
	Fail,
	Success,
	[Comment("더 이상 룰렛을 돌릴 수 없습니다.")]
	NoMoreRoulette,
	AlreadyRewarded,
	InvalidRewardData
}
