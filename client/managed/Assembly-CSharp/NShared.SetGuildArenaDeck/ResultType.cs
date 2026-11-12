namespace NShared.SetGuildArenaDeck;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("진행중이지 않은 길드전 시즌입니다.")]
	InvalidGuildArenaSeasonIndex,
	[Comment("신청기간이 아니여서 방어덱을 세팅할 수 없습니다.")]
	GuildArenaNotApplySetDeck,
	[Comment("방어덱 지정에 영웅을 요청하지 않았습니다.")]
	GuildArenaDeckHeroNotFound,
	[Comment("영웅을 보유하고 있지 않습니다.")]
	GuildArenaHeroNotFound,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound,
	[Comment("가입된 길드가 없습니다.")]
	NoGuild,
	[Comment("길드 회원정보를 찾을 수 없습니다.")]
	GuildMemberNotFound
}
