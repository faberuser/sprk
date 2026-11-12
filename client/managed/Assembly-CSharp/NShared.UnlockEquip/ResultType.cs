namespace NShared.UnlockEquip;

public enum ResultType
{
	Fail,
	Success,
	NoHero,
	HeroDataNotFound,
	InvalidHeroPartIndex,
	AlreadyUnlocked,
	LowHeroLevel,
	PriceError,
	CostTypeError,
	NotEnoughGold,
	NotEnoughGem,
	UnlockFailed
}
