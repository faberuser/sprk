namespace NShared.QueryNick;

public enum ResultType
{
	Fail,
	Success,
	EmptyNick,
	InvalidNick,
	AlreadyExist,
	TooShortNick,
	TooLongNick,
	CensoredWord,
	[Comment("이름을 생성하는데 유효한 이름이 아닙니다.")]
	InvalidNickInThisServer,
	SensitiveWord,
	[Comment("이름을 변경할 수 없습니다.")]
	NickChangeDisabled
}
