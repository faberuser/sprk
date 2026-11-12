namespace NShared.CheckGuildName;

public enum ResultType
{
	Fail,
	Success,
	EmptyName,
	InvalidName,
	AlreadyExistName,
	TooShortName,
	TooLongName,
	CensoredWord,
	SensitiveWord,
	AlreadyExistGuildName,
	[Comment("이름을 생성하는데 유효한 이름이 아닙니다.")]
	InvalidGuildNameInThisServer
}
