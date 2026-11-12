namespace NShared.AwakenTransition;

public enum ResultType
{
	Fail,
	Success,
	ItemDataNotFound,
	InvalidItemCount,
	InvalidTicket,
	EquipDataNotFound,
	CantUsableEquipItem,
	[Comment("장비 아이템이 잠겨 있습니다.")]
	LockedEquipItemExists,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("장비를 착용 할 수 있는 영웅을 보유하지 않았습니다.")]
	HeroNotFound
}
