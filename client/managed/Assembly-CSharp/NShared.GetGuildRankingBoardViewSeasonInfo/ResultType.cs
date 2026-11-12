namespace NShared.GetGuildRankingBoardViewSeasonInfo;

public enum ResultType
{
	Fail,
	Success,
	[Comment("알수없는 보드타입 입니다.")]
	UnknownType,
	[Comment("랭킹 보드판의 랭킹타입과 요청 타입이 다릅니다.")]
	NotMatchType
}
