namespace NShared.SaveCostumeStorageSlot;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 값으로 요청하였습니다.")]
	InvalidValue,
	[Comment("해당 영웅을 보유하고 있지 않습니다.")]
	NotHaveHero
}
