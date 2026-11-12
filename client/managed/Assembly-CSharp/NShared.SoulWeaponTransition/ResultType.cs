namespace NShared.SoulWeaponTransition;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	[Comment("전이 될 장비와 전이 할 장비가 같은 장비입니다.")]
	CanNotTransitionSameEquipItem,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	[Comment("동일한 장비가 아닙니다.")]
	NotMatchedItem,
	[Comment("소울웨폰이 아닌 장비는 전이가 불가능합니다.")]
	TransitionItemIsNotSoulWeapon,
	[Comment("아이템의 게임데이터가 존재하지 않습니다.")]
	ItemDataNotFound,
	[Comment("장비 아이템의 게임데이터가 존재하지 않습니다.")]
	EquipDataNotFound,
	[Comment("전용무기가 아닙니다.")]
	IsNotUniqueWeapon,
	[Comment("소울웨폰 기능이 활성화된 장비가 아닙니다.")]
	IsNotOpenSoulWeapon,
	[Comment("전용무기를 착용 할 수 있는 영웅을 보유하고 있지 않습니다.")]
	HeroNotFound,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("골드가 부족합니다.")]
	NotEnoughGold
}
