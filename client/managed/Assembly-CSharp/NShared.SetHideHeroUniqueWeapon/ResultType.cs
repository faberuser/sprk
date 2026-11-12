namespace NShared.SetHideHeroUniqueWeapon;

public enum ResultType
{
	Fail,
	Success,
	InvalidHeroIndex,
	[Comment("보유하지 않은 무기 코스튬입니다.")]
	NotHaveWeaponCostume
}
