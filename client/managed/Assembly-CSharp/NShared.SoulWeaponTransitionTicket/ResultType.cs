namespace NShared.SoulWeaponTransitionTicket;

public enum ResultType
{
	Fail,
	Success,
	InvalidTicket,
	InvalidItemCount,
	CantUsableEquipItem,
	[Comment("장비 아이템이 잠겨 있습니다.")]
	LockedEquipItemExists,
	[Comment("파견중인 영웅이 착용중인 장비입니다.")]
	AlreadyOnBattleHero,
	[Comment("소울웨폰이 아닌 장비로 전이 할 수 없습니다.")]
	SourceEquipIsNotSoulWeapon,
	[Comment("이미 같은 종류의 소울웨폰이 존재 합니다.")]
	AlreadyHasSameSoulWeapon,
	[Comment("전이 대상 장비의 승급 수치가 더 높습니다.")]
	TargetEquipHigherGrade,
	[Comment("장비 아이템의 게임데이터를 찾을 수 없습니다.")]
	EquipDataNotFound,
	[Comment("장비를 착용 할 수 있는 영웅을 보유하지 않았습니다.")]
	HeroNotFound
}
