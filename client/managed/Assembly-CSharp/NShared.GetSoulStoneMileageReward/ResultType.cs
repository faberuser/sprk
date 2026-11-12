namespace NShared.GetSoulStoneMileageReward;

public enum ResultType
{
	Fail,
	Success,
	NotMaxMileage,
	[Comment("영혼석 감정 게임데티어를 찾을 수 없습니다.")]
	RestoreDataNotFound
}
