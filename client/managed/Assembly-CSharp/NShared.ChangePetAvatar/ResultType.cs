namespace NShared.ChangePetAvatar;

public enum ResultType
{
	Fail,
	Success,
	[Comment("보유하지 않은 펫입니다.")]
	NotHavePet
}
