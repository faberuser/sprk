namespace NShared.ChangeCountryCode;

public enum ResultType
{
	Fail,
	Success,
	[Comment("존재하지 않는 국가코드 입니다.")]
	InvalidCountryCodeIndex
}
