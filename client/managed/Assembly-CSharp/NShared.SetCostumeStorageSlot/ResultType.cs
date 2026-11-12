namespace NShared.SetCostumeStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 값으로 요청하였습니다.")]
	InvalidValue,
	[Comment("해당 영웅을 보유하고 있지 않습니다.")]
	NotHaveHero,
	[Comment("착용을 원하는 코스튬 저장 슬롯이 비어있습니다.")]
	EmptySorageSlot
}
