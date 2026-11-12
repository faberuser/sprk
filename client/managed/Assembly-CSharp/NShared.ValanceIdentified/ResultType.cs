namespace NShared.ValanceIdentified;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 값으로 요청하였습니다.")]
	InvalidValue,
	EquipNotOwned,
	[Comment("감정 불가능한 장비입니다.")]
	InvaildEquipItem,
	[Comment("이미 감정된 장비입니다.")]
	AlreadyIdentified,
	NotEnoughGold
}
