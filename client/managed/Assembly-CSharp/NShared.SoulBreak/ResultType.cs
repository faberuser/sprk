namespace NShared.SoulBreak;

public enum ResultType
{
	Fail,
	Success,
	[Comment("착용중인 장비는 영혼 해제를 할수없습니다.")]
	Equipped,
	[Comment("소울웨폰 정보가 존재하지 않습니다.")]
	SoulWeaponInfoNotFound,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	[Comment("아이템의 게임데이터가 존재하지 않습니다.")]
	ItemDataNotFound,
	[Comment("장비 아이템의 게임데이터가 존재하지 않습니다.")]
	EquipDataNotFound,
	[Comment("소울웨폰 기능이 활성화된 장비가 아닙니다.")]
	IsNotOpenSoulWeapon,
	[Comment("장비 아이템의 게임데이터가 존재하지 않습니다.")]
	IsNotUniqueWeapon,
	[Comment("전용무기를 착용 할 수 있는 영웅을 보유하고 있지 않습니다.")]
	HeroNotFound,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("잠금상태인 장비입니다.")]
	LockedEquipItemExists
}
