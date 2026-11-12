namespace NShared.PetUpgradeTier;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 입력값입니다.")]
	InvalidValue,
	[Comment("보유하지 않은 펫정수입니다.")]
	NotHavePetSoul,
	NotEnoughGold,
	NotEnoughItem,
	InvalidItemData
}
