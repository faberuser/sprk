namespace NShared.ChangePetLayout;

public enum ResultType
{
	Fail,
	Success,
	[Comment("잘못된 입력값입니다.")]
	InvalidValue,
	[Comment("보유하지 않은 펫입니다.")]
	NotHavePet
}
