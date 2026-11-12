namespace NShared.UnsetEquip;

public enum ResultType
{
	Fail,
	Success,
	HeroNotFound,
	EquipItemNotFound,
	UnequipFailed,
	RemoveFailed,
	NotEnoughGem,
	[Comment("파견중인 영웅입니다.")]
	AlreadyOnBattleHero
}
