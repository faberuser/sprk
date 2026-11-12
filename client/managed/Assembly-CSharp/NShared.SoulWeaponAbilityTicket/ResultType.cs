namespace NShared.SoulWeaponAbilityTicket;

public enum ResultType
{
	Fail,
	Success,
	[Comment("소유하고 있지 않은 아이템 입니다.")]
	ItemNotOwned,
	[Comment("아이템의 게임데이터가 존재하지 않습니다.")]
	ItemDataNotFound,
	InvalidTicket,
	[Comment("게임데이터가 존재하지 않습니다.")]
	SoulWeaponAbilityTicketDataNotFound,
	[Comment("아이템을 사용할 수 없는 장비입니다.")]
	NotAvailableEquipItemindex,
	[Comment("장비 아이템의 게임데이터가 존재하지 않습니다.")]
	EquipDataNotFound,
	[Comment("전용무기가 아닙니다.")]
	IsNotUniqueWeapon,
	[Comment("장비 아이템이 잠겨 있습니다.")]
	LockedEquipItemExists,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("이미 해방된 소울웨폰을 보유중 입니다.")]
	IsAlreadyLiberation,
	[Comment("유효하지 않은 티켓 타입입니다.")]
	InvalidTicketType,
	[Comment("현재 승급 수치와 같거나 더 높은 상태입니다.")]
	AlreadySameOrGreaterGrade,
	[Comment("소울웨폰 정보가 존재하지 않습니다.")]
	SoulWeaponInfoNotFound,
	[Comment("현재 강화 수치와 같거나 더 높은 상태입니다.")]
	AlreadySameOrGreaterReinforceLevel,
	[Comment("현재 승급 수치와 강화 수치가 같거나 더 높은 상태입니다.")]
	AlreadySameOrGreater
}
