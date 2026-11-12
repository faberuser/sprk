namespace NShared.SoulWeaponRenewOption;

public enum ResultType
{
	Fail,
	Success,
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
	[Comment("루비가 부족합니다.")]
	NotEnoughGem,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("옵션 비율 재분배를 실패하였습니다.")]
	RenewOptionRaioFail,
	[Comment("지정된 쿠폰이 아닙니다.")]
	CouponMismatch,
	[Comment("쿠폰을 보유량이 부족합니다.")]
	NotEnoughCouponItem,
	[Comment("쿠폰의 아이템 데이터가 존재하지 않습니다.")]
	CouponItemDataNotFound,
	[Comment("쿠폰 타입이 맞지 않습니다.")]
	InvalidCouponItemType,
	[Comment("소모 루비 정보가 존재하지 않습니다.")]
	EmptyPriceInfo,
	[Comment("소모 루비 데이터가 유효하지 않습니다.")]
	InvaidPrice,
	[Comment("옵션 수치값 계산값이 서버와 불일치 합니다.")]
	SoulWeaponOptionStatNotMatchded
}
