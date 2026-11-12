namespace NShared.RenewEquipOption;

public enum ResultType
{
	Fail,
	Success,
	EquipNotOwned,
	OptionSlotMismatch,
	InvalidOptionSlot,
	ItemDataNotFound,
	EquipDataNotFound,
	[Comment("장비의 등급정보가 존재하지 않습니다.")]
	EquipGradeDataNotFound,
	[Comment("장비의 상세정보가 존재하지 않습니다.")]
	EquipDetailDataNotFound,
	[Comment("잘못된 쿠폰을 사용하였습니다.")]
	CouponMismatch,
	[Comment("쿠폰 개수가 충분하지 않습니다.")]
	NotEnoughCouponItem,
	[Comment("쿠폰 정보가 올바르지 않습니다.")]
	CouponItemDataNotFound,
	[Comment("쿠폰 정보가 올바르지 않습니다.")]
	InvalidCouponItemType,
	EmptyPrice,
	InvaidPrice,
	[Comment("잘못된 재화 정보입니다.")]
	InvalidPriceType,
	NotEnoughGem,
	[Comment("골드가 부족합니다.")]
	NotEnoughGold,
	NoAvailableOption,
	[Comment("재료 아이템이 부족합니다.")]
	NotEnoughItem
}
