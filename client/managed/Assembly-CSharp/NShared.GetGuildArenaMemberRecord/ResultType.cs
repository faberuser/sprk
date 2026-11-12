namespace NShared.GetGuildArenaMemberRecord;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("진행중이지 않은 길드전입니다.")]
	InvalidGuildArenaSessionIndex,
	[Comment("길드전 상태가 전투중이 아닙니다.")]
	GuildArenaNotBattleState,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("길드 회원정보를 찾을 수 없습니다.")]
	GuildMemberNotFound,
	[Comment("매칭된 적을 찾을 수 없습니다.")]
	MatchedGuildArenaEnemyNotFound,
	[Comment("전적정보를 받을 수 없습니다.")]
	GuildArenaMemberRecordNotFound
}
