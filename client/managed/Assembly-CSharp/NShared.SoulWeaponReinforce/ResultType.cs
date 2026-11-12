namespace NShared.SoulWeaponReinforce;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	[Comment("주입 재료 요청 정보가 잘못되었습니다.")]
	InjectionItemInfosError,
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
	[Comment("재료아이템의 게임데이터가 존재하지 않습니다.")]
	MaterialItemDataNotFound,
	[Comment("에테르 게임데이터가 존재하지 않습니다.")]
	SoulWeaponEtherDataNotFound,
	[Comment("소울웨폰 강화 게임데이터가 존재하지 않습니다.")]
	SoulWeaponReinforceDataNotFound,
	[Comment("에테르가 100% 상태가 아닙니다.")]
	NotMaxExp,
	[Comment("이미 최고레벨 상태입니다.")]
	AlreadyMaxLevel,
	[Comment("골드가 부족합니다.")]
	NotEnoughGold,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("최소 강화 확률 상태가 아닙니다.")]
	ReinforceRatioIsLessMinValue
}
