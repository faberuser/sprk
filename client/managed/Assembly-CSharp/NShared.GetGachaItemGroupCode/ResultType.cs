namespace NShared.GetGachaItemGroupCode;

public enum ResultType
{
	Fail,
	Success,
	GachaDataNotFound,
	[Comment("확률 테이블을 로딩할 수 없습니다.")]
	GachaItemGroupCodeDataNotFound
}
