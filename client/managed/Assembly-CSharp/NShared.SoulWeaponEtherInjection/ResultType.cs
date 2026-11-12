namespace NShared.SoulWeaponEtherInjection;

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
	[Comment("소울웨폰 기능이 활성화된 장비가 아닙니다.")]
	IsNotOpenSoulWeapon,
	[Comment("전용무기가 아닙니다.")]
	IsNotUniqueWeapon,
	[Comment("전용무기를 착용 할 수 있는 영웅을 보유하고 있지 않습니다.")]
	HeroNotFound,
	[Comment("재료아이템의 게임데이터가 존재하지 않습니다.")]
	MaterialItemDataNotFound,
	[Comment("사용 할수 없는 재료 아이템입니다.")]
	UnavailableMaterialItem,
	[Comment("에테르 게임데이터가 존재하지 않습니다.")]
	SoulWeaponEtherDataNotFound,
	[Comment("소울웨폰 강화 게임데이터가 존재하지 않습니다.")]
	SoulWeaponReinforceDataNotFound,
	[Comment("이미 최고레벨 상태입니다.")]
	AlreadyMaxLevel,
	[Comment("이미 에테르 주입 100% 상태입니다.")]
	AlreadyMaxExp,
	[Comment("골드가 부족합니다.")]
	NotEnoughGold,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("에테르가 부족합니다.")]
	NotEnoughEther,
	[Comment("강화 성공 확률이 100%를 초과 한 상태입니다.")]
	AlreadyMaxReinforceRatio
}
