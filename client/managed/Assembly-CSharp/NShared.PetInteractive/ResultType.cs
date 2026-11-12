namespace NShared.PetInteractive;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보유하지 않은 펫입니다.")]
	NotHavePet,
	[Comment("이미 완료된 상호작용입니다.")]
	AlreadyAction,
	[Comment("행복도가 최대입니다.")]
	MaxHappinessPoint,
	[Comment("먹이가 충분하지 않습니다.")]
	NotEnoughFeed,
	[Comment("펫이 잠들어 있는 상태입니다.")]
	Sleepping,
	[Comment("행복도가 최대치 이상입니다.")]
	Overflow,
	ExceedWood
}
