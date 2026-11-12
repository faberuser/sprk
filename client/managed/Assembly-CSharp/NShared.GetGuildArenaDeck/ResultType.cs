namespace NShared.GetGuildArenaDeck;

public enum ResultType
{
	Fail,
	Success,
	[Comment("길드전이 진행중이 아닙니다.")]
	GuildArenaSessionNotFound,
	[Comment("가입된 길드가 없습니다.")]
	NoGuild,
	[Comment("가입된 길드가 존재하지 않습니다.")]
	GuildIdNotFound
}
