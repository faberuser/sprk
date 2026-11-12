namespace NShared.EventExchangeEquip;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보유하지 않은 장비입니다.")]
	EquipNotOwned,
	[Comment("장비 아이템의 게임데이터가 존재하지 않습니다.")]
	EquipDataNotFound,
	[Comment("착용중인 장비입니다.")]
	Equipped,
	[Comment("잠금상태인 장비입니다.")]
	LockedEquipItemExists,
	[Comment("아이템의 게임데이터가 존재하지 않습니다.")]
	ItemDataNotFound,
	[Comment("소울웨폰은 분해 할 수 없습니다.")]
	SoulWeaponCanNotBreak
}
