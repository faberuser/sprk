namespace NShared.SoulWeaponConfirmRenewOption;

public enum ResultType
{
	Fail,
	Success,
	ContentsDisabled,
	[Comment("필수 요청 값이 비어있습니다.")]
	RequiredValueEmpty,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	[Comment("소울웨폰 정보가 존재하지 않습니다.")]
	SoulWeaponInfoNotFound,
	[Comment("옵션 비율 재조정을 실행하지 않은 아이템입니다.")]
	NoRenewOptionRatio,
	[Comment("아이템의 게임데이터가 존재하지 않습니다.")]
	ItemDataNotFound,
	[Comment("전용무기를 착용 할 수 있는 영웅을 보유하고 있지 않습니다.")]
	HeroNotFound,
	[Comment("장비 아이템의 게임데이터가 존재하지 않습니다.")]
	EquipDataNotFound,
	[Comment("소울웨폰 기능이 활성화된 장비가 아닙니다.")]
	IsNotOpenSoulWeapon,
	[Comment("전용무기가 아닙니다.")]
	IsNotUniqueWeapon,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("소모 루비 데이터가 유효하지 않습니다.")]
	InvaidPrice,
	[Comment("옵션 비율 선택 값이 유효하지 않습니다.")]
	InvalidIsNew,
	[Comment("옵션 수치값 계산값이 서버와 불일치 합니다.")]
	SoulWeaponOptionStatNotMatchded
}
