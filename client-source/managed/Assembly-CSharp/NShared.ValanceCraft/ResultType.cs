namespace NShared.ValanceCraft;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 값으로 요청하였습니다.")]
	InvalidValue,
	NotEnoughGold,
	NotEnoughMaterial,
	EquipNotOwned,
	AlreadyIdentified,
	EquipDataNotFound,
	EquipItemFull,
	SelectEquipOptionCountOver,
	NotPossibleSelcetEquipOption
}
