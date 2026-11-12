namespace NShared.SoulWeaponUpgrade;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	[Comment("소울웨폰 정보가 존재하지 않습니다.")]
	SoulWeaponInfoNotFound,
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
	[Comment("소울웨폰 승급 재료의 게임데이터가 존재하지 않습니다.")]
	SoulWeaponUpgradePriceDataNotFound,
	[Comment("소울웨폰 게임데이터의 재료 갯수 정보가 잘못 되었습니다.")]
	MaterialCountError,
	[Comment("재료아이템의 게임데이터가 존재하지 않습니다.")]
	MaterialItemDataNotFound,
	[Comment("이미 최대 승급단계 입니다.")]
	AlreadyMaxGrade,
	[Comment("소울웨폰 승급 재료가 부족합니다.")]
	NotEnoughMaterial,
	[Comment("소울웨폰으로 해방되지 않은 상태 입니다.")]
	IsNotSoulWeaponState,
	[Comment("소울웨폰의 강화수치가 승급조건 보다 낮습니다.")]
	NotEnoughLevel,
	[Comment("골드가 부족합니다.")]
	NotEnoughGold,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero
}
