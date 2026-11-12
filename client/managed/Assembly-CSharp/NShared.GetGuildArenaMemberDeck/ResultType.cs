namespace NShared.GetGuildArenaMemberDeck;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("잘못된 세션정보를 요청하였습니다.")]
	InvalidGuildArenaSessionIndex,
	[Comment("잘못된 시즌정보를 요청하였습니다.")]
	InvalidGuildArenaSeasonIndex,
	[Comment("길드 정보가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("길드에 가입한 상태가 아닙니다.")]
	GuildMemberNotFound,
	[Comment("본인의 매칭티어 정보를 찾을 수 없습니다.")]
	MatchTierNotFound,
	[Comment("시즌 점수 정보를 찾을 수 없습니다.")]
	NotFoundSeasonScoreInfo,
	[Comment("길드원이 존재하지 않습니다.")]
	GuildMembersEmpty
}
